﻿using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    /// <summary>
    /// This class works the same as TestRotationComputerPCA,
    /// but the basis of micro and macro data is generated by getting uniformly distributed values
    /// </summary>
    public class TestUniformRotationComputerPCA
    {
        public static double radius = 1;

        /// <summary>
        /// Calculates the rotation matrix from dMicro to dMacro
        /// </summary>
        /// <param name="dMicro">Data1</param>
        /// <param name="dMacro">Data2</param>
        /// <param name="pointMicro">Point in data1 to take samples around</param>
        /// <param name="pointMacro">Point in data2 to take samples around</param>
        /// <param name="count">Number of samples taken</param>
        /// <returns>Returns rotation matrix</returns>
        public static Matrix<double> CalculateRotation(AData dMicro, AData dMacro, Point3D pointMicro, Point3D pointMacro, double spacing)
        {
            Matrix<double> basisMicro;
            Matrix<double> basisMacro;

            spacing = 0.3;

            //Console.WriteLine("Point micro: " + pointMicro);
            //Console.WriteLine("Point macro: " + pointMacro);

            try
            {
                basisMicro = GetPointBasis(dMicro, pointMicro, spacing, radius);
                basisMacro = GetPointBasis(dMacro, pointMacro, spacing, radius);

            }
            catch (Exception e) { throw e; }

            //Console.WriteLine("Basis micro: " + basisMicro);
            //Console.WriteLine("Basis macro: " + basisMacro);

            Matrix<double> resultMatrix = calculationRotationMatrix(basisMicro, basisMacro);
            //Console.WriteLine("Rotation matrix: " + resultMatrix);

            return resultMatrix;
        }

        private static EulerAngles CalculateRotationMatrixForVectors(Vector<double> vectorMicro, Vector<double> vectorMacro)
        {
            Vector<double> testVector1Normalized = NormalizeVector(vectorMicro);
            Vector<double> testVector2Normalized = NormalizeVector(vectorMacro);

            Vector<double> crossProduct;
            if (ParallelVectors(vectorMacro, vectorMicro))
                crossProduct = findOrthogonalVector(vectorMicro);
            else
            {
                Matrix<double> tempCrossProduct = CrossProduct(testVector1Normalized, testVector2Normalized);
                crossProduct = Vector<double>.Build.DenseOfArray(new double[] { tempCrossProduct[0, 0], tempCrossProduct[0, 1], tempCrossProduct[0, 2] });
            }

            crossProduct = NormalizeVector(crossProduct);

            double cosine = testVector1Normalized.DotProduct(testVector2Normalized);
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

            double phi = Math.Atan2(rotationMatrix[2, 1], rotationMatrix[2, 2]);
            double theta = Math.Asin(-rotationMatrix[2, 0]);
            double psi = Math.Atan2(rotationMatrix[1, 0], rotationMatrix[0, 0]);

            return new EulerAngles(phi, theta, psi);
        }

        /// <summary>
        /// Calculates the matrix using angles between the bases
        /// </summary>
        /// <param name="basisMicro">Micro basis</param>
        /// <param name="basisMacro">Macro basis</param>
        /// <returns>Returns rotation matrix to align micro to macro basis</returns>
        private static Matrix<double> calculationRotationMatrix(Matrix<double> basisMicro, Matrix<double> basisMacro)
        {
            return calculateTransitionMatrix(basisMicro, basisMacro);

            /*
            EulerAngles eulerAnglesHigh = CalculateRotationMatrixForVectors(basisMicro.Column(0), basisMacro.Column(0));
            EulerAngles eulerAnglesLow = CalculateRotationMatrixForVectors(basisMicro.Column(1), basisMacro.Column(1));
            EulerAngles eulerAnglesCross = CalculateRotationMatrixForVectors(basisMicro.Column(2), basisMacro.Column(2));

            double averageRotationX = (eulerAnglesHigh.RotationX + eulerAnglesLow.RotationX + eulerAnglesCross.RotationX)/3;
            double averageRotationY = (eulerAnglesHigh.RotationY + eulerAnglesLow.RotationY + eulerAnglesCross.RotationY) / 3;
            double averageRotationZ = (eulerAnglesHigh.RotationZ + eulerAnglesLow.RotationZ + eulerAnglesCross.RotationZ) / 3;

            //check if the rotation matrix does transform elements  of micro basis to macro
            //if not, transpose it ( or inverse it, the result should be the same) and try again
            //if neither of the solutions work, throw error

            return GetRotationMatrix(averageRotationX, averageRotationY, averageRotationZ);
            */
        }

        private static Matrix<double> GetRotationMatrix(double angleX, double angleY, double angleZ)
        {
            Matrix<double> rotationMatrixX = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                    { 1, 0, 0 },
                    { 0, Math.Cos(angleX), -Math.Sin(angleX) },
                    { 0, Math.Sin(angleX), Math.Cos(angleX) }
            });

            Matrix<double> rotationMatrixY = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                    { Math.Cos(angleY), 0, Math.Sin(angleY) },
                    { 0, 1, 0 },
                    { -Math.Sin(angleY), 0, Math.Cos(angleY) }
             });

            Matrix<double> rotationMatrixZ = Matrix<double>.Build.DenseOfArray(new double[,]
            {
                    { Math.Cos(angleZ), -Math.Sin(angleZ), 0 },
                    { Math.Sin(angleZ), Math.Cos(angleZ), 0 },
                    { 0, 0, 1 }
            });

            return (rotationMatrixX * rotationMatrixY * rotationMatrixZ);
        }

        /// <summary>
        /// Calculates the transition matrix between two given bases
        /// </summary>
        /// <param name="basisMicro">Micro basis</param>
        /// <param name="basisMacro">Macro basis</param>
        /// <returns>Returns transition matrix</returns>
        private static Matrix<double> calculateTransitionMatrix(Matrix<double> basisMicro, Matrix<double> basisMacro)
        {
            Matrix<double> transitionMatrix = Matrix<double>.Build.Dense(3, 3);

            Matrix<double> equationMatrix = Matrix<double>.Build.Dense(3, 4);


            //Calculation of the transition matrix
            for (int basisNumber = 0; basisNumber < 3; basisNumber++)
            {
                for (int i = 0; i < 3; i++)
                    equationMatrix.SetColumn(i, basisMicro.Column(i));

                equationMatrix.SetColumn(3, basisMacro.Column(basisNumber));

                Vector<double> result;

                try { result = EquationComputer.CalculateSolution(equationMatrix); }
                catch (Exception e) { throw e; }

                transitionMatrix.SetColumn(basisNumber, result);
            }

            //Replace 0 values
            for (int i = 0; i < transitionMatrix.RowCount; i++)
            {
                for (int j = 0; j < transitionMatrix.ColumnCount; j++)
                {
                    if (Math.Abs(transitionMatrix[i, j]) <= Double.Epsilon)
                        transitionMatrix[i, j] = 0;
                }
            }

            return transitionMatrix;
        }



        /// <summary>
        /// Samples points around p and returns the basis thats the most common
        /// </summary>
        /// <param name="data">Instance to data</param>
        /// <param name="p">Point p around which the points should be sampled</param>
        /// <param name="spacing">Spacing between the points</param>
        /// <param name="radius">Radius arpind the sampled </param>
        /// <returns></returns>
        private static Matrix<double> GetAveragePointBasis(AData data, Point3D p, double spacing, double radius)
        {
            List<Matrix<double>> matrices = new List<Matrix<double>>();

            for (int i = 0; i<3; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    Point3D currentPoint = new Point3D();
                    switch (i)
                    {
                        case 0:
                            currentPoint = new Point3D(p.X + j * spacing, p.Y, p.Z);
                            break;
                        case 1:
                            currentPoint = new Point3D(p.X, p.Y + j * spacing, p.Z); 
                            break;
                        case 2:
                            currentPoint = new Point3D(p.X, p.Y, p.Z + j * spacing);
                            break;
                    }
                    try
                    {
                        matrices.Add(GetPointBasis(data, currentPoint, spacing, radius));
                    }
                    catch
                    {
                        //If the basis cant be calculated
                        continue;
                    }
                }
            }

            if (matrices.Count == 0)
                throw new Exception("None of the matrices could be constructed");

            double[] sqrtAverageDistances = new double[matrices.Count];
            //Select the one that is the closest to all other
            Vector<double> zeroTranslationVector = Vector<double>.Build.Dense(3);

            for (int i = 0; i<matrices.Count; i++)
            {
                Transform3D currentlyTestedTransformation = new Transform3D(matrices[i], zeroTranslationVector);
                for(int j = i+1; j<matrices.Count; j++)
                {
                    Transform3D temporarilyTestedTransformation = new Transform3D(matrices[j], zeroTranslationVector);
                    double distance = currentlyTestedTransformation.SqrtDistanceTo(temporarilyTestedTransformation);
                    sqrtAverageDistances[j] += distance / (matrices.Count-1);
                    sqrtAverageDistances[i] += distance / (matrices.Count - 1);
                }
            }

            int smallestIndex = 0;

            //Finds element with smalles average distance
            for(int i = 1; i<matrices.Count; i++)
            {
                if (sqrtAverageDistances[i] < sqrtAverageDistances[smallestIndex])
                    smallestIndex = i;
            }

            return matrices[smallestIndex];
        }

        private static double Constrain(double value, double minValue, double maxValue)
        {
            if (value > maxValue)
                return maxValue;

            if (value < minValue)
                return minValue;

            return value;
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

        /// <summary>
        /// Compares numberA to numberB and returns true if the numbers are ~ the same
        /// </summary>
        /// <param name="numberA">First number</param>
        /// <param name="numberB">Second number</param>
        /// <returns>Returns true if the numbers are ~ the same, otherwise returns false</returns>
        private static bool compareWithTolerance(double numberA, double numberB)
        {
            double epsilon = 0.00000001;
            if ((numberA + epsilon > numberB) && (numberA - epsilon < numberB))
                return true;
            
            return false;
        }

        /// <summary>
        /// Finds orthogonal vector to the one passed as a parameter
        /// </summary>
        /// <param name="inputVector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
        /// Normalizes vector passsed as a parameter
        /// </summary>
        /// <param name="vector">Vector to normalize</param>
        /// <returns>
        /// Returns normalized vector if its magnitude is not equal to 0.
        /// Otherwise it returns Vector with 3 rows and 0 columns.
        /// </returns>
        private static Vector<double> NormalizeVector(Vector<double> vector) {

            double magnitude = vector.L2Norm();

            //Prevents from crashing when denominator(magnitude) is equal to 0
            if (magnitude == 0)
                return Vector<double>.Build.Dense(3, 0);

            return vector / magnitude;
        }

        /// <summary>
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

        // od pana Váši
        private static Matrix<double> GetPointBasis(AData d, Point3D point, double spacing, double radius)
        {
            List<Point3D> pointsInSphere = GetSphere(point, radius, spacing, d);
            List<double> values = new List<double>();
            double min = double.MaxValue;
            double max = double.MinValue;

            for (int i = 0; i < pointsInSphere.Count; i++)
            {
                values.Add(d.GetValue(pointsInSphere[i])); //All values are within bounds, no need to test it
                min = Math.Min(min, values[values.Count - 1]);
                max = Math.Max(max, values[values.Count - 1]);
            }

            if (Math.Abs(min - max) < Double.Epsilon)
                throw new ArgumentException("Basis cannot be calculated because all sampled values in the point surrounding are the same.");

            Vector<double> meanVector = Vector<double>.Build.Dense(3);
            double weightSum = 0;

            for (int i = 0; i < pointsInSphere.Count; i++)
            {
                double weight = (values[i] - min) / (max - min); // Calculate the weight

                meanVector[0] += pointsInSphere[i].X * weight;
                meanVector[1] += pointsInSphere[i].Y * weight;
                meanVector[2] += pointsInSphere[i].Z * weight;

                weightSum += weight;
            }

            meanVector /= weightSum;


            //Optimization needed
            Matrix<double> a = Matrix<double>.Build.Dense(3, pointsInSphere.Count);

            for (int i = 0; i < values.Count; i++)
            {
                Vector<double> currentVector = Vector<double>.Build.DenseOfArray(new double[] { pointsInSphere[i].X, pointsInSphere[i].Y, pointsInSphere[i].Z });
                currentVector -= meanVector;

                a.SetColumn(i, currentVector);
            }

            Matrix<double> covarianceMatrix = a * a.Transpose();

            var evd = covarianceMatrix.Evd();
            return evd.EigenVectors;
        }

        /// <summary>
        /// This method prints generated points in a sphere that are 
        /// </summary>
        /// <param name="pointsInSphere"></param>
        private static void PrintGeneratedPoints(List<Point3D> pointsInSphere)
        {
            string pointsX = "[";
            for (int i = 0; i < pointsInSphere.Count; i++)
            {
                pointsX = pointsX + (pointsInSphere[i].X.ToString().Replace(",", ".")) + ", ";
            }
            pointsX = pointsX + "]";
            Console.WriteLine(pointsX);

            string pointsY = "[";
            for (int i = 0; i < pointsInSphere.Count; i++)
            {
                pointsY = pointsY + (pointsInSphere[i].Y.ToString().Replace(",", ".")) + ", ";
            }
            pointsY = pointsY + "]";
            Console.WriteLine(pointsY);

            string pointsZ = "[";
            for (int i = 0; i < pointsInSphere.Count; i++)
            {
                pointsZ = pointsZ + (pointsInSphere[i].Z.ToString().Replace(",", ".")) + ", ";
            }
            pointsZ = pointsZ + "]";
            Console.WriteLine(pointsZ);
        }

        /// <summary>
        /// Gets points uniformly distributed from the center point
        /// </summary>
        /// <param name="p">Center around which the points are generated</param>
        /// <param name="r">Generated point's distance from the center</param>
        /// <param name="spacing">Spacing between the points</param>
        /// <param name="data">Data unsed for checking if the value is within bounds of the object.</param>
        /// <returns>A grid of points uniformly distributed in the sphere radius from a given point.</returns>
        private static List<Point3D> GetSphere(Point3D p, double r, double spacing, AData data)
        {
            List<Point3D> points = new List<Point3D>();

            for(double x = -r; x<=r; x+=spacing)
            {
                for(double y = -r; y<=r; y+=spacing)
                {
                    SphereBounds zBounds;

                    try { zBounds = GetSphereBounds(x, y, r); }
                    catch { continue; } //No point in z is in the bounds

                    for (double z = zBounds.MinCoordinate; z <= zBounds.MaxCoordinate; z += spacing)
                    {
                        Point3D point = new Point3D(x + p.X, y + p.Y, z + p.Z);
                        if (data.PointWithinBounds(point))
                            points.Add(point);
                    }
                }
            }

            return points;
        }


        /// <summary>
        /// Given an x and y coordinates, this method will generate a min and max z coordinate
        /// so that [x, y, z] are inside a sphere specified by given radius
        /// </summary>
        /// <param name="x">Coordinate X</param>
        /// <param name="y">Coordinate Y</param>
        /// <param name="radius">Radius of a sphere</param>
        /// <returns>Returns instance of SphereBounds where are the min and max Z values.</returns>
        /// <exception cref="ArgumentException">Throws an exception if the bounds for Z don't exist.</exception>
        private static SphereBounds GetSphereBounds(double x, double y, double radius)
        {
            double rSquared = Math.Pow(radius, 2);
            double zSquared = rSquared - Math.Pow(x, 2) - Math.Pow(y, 2);

            if (zSquared < 0)
                throw new ArgumentException("No values are within bounds for the given X, Y coordinates and given radius");

            double minZ = -Math.Sqrt(zSquared);
            double maxZ = -minZ;

            return new SphereBounds(minZ, maxZ);
        }

        /// <summary>
        /// Returns random number within the specified bounds
        /// </summary>
        /// <param name="minimum">Minimum number to be generated</param>
        /// <param name="maximum">Maximum number to be generated</param>
        /// <param name="r">Instance of Random class</param>
        /// <returns></returns>
        private static double GetRandomDouble(double minimum, double maximum, Random r)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }

        
    }

    /// <summary>
    /// This is a messenger class for passing min and max bounds
    /// </summary>
    class SphereBounds
    {
        private double minCoordinate;
        private double maxCoordinate;

        public SphereBounds(double minCoordinate, double maxCoordinate)
        {
            this.minCoordinate = minCoordinate;
            this.maxCoordinate = maxCoordinate;
        }

        //GETTERS
        public double MinCoordinate
        {
            get { return minCoordinate; }
        }

        public double MaxCoordinate
        {
            get { return maxCoordinate; }
        }
    }

    /// <summary>
    /// This is a messenger class for passing Euler angles of a rotation
    /// </summary>
    class EulerAngles
    {
        private double rotationX;
        private double rotationY;
        private double rotationZ;

        public EulerAngles(double rotationX, double rotationY, double rotationZ)
        {
            this.rotationX = rotationX;
            this.rotationY = rotationY;
            this.rotationZ = rotationZ;
        }

        //GETTERS
        public double RotationX
        {
            get { return rotationX; }
        }

        public double RotationY
        {
            get { return rotationY; }
        }

        public double RotationZ
        {
            get { return rotationZ; }
        }
    }
}