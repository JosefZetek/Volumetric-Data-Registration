using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    class RotationComputerVector
    {

        public static double radius = 1;

        /// <summary>
        /// Calculates the rotation matrix from d1 to d2
        /// </summary>
        /// <param name="dMicro">Data1</param>
        /// <param name="dMacro">Data2</param>
        /// <param name="pointMicro">Point in data1 to take samples around</param>
        /// <param name="pointMacro">Point in data2 to take samples around</param>
        /// <param name="count">Number of samples taken</param>
        /// <returns></returns>
        public static Matrix<double> CalculateRotation(AData dMicro, AData dMacro, Point3D pointMicro, Point3D pointMacro, int count)
        {
            Random mainRnd = new Random();

            Vector<double> vectorMicro = GetDirectionVector(dMicro, pointMicro, count, radius, mainRnd);
            Vector<double> vectorMacro = GetDirectionVector(dMacro, pointMacro, count, radius, mainRnd);
            

            Matrix<double> matrixMacro = Matrix<double>.Build.Dense(1, 3);
            matrixMacro[0, 0] = vectorMacro[0];
            matrixMacro[0, 1] = vectorMacro[1];
            matrixMacro[0, 2] = vectorMacro[2];

            Matrix<double> matrixMicro = Matrix<double>.Build.Dense(1, 3);
            matrixMicro[0, 0] = vectorMicro[0];
            matrixMicro[0, 1] = vectorMicro[1];
            matrixMicro[0, 2] = vectorMicro[2];



            Vector<double> testVector1Normalized = NormalizeVector(vectorMicro);
            Vector<double> testVector2Normalized = NormalizeVector(vectorMacro);

            Vector<double> crossProduct = Vector<double>.Build.Dense(3);
            if(ParallelVectors(vectorMacro, vectorMicro))
                crossProduct = findOrthogonalVector(vectorMicro);
            else
            {
                Matrix<double> tempCrossProduct = CrossProduct(testVector1Normalized, testVector2Normalized);
                crossProduct = Vector<double>.Build.DenseOfArray(new double[] { tempCrossProduct[0, 0], tempCrossProduct[0, 1], tempCrossProduct[0, 2] });
            }

            crossProduct = NormalizeVector(crossProduct);

            double cosine = ScalarProduct(testVector1Normalized, testVector2Normalized);
            cosine = Constrain(cosine, -1, 1);
            double sine = Math.Sqrt(1 - Math.Pow(cosine, 2));

            //Derived from Rodrigue's formula - https://mathworld.wolfram.com/RodriguesRotationFormula.html

            Matrix<double> rotationMatrix = Matrix<double>.Build.Dense(3, 3);
            rotationMatrix[0, 0] = cosine + (Math.Pow(crossProduct[0], 2) * (1 - cosine));
            rotationMatrix[0, 1] = (crossProduct[0] * crossProduct[1] * (1 - cosine)) - (crossProduct[2] * sine);
            rotationMatrix[0, 2] = (crossProduct[1] * sine) + (crossProduct[0] * crossProduct[2] * (1 - cosine));
            rotationMatrix[1, 0] = (crossProduct[2] * sine) + (crossProduct[0] * crossProduct[1] * (1 - cosine));
            rotationMatrix[1, 1] = cosine + (Math.Pow(crossProduct[1], 2) * (1 - cosine));
            rotationMatrix[1, 2] = -(crossProduct[0] * sine) + (crossProduct[1] * crossProduct[2] * (1 - cosine));
            rotationMatrix[2, 0] = -(crossProduct[1] * sine) + (crossProduct[0] * crossProduct[2] * (1 - cosine));
            rotationMatrix[2, 1] = (crossProduct[0] * sine) + (crossProduct[1] * crossProduct[2] * (1 - cosine));
            rotationMatrix[2, 2] = cosine + (Math.Pow(crossProduct[2], 2) * (1 - cosine));

            /*
            Console.WriteLine("This is the non scaled result:");
            Console.WriteLine(matrixMacro * rotationMatrix); //aligning macro vector to micro
            */

            Matrix<double> nonScaledMatrix = matrixMacro * rotationMatrix;

            if (nonScaledMatrix.ColumnCount != 3)
                throw new ArgumentException("Matrix doesnt have 3 columns");

            if (nonScaledMatrix.RowCount != 1)
                throw new ArgumentException("Matrix doesnt have 1 row");


            Matrix<double> scalingMatrix = Matrix<double>.Build.Dense(3, 3);
            for (int i = 0; i < 3; i++)
                scalingMatrix[i, i] = vectorMicro[i] / nonScaledMatrix[0, i];

            /*
            Console.WriteLine("This is a scaling matrix");
            Console.WriteLine(scalingMatrix);
            */
            rotationMatrix *= scalingMatrix;

            /*
            Console.WriteLine("This is the final result: ");
            Console.WriteLine(matrixMacro * rotationMatrix); //aligning macro vector to micro
            */

            return rotationMatrix;
        }

        /// <summary>
        /// This method tests whether the given vectors are parallel or antiparallel.
        /// Both of the passed vectors are expected to have dimension 3
        /// </summary>
        /// <param name="vectorA">First vector</param>
        /// <param name="vectorB">Second vector</param>
        /// <returns>Returns true if vectors are parallel or antiparallel, otherwise false.</returns>
        /// <exception cref="ArgumentException">Exception when the given arguments</exception>
        /// 
        private static bool ParallelVectors(Vector<double> vectorA, Vector<double> vectorB)
        {
            if (vectorA.Count != 3)
                throw new ArgumentException("VectorA is expected to have dimension 3");
            if (vectorB.Count != 3)
                throw new ArgumentException("VectorB is expected to have dimension 3");
            if (vectorA.Equals(Vector<double>.Build.Dense(3)))
                throw new ArgumentException("VectorA has only zeros.");
            if(vectorB.Equals(Vector<double>.Build.Dense(3)))
                throw new ArgumentException("VectorB has only zeros.");

            double scalingFactor = vectorA[0] / vectorB[0];
            for(int i = 1; i<vectorA.Count; i++)
            {
                double scaledVector = vectorB[i] * scalingFactor;
                if (compareWithTolerance(scaledVector, vectorA[i]) || compareWithTolerance(scaledVector, -vectorA[i]))
                    return true;
            }
            return false;
        }

        private static bool compareWithTolerance(double numberA, double numberB)
        {
            double epsilon = 0.00000001;
            if ((numberA + epsilon > numberB) && (numberA - epsilon < numberB))
                return true;
            
            return false;
        }

        private static Vector<double> findOrthogonalVector(Vector<double> inputVector)
        {
            if (inputVector.Count != 3)
                throw new ArgumentException("Vector needs to have dimension 3");

            Vector<double> orthogonalVector = Vector<double>.Build.Dense(3);
            double a1 = 1;
            double b1 = 1;

            orthogonalVector[0] = a1;
            orthogonalVector[1] = b1;
            orthogonalVector[2] = (inputVector[0] * a1 + inputVector[1] * b1) / (-inputVector[2]);

            return orthogonalVector;
        }

        /// <summary>
        /// This method makes sure value is within bounds
        /// </summary>
        /// <param name="currentValue">Value to be constrained</param>
        /// <param name="minimumValue">Minimum value</param>
        /// <param name="maxiumValue">Maximum value</param>
        /// <returns>Returns constrained value</returns>
        private static double Constrain(double currentValue, double minimumValue, double maxiumValue)
        {
            if (currentValue > maxiumValue)
                return maxiumValue;

            if (currentValue < minimumValue)
                return minimumValue;

            return currentValue;
        }

        private static Vector<double> NormalizeVector(Vector<double> vector) {

            double magnitude = vector.L2Norm();

            //Prevents from crashing when denominator(magnitude) is equal to 0
            if (magnitude == 0)
                return Vector<double>.Build.Dense(3, 0);

            return vector / magnitude;
        }

        /// <summary>
        /// ForTesting
        /// Computes the crossProduct of vectors size 3
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        private static Matrix<double> CrossProduct(Vector<double> u, Vector<double> v)
        {
            Matrix<double> w = Matrix<double>.Build.Dense(1,3);
            w[0, 0] = u[1] * v[2] - v[1] * u[2];
            w[0, 1] = v[0] * u[2] - u[0] * v[2];
            w[0, 2] = u[0] * v[1] - v[0] * u[1];
            return w;
        }

        private static double ScalarProduct(Vector<double> vector1, Vector<double> vector2)
        {
            int upperBound = vector1.AsArray().Length;

            if (upperBound != vector2.AsArray().Length)
                throw new ArgumentException("Vectors should be the same size");

            double result = 0;
            for(int i = 0; i<upperBound; i++)
                result += vector1[i] * vector2[i];

            return result;
        }

        // od pana V�i
        private static Vector<double> GetDirectionVector(AData d, Point3D point, int count, double radius, Random rnd)
        {
            List<Point3D> pointsInSphere = GetSphere(point, radius, count, rnd.Next()); 

            Matrix<double> result = Matrix<double>.Build.Dense(3, 3);

            double[] values = new double[count];
            double min = double.MaxValue;
            double max = double.MinValue;

            Point3D wAvg = new Point3D(0, 0, 0);
            double ws = 0;

            for (int i = 0; i < count; i++)
            {
                values[i] = d.GetValue(pointsInSphere[i]);

                min = Math.Min(min, values[i]);
                max = Math.Max(max, values[i]);
            }

            for (int i = 0; i < count; i++)
            {

                double w = (values[i] - min) / (max - min); //percentage from the overall range
                ws += w;

                wAvg.X += pointsInSphere[i].X * w;
                wAvg.Y += pointsInSphere[i].Y * w;
                wAvg.Z += pointsInSphere[i].Z * w;
            }


            wAvg.X /= ws;
            wAvg.Y /= ws;
            wAvg.Z /= ws;

            double diffX = wAvg.X - point.X;
            double diffY = wAvg.Y - point.Y;
            double diffZ = wAvg.Z - point.Z;

            Vector<double> resultVector = Vector<double>.Build.Dense(3);

            resultVector[0] = diffX;
            resultVector[1] = diffY;
            resultVector[2] = diffZ;

            return resultVector;
        }

        private static List<Point3D> GetSphere(Point3D p, double r, int count, int seed)
        {
            List<Point3D> points = new List<Point3D>();

            Random rnd = new Random(seed);
            double rSquared = r * r;
            do
            {
                double xCoordinate = GetRandomDouble(p.X - r, p.X + r, rnd);
                double yCoordinate = GetRandomDouble(p.Y - r, p.Y + r, rnd);
                double zCoordinate = GetRandomDouble(p.Z - r, p.Z + r, rnd);

                double distance = (xCoordinate - p.X) * (xCoordinate - p.X) + (yCoordinate - p.Y) * (yCoordinate - p.Y) + (zCoordinate - p.Z) * (zCoordinate - p.Z);
                if (distance <= rSquared)
                {
                    points.Add(new Point3D(xCoordinate, yCoordinate, zCoordinate));
                }
            } while (
                points.Count < count
            );

            return points;
        }

        private static double GetRandomDouble(double minimum, double maximum, Random r)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}