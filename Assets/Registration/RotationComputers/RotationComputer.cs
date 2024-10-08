using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class RotationComputer
    {
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

            Matrix<double> Amicro = GetSymetricMatrixForEigenVectors(dMicro, pointMicro, count, 3, mainRnd);
            var evdMicro = Amicro.Evd();

            Matrix<double> Amacro = GetSymetricMatrixForEigenVectors(dMacro, pointMacro, count, 3, mainRnd);
            var evdMacro = Amacro.Evd();

            Matrix<double> rotationMatrix = ComputeChangeOfBasisMatrixUsingTransposition(evdMicro.EigenVectors, evdMacro.EigenVectors, mainRnd); //eigenvectors make up an orthonormal basis    
            return rotationMatrix;
        }

        private static void TestChangeOfBasisMatrixCorrectness(Matrix<double> transformMatrix, Matrix<double> base1, Matrix<double> base2)
        {
            Vector<double> v1t = transformMatrix.Multiply(base1.Column(0));
            Vector<double> v2t = transformMatrix.Multiply(base1.Column(1));
            Vector<double> v3t = transformMatrix.Multiply(base1.Column(2));
            Console.Write("v1: " + base2.Column(0));
            Console.WriteLine("v1t: " + v1t.ToString());

            Console.Write("v2: " + base2.Column(1));
            Console.WriteLine("v2t: " + v2t.ToString());

            Console.Write("v3: " + base2.Column(2));
            Console.WriteLine("v3t: " + v3t.ToString());
        }

        /// <summary>
        /// PCA works as intended
        /// </summary>
        public void TestPCA()
        {
            PCATester pcatester = new PCATester(1000);
            Matrix<double> baseMatrix = GetSymetricMatrixForEigenVectors(pcatester.Qs);
            var evd = baseMatrix.Evd();
            Matrix<double> output = evd.EigenVectors;

            Console.WriteLine("Input vectors:");
            Console.WriteLine(String.Format("{0} {1} {2}", pcatester.v1[0], pcatester.v2[0], pcatester.v3[0]));
            Console.WriteLine(String.Format("{0} {1} {2}", pcatester.v1[1], pcatester.v2[1], pcatester.v3[1]));
            Console.WriteLine(String.Format("{0} {1} {2}", pcatester.v1[2], pcatester.v2[2], pcatester.v3[2]));

            Console.WriteLine("PCA output:");
            Console.WriteLine(output.ToMatrixString());
        }

        /// <summary>
        /// if v1 x v2 != v3
        /// then v3 = -v3
        /// </summary>
        /// <param name="matrix"></param>
        private static void MakeBaseRightHanded(Matrix<double> matrix)
        {
            if (IsBaseRightHanded(matrix))
                return;
            matrix.SetRow(2, matrix.Row(2).Multiply(-1));
        }

        /// <summary>
        /// Tests whether v1 x v2 = v3
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        private static bool IsBaseRightHanded(Matrix<double> matrix)
        {
            double epsilon = 0.000001;
            Vector<double> cross = CrossProduct(matrix.Column(0), matrix.Column(1));

            if ((Math.Abs(cross[0] - matrix.Column(2)[0]) < epsilon) && (Math.Abs(cross[1] - matrix.Column(2)[1]) < epsilon) && (Math.Abs(cross[2] - matrix.Column(2)[2]) < epsilon))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ForTesting
        /// Computes the crossProduct of vectors size 3
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        private static Vector<double> CrossProduct(Vector<double> u, Vector<double> v)
        {
            Vector<double> w = Vector<double>.Build.Dense(3);
            w[0] = u[1] * v[2] - v[1] * u[2];
            w[1] = v[0] * u[2] - u[0] * v[2];
            w[2] = u[0] * v[1] - v[0] * u[1];
            return w;
        }

        /// <summary>
        /// ForTesting
        /// Sets the norm of column vectors in a matrix to 1
        /// </summary>
        /// <param name="matrix"></param>
        private static void NormalizeCollumnVectorsInMatrix(Matrix<double> matrix)
        {
            for (int i = 0; i < matrix.ColumnCount; i++)
            {
                Vector<double> v = matrix.Column(i);
                v.Divide(Math.Sqrt(PCATester.ScalarProduct(v, v)));
                matrix.SetColumn(i, v);
            }
        }

        /// <summary>
        /// Computes the change of basis matrix for L: base1 -> base2
        /// 
        /// change of basis matrix A = (a1, a2, a3)
        /// a1 = base1^-1 * u1
        /// 
        /// [v]w = A * [v]u
        /// 
        /// https://eli.thegreenplace.net/2015/change-of-basis-in-linear-algebra/
        /// </summary>
        /// <param name="base1"></param>
        /// <param name="base2"></param>
        /// <returns></returns>
        private static Matrix<double> ComputeChangeOfBasisMatrix(Matrix<double> base1, Matrix<double> base2)
        {
            //NormalizeCollumnVectorsInMatrix(base1); //base1 and base2 are already orthonormal
            //NormalizeCollumnVectorsInMatrix(base2);

            MakeBaseRightHanded(base1);
            MakeBaseRightHanded(base2);

            if (!IsBaseRightHanded(base1))
                Console.WriteLine("base1 is not righthanded :( ");
            if (!IsBaseRightHanded(base2))
                Console.WriteLine("base2 is not righthanded :( ");

            Matrix<double> base2Inverse = base2.Inverse();

            Vector<double> u1 = base1.Column(0);
            Vector<double> a1 = base2Inverse.Multiply(u1);

            Vector<double> u2 = base1.Column(1);
            Vector<double> a2 = base2Inverse.Multiply(u2);

            Vector<double> u3 = base1.Column(2);
            Vector<double> a3 = base2Inverse.Multiply(u3);

            Matrix<double> changeOfBasisMatrix = Matrix<double>.Build.DenseOfColumnVectors(a1, a2, a3);

            return changeOfBasisMatrix;
        }

        private static Matrix<double> ComputeChangeOfBasisMatrixUsingTransposition(Matrix<double> baseMicro, Matrix<double> baseMacro, Random rnd)
        {
            // bases already ortonormal
            MakeBaseRightHanded(baseMicro);
            MakeBaseRightHanded(baseMacro);

            if (!IsBaseRightHanded(baseMicro))
                Console.WriteLine("base1 is not righthanded :( ");
            if (!IsBaseRightHanded(baseMacro))
                Console.WriteLine("base2 is not righthanded :( ");

            RandomFlipBasis(baseMacro, rnd);
            Matrix<double> changeOfBasisMatrix = baseMacro.Multiply(baseMicro.Transpose());

            return changeOfBasisMatrix;
        }

        private static void RandomFlipBasis(Matrix<double> basis, Random rnd)
        {
            Random lr = new Random(rnd.Next());
            if (lr.NextDouble() < 0.5)
            {
                FlipColumn(basis, 0);
                FlipColumn(basis, 1);
            }

            if (lr.NextDouble() < 0.5)
            {
                FlipColumn(basis, 1);
                FlipColumn(basis, 2);
            }

            if (lr.NextDouble() < 0.5)
            {
                FlipColumn(basis, 2);
                FlipColumn(basis, 0);
            }
        }

        private static void FlipColumn(Matrix<double> basis, int v)
        {
            basis[0, v] = -basis[0, v];
            basis[1, v] = -basis[1, v];
            basis[2, v] = -basis[2, v];
        }

        private static Point3D[] SampleSphereAroundPoint(AData d, Point3D centerPoint, int radius, int count, Random rnd)
        {
            List<Point3D> survivingPoints = new List<Point3D>();
            List<Point3D> pointsInSphere = GetSphere(centerPoint, radius, count, rnd.Next()); //gets all points in a given sphere

            Random rndL = new Random();
            double currentValue;
            double maxValue = 0;
            double minValue = int.MaxValue;

            foreach (Point3D point in pointsInSphere) //finds the minumum and maximum value in the sphere
            {
                currentValue = d.GetValue(point);

                if (currentValue > maxValue)
                    maxValue = currentValue;

                if (currentValue < minValue)
                    minValue = currentValue;
            }
            bool NonTrivial = true;
            if (maxValue == minValue)
                NonTrivial = false;
            while (survivingPoints.Count < count && pointsInSphere.Count > 0)
            {
                int rndIndex = rndL.Next(0, pointsInSphere.Count); //random index in pointsInSphere
                if (NonTrivial)
                {
                    if (DecideFate(rndL, d.GetValue(pointsInSphere[rndIndex]), minValue, maxValue)) //decides whether to keep the point or not
                    {
                        //the point is kept
                        survivingPoints.Add(pointsInSphere[rndIndex]); //add to result
                    }
                }
                else
                {
                    if (rndL.NextDouble() > 0.5)
                    {
                        survivingPoints.Add(pointsInSphere[rndIndex]); //add to result
                    }
                }
                pointsInSphere.RemoveAt(rndIndex); //removed from pointsInSphere
            }
            return survivingPoints.ToArray();
        }

        private static bool DecideFate(Random rnd, double pointValue, double min, double max)
        {
            double rndValue = GetRandomDouble(min, max, rnd);
            if (pointValue > rndValue)
                return true;
            else
                return false;
        }

        /// <summary>
        /// FUJKY METODA NEPOUZIVEJ
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static Matrix<double> GetSymetricMatrixForEigenVectors(Point3D[] points)
        {
            Matrix<double> sampleMatrix = Point3DArrayToMatrix(points); //matrix D
            double[] averageCoordinates = GetAverageCoordinate(sampleMatrix); // vector overline{x}
            Matrix<double> sampleMatrix1Subtracted = SubtractVectorFromMatrix(sampleMatrix, averageCoordinates); //matrix D*
            Matrix<double> A = sampleMatrix1Subtracted.TransposeAndMultiply(sampleMatrix1Subtracted); // D* times transpose(D*)
            return A;
        }

        /// <summary>
        /// Samples the volumetric data, 
        /// computes matrix D* = samples - average sample
        /// returns D* times transpose(D*)
        /// returned matrix is symetric and its eigenvectors are orthogonal
        /// </summary>
        /// <param name="d"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static Matrix<double> GetSymetricMatrixForEigenVectors(AData d, Point3D point, int count, Random rnd)
        {
            Point3D[] sample = SampleSphereAroundPoint(d, point, 3, count, rnd);

            Matrix<double> sampleMatrix = Point3DArrayToMatrix(sample); //matrix D
            double[] averageCoordinates = GetAverageCoordinate(sampleMatrix); // vector overline{x}

            Matrix<double> sampleMatrixSubtracted = SubtractVectorFromMatrix(sampleMatrix, averageCoordinates); //matrix D*
            Matrix<double> A = sampleMatrixSubtracted.TransposeAndMultiply(sampleMatrixSubtracted); // D* times transpose(D*)

            return A;
        }

        // od pana V�i
        private static Matrix<double> GetSymetricMatrixForEigenVectors(AData d, Point3D point, int count, double radius, Random rnd)
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

            for (int i = 0; i < count; i++)
            {
                Matrix<double> pnt = Matrix<double>.Build.Dense(3, 1);
                pnt[0, 0] = pointsInSphere[i].X - wAvg.X;
                pnt[1, 0] = pointsInSphere[i].Y - wAvg.Y;
                pnt[2, 0] = pointsInSphere[i].Z - wAvg.Z;

                result += (values[i] - min) / (max - min) * (pnt * pnt.Transpose());
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m">3*n matrix</param>
        /// <param name="v">vector of size 3</param>
        /// <returns></returns>
        private static Matrix<double> SubtractVectorFromMatrix(Matrix<double> m, double[] v)
        {
            Matrix<double> output = Matrix<double>.Build.Dense(3, m.ColumnCount);
            for (int i = 0; i < m.ColumnCount; i++)
            {
                output[0, i] = m[0, i] - v[0];  //newX1 = x1 - overline{x}
                output[1, i] = m[1, i] - v[1];
                output[2, i] = m[2, i] - v[2];
            }
            return output;
        }

        /// <summary>
        /// Converts array of Point3Ds to matrix of dimensions 3*points.length
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static Matrix<double> Point3DArrayToMatrix(Point3D[] points)
        {
            Matrix<double> m = Matrix<double>.Build.Dense(3, points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                m[0, i] = points[i].X;
                m[1, i] = points[i].Y;
                m[2, i] = points[i].Z;
            }

            return m;
        }
        
        /// <summary>
        /// Computes a vector v of size 3 where v[0] = average(m[0,0], m[0,1], m[0,2], ..., m[0,m.ColumnCount - 1])
        /// </summary>
        /// <param name="m">3 * n  matrix</param>
        /// <returns></returns>
        private static double[] GetAverageCoordinate(Matrix<double> m)
        {
            double[] averages = new double[3];
            double[] sums = new double[3];
            for (int i = 0; i < m.ColumnCount; i++)
            {
                sums[0] += m[0, i];
                sums[1] += m[1, i];
                sums[2] += m[2, i];
            }
            averages[0] = sums[0] / m.ColumnCount;
            averages[1] = sums[1] / m.ColumnCount;
            averages[2] = sums[2] / m.ColumnCount;
            return averages;
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