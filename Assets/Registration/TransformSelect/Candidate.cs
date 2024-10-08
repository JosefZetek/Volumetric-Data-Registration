using System;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class Candidate
    {
        private Transform3D originalTranform;

        // properties of Q        
        static double[] tensorProductsSum; // sum of tensor products, diagonal only
        static double dotProductsSum; // sum of dot products
        static double numberOfPoints; // number of points
        static double inverseNumberOfPoints; // inverse number of points        
        static Vector<double> centerPoint; //Center point of the whole object

        double[] rotation; // rotation matrix
        double[] translation; // translation

        public Transform3D getOriginalTransform()
        {
            return originalTranform;
        }

        /// <summary>
        /// initializes rotation and translation from dual quaternion representation
        /// </summary>
        public void initRT()
        {
            this.rotation = new double[9];
            this.translation = new double[3];
        }

        public static void initSums(double xSize, double ySize, double zSize)
        {
            centerPoint = Vector<double>.Build.Dense(new double[] { xSize / 2, ySize / 2, zSize / 2 });

            tensorProductsSum = new double[3]; // we only need the diagonal 
            tensorProductsSum[0] = xSize * xSize * xSize * ySize * zSize / 12;
            tensorProductsSum[1] = xSize * ySize * ySize * ySize * zSize / 12;
            tensorProductsSum[2] = xSize * ySize * zSize * zSize * zSize / 12;

            dotProductsSum = tensorProductsSum[0] + tensorProductsSum[1] + tensorProductsSum[2];
            
            numberOfPoints = xSize*ySize*zSize;

            inverseNumberOfPoints = 1.0 / numberOfPoints;
            Density.radius = Math.Sqrt(xSize * xSize + ySize * ySize + zSize * zSize)/4;
        }

        internal Transform3D toTransform3D()
        {            
            Matrix<double> rotationMatrix = Matrix<double>.Build.Dense(3, 3);
            Vector<double> translationVector = Vector<double>.Build.Dense(3);

            rotationMatrix[0, 0] = rotation[0];
            rotationMatrix[0, 1] = rotation[1];
            rotationMatrix[0, 2] = rotation[2];
            rotationMatrix[1, 0] = rotation[3];
            rotationMatrix[1, 1] = rotation[4];
            rotationMatrix[1, 2] = rotation[5];
            rotationMatrix[2, 0] = rotation[6];
            rotationMatrix[2, 1] = rotation[7];
            rotationMatrix[2, 2] = rotation[8];
            var Rt0 = rotationMatrix * centerPoint;

            translationVector[0] = translation[0] - Rt0[0];
            translationVector[1] = translation[1] - Rt0[1];
            translationVector[2] = translation[2] - Rt0[2];

            return new Transform3D(rotationMatrix, translationVector);            
        }

        public Candidate(Transform3D tr)
        {
            this.originalTranform = tr;

            var Rt0 = tr.RotationMatrix * centerPoint;

            rotation = new double[9];
            translation = new double[3];

            rotation[0] = tr.RotationMatrix[0, 0];
            rotation[1] = tr.RotationMatrix[0, 1];
            rotation[2] = tr.RotationMatrix[0, 2];
            rotation[3] = tr.RotationMatrix[1, 0];
            rotation[4] = tr.RotationMatrix[1, 1];
            rotation[5] = tr.RotationMatrix[1, 2];
            rotation[6] = tr.RotationMatrix[2, 0];
            rotation[7] = tr.RotationMatrix[2, 1];
            rotation[8] = tr.RotationMatrix[2, 2];

            translation[0] = tr.TranslationVector[0] + Rt0[0];
            translation[1] = tr.TranslationVector[1] + Rt0[1];
            translation[2] = tr.TranslationVector[2] + Rt0[2];
        }

        public double SqrtDistanceTo(Candidate target)
        {
            double d = DistanceTo(target);
            if (d < 0)
                d = 0;
            return Math.Sqrt(d);
        }


        /// <summary>
        /// Calculates transition matrix between the two passed matrices
        /// Then it subtracts 1 where i==j
        /// Finally finds maximum number from the transition matrix and returns it
        /// </summary>
        /// <param name="target"></param>
        /// <returns>Passed rotation matrices have wrong dimensions</returns>
        public double AlternativeRotationMatrixDistance(Candidate target)
        {
            Matrix<double> thisRotationMatrix = this.originalTranform.RotationMatrix;
            Matrix<double> targetRotationMatrix = target.originalTranform.RotationMatrix;

            Matrix<double> transitionMatrix = Matrix<double>.Build.Dense(3, 3);

            Matrix<double> equationMatrix = Matrix<double>.Build.Dense(3, 4);

            //Calculation of the transition matrix

            for (int basisNumber = 0; basisNumber < 3; basisNumber++)
            {
                for (int i = 0; i < 3; i++)
                    equationMatrix.SetColumn(i, thisRotationMatrix.Column(i));

                equationMatrix.SetColumn(3, targetRotationMatrix.Column(basisNumber));

                Console.WriteLine("This is the equation matrix: " + equationMatrix);
                try
                {
                    Vector<double> result = EquationComputer.CalculateSolution(equationMatrix);
                    transitionMatrix.SetColumn(basisNumber, result);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }


            //Subtract 1 from diagonal so that if the matrices were 100% same,
            //the transition would be matrix of zeros
            for (int i = 0; i < transitionMatrix.ColumnCount; i++)
                transitionMatrix[i, i] -= 1;

            double distance = 0;

            for(int i = 0; i<transitionMatrix.RowCount; i++)
            {
                for (int j = 0; j < transitionMatrix.ColumnCount; j++)
                {
                    distance = Math.Max(transitionMatrix[i,j], distance);
                }
            }

            return distance;
        }

        /// <summary>
        /// Finds transition matrix between passed matrices
        /// Subtracts 1 from the transition matrix where i==j
        /// Calculates frobenius norm and returns the number
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Passed rotation matrices have wrong dimensions</exception>
        public double FrobeniusRotationMatrixDistance(Candidate target)
        {

            Matrix<double> thisRotationMatrix = this.originalTranform.RotationMatrix;
            Matrix<double> targetRotationMatrix = target.originalTranform.RotationMatrix;

            if (thisRotationMatrix.ColumnCount != thisRotationMatrix.ColumnCount)
                throw new ArgumentException("Rotation matrix of this object should be n x n");
            if (thisRotationMatrix.ColumnCount == 0)
                throw new ArgumentException("Rotation matrix of this object should be n x n where n>0");

            if (targetRotationMatrix.ColumnCount != targetRotationMatrix.ColumnCount)
                throw new ArgumentException("Rotation matrix of passed should be n x n");
            if (targetRotationMatrix.ColumnCount == 0)
                throw new ArgumentException("Rotation matrix of passed object should be n x n where n>0");

            if (thisRotationMatrix.ColumnCount != targetRotationMatrix.ColumnCount)
                throw new ArgumentException("Both rotation matrices should have the same dimensions");

            /*
            Console.WriteLine("This is the original matrix: " + thisRotationMatrix);
            Console.WriteLine("This is the new matrix: " + targetRotationMatrix);
            */

            Matrix<double> transitionMatrix = Matrix<double>.Build.Dense(3, 3);

            Matrix<double> equationMatrix = Matrix<double>.Build.Dense(3, 4);

            //Calculation of the transition matrix

            for (int basisNumber = 0; basisNumber < 3; basisNumber++)
            {
                for (int i = 0; i < 3; i++)
                    equationMatrix.SetColumn(i, thisRotationMatrix.Column(i));

                equationMatrix.SetColumn(3, targetRotationMatrix.Column(basisNumber));

                //Console.WriteLine("This is the equation matrix: " + equationMatrix);
                try
                {
                    Vector<double> result = EquationComputer.CalculateSolution(equationMatrix);
                    transitionMatrix.SetColumn(basisNumber, result);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }


            //Subtract 1 from diagonal so that if the matrices were 100% same,
            //the transition would be matrix of zeros
            for (int i = 0; i < transitionMatrix.ColumnCount; i++)
                transitionMatrix[i, i] -= 1;

            return transitionMatrix.FrobeniusNorm();
            
        }

        /// <summary>
        /// This method works only when there is very small deviations between the correct rotations
        /// If there is a bigger deviation, it will need to be assigned a small weight but that can't be influenced by the user
        /// Finds transition matrix between passed matrices
        /// Subtracts 1 from the transition matrix where i==j
        /// All values are assigned a weight and then normalized to be between 0-127
        /// Finally there is a result variable with 64 bits where the numbers are "concantenated" as follows
        /// 0-6th bit = value with weight 0
        /// 7-13th bit = value with weight 1
        /// 14-20th bit = value with weight 2
        /// ...
        /// This value is then returned
        /// </summary>
        /// <param name="target">Target candidate</param>
        /// <returns>Returns the value where all normalized weights are "concantenated"</returns>
        /// <exception cref="ArgumentException">Passed rotation matrices have wrong dimensions</exception>
        public double RotationMatrixDistance(Candidate target)
        {

            Matrix<double> thisRotationMatrix = this.originalTranform.RotationMatrix;
            Matrix<double> targetRotationMatrix = target.originalTranform.RotationMatrix;

            if (thisRotationMatrix.ColumnCount != thisRotationMatrix.ColumnCount)
                throw new ArgumentException("Rotation matrix of this object should be n x n");
            if (thisRotationMatrix.ColumnCount == 0)
                throw new ArgumentException("Rotation matrix of this object should be n x n where n>0");

            if (targetRotationMatrix.ColumnCount != targetRotationMatrix.ColumnCount)
                throw new ArgumentException("Rotation matrix of passed should be n x n");
            if (targetRotationMatrix.ColumnCount == 0)
                throw new ArgumentException("Rotation matrix of passed object should be n x n where n>0");

            if (thisRotationMatrix.ColumnCount != targetRotationMatrix.ColumnCount)
                throw new ArgumentException("Both rotation matrices should have the same dimensions");


            Console.WriteLine("This is the original matrix: " + thisRotationMatrix);
            Console.WriteLine("This is the new matrix: " + targetRotationMatrix);

            Matrix<double> transitionMatrix = Matrix<double>.Build.Dense(3, 3);

            Matrix<double> equationMatrix = Matrix<double>.Build.Dense(3, 4);

            //Calculation of the transition matrix
            
            for (int basisNumber = 0; basisNumber < 3; basisNumber++)
            {
                for (int i = 0; i < 3; i++)
                    equationMatrix.SetColumn(i, thisRotationMatrix.Column(i));

                equationMatrix.SetColumn(3, targetRotationMatrix.Column(basisNumber));

                Console.WriteLine("This is the equation matrix: " + equationMatrix);
                try
                {
                    Vector<double> result = EquationComputer.CalculateSolution(equationMatrix);
                    transitionMatrix.SetColumn(basisNumber, result);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }


            //Subtract 1 from diagonal so that if the matrices were 100% same,
            //the transition would be matrix of zeros
            for (int i = 0; i < transitionMatrix.ColumnCount; i++)
                transitionMatrix[i, i] -= 1;

            //Convert all values to range 0-127
            transitionMatrix = normalizeMatrix(transitionMatrix);

            double resultNumber = 0;

            for(int i = 0; i<transitionMatrix.ColumnCount; i++)
            {
                for (int j = 0; j < transitionMatrix.RowCount; j++)
                {
                    int numberIndex = j * transitionMatrix.ColumnCount + i;
                    resultNumber += (int)transitionMatrix[j, i] << (7 * numberIndex);
                }
            }

            /*
            double resultSum = 0;
            
            for(int i = 0; i<transitionMatrix.ColumnCount; i++)
            {
                for(int j = 0; j<transitionMatrix.RowCount; j++)
                {
                    resultSum += transitionMatrix[j, i];
                }
            }
            */
            return resultNumber;
        }

        /// <summary>
        /// Calculates Euclidean norm of a difference between the two vectors
        /// </summary>
        /// <param name="target">Target candidate</param>
        /// <returns>Norm of a difference between the two translation vectors</returns>
        public double TranslationVectorDistance(Candidate target)
        {
            Vector<double> thisTranslationVector = this.originalTranform.TranslationVector;
            Vector<double> targetTranslationVector = target.originalTranform.TranslationVector;

            return (thisTranslationVector - targetTranslationVector).L2Norm();
        }

        private Matrix<double> normalizeMatrix(Matrix<double> matrix)
        {
            //Matrix has 9 values that need to be saved to 64 bits (double)
            //Since 64/9 ~ 7, the max number for 7 bits is 127

            const int maxRange = 127;

            Matrix<double> normalizedMatrix = Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount);

            double minValue = double.MaxValue;
            double maxValue = double.MinValue;

            //Finds the max and min values
            for(int i = 0; i<matrix.ColumnCount; i++)
            {
                for (int j = 0; j < matrix.RowCount; j++)
                {
                    minValue = Math.Min(minValue, matrix[j, i]);
                    maxValue = Math.Max(maxValue, matrix[j, i]);
                }
            }

            for (int i = 0; i < matrix.ColumnCount; i++)
            {
                for (int j = 0; j < matrix.RowCount; j++)
                {
                    double denominator = maxValue - minValue;

                    if (Math.Abs(denominator) < double.Epsilon)
                    {
                        normalizedMatrix[j, i] = 0;
                        continue;
                    }

                    normalizedMatrix[j, i] = (maxRange*(matrix[j, i] - minValue)) / (maxValue - minValue);
                }
            }

            return normalizedMatrix;
        }

        /// <summary>
        /// Caution! Only works if the mesh is centered and rotated correctly, otherwise use bfDistanceTo(...)!
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double DistanceTo(Candidate target)
        {
            double[] r1 = this.rotation;
            double[] r2 = target.rotation;
            double[] t1 = this.translation;
            double[] t2 = target.translation;


            double r1tr20 = r1[0] * r2[0] + r1[3] * r2[3] + r1[6] * r2[6];
            double r1tr21 = r1[1] * r2[1] + r1[4] * r2[4] + r1[7] * r2[7];
            double r1tr22 = r1[2] * r2[2] + r1[5] * r2[5] + r1[8] * r2[8];

            double t1t2 = t1[0] * t2[0] + t1[1] * t2[1] + t1[2] * t2[2];
            double t2t2 = t2[0] * t2[0] + t2[1] * t2[1] + t2[2] * t2[2];
            double t1t1 = t1[0] * t1[0] + t1[1] * t1[1] + t1[2] * t1[2];

            double result = 2 * (dotProductsSum) + numberOfPoints * (t2t2 + t1t1 - 2 * t1t2);

            result -= 2 * (r1tr20 * tensorProductsSum[0] + r1tr21 * tensorProductsSum[1] + r1tr22 * tensorProductsSum[2]);

            result *= inverseNumberOfPoints;

            return (result);
        }        
    }
}
