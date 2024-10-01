using System;
using System.Collections.Generic;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    class FeatureComputerCurvature : IFeatureComputer
    {
        //public static double radius = 2;

        private Matrix<double> GetPCABasis(AData d, Point3D point, double spacing, double radius)
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

            for (double x = -r; x <= r; x += spacing)
            {
                for (double y = -r; y <= r; y += spacing)
                {
                    Bounds zBounds;

                    try { zBounds = GetSphereBounds(x, y, r); }
                    catch { continue; } //No point in z is in the bounds

                    for (double z = zBounds.MinCoordinate; z <= zBounds.MaxCoordinate; z += spacing)
                    {
                        Point3D point = new Point3D(x + p.X, y + p.Y, z + p.Z);

                        if (IsWithinBounds(data, point))
                            points.Add(point);
                    }
                }
            }

            return points;
        }


        /// <summary>
        /// This method tests if the point is within the bounds of an object provided as a parameter data
        /// </summary>
        /// <param name="data">Data instance for the object</param>
        /// <param name="p">Point tested</param>
        /// <returns>Returns true if point is within the bounds, false otherwise.</returns>
        private static bool IsWithinBounds(AData data, Point3D p)
        {
            if (p.X < 0 || p.X > (data.Measures[0] - data.XSpacing))
                return false;

            if (p.Y < 0 || p.Y > (data.Measures[1] - data.YSpacing))
                return false;

            if (p.Z < 0 || p.Z > (data.Measures[2] - data.ZSpacing))
                return false;

            return true;
        }

        private Curvatures CalculateCurvature(Point3D point, AData d, double spacing, double radius, double derivativeStep)
        {
            Vector<double> pointVertex = Vector<double>.Build.DenseOfArray(new double[3] { point.X, point.Y, point.Z });
            pointVertex /= pointVertex.L2Norm();
            double pointValue = d.GetValue(point);

            Matrix<double> originalVectors;
            Vector<double> firstDirection, secondDirection;
            Point3D shiftedPoint;

            originalVectors = GetPCABasis(d, point, spacing, radius);

            //min curvature
            firstDirection = originalVectors.Column(0);
            //Console.WriteLine("Min direction: " + firstDirection);

            shiftedPoint = new Point3D(
                point.X + firstDirection[0] * derivativeStep,
                point.Y + firstDirection[1] * derivativeStep,
                point.Z + firstDirection[2] * derivativeStep
                );

            secondDirection = GetPCABasis(d, shiftedPoint, spacing, radius).Column(0);
            //Console.WriteLine("Min direction after shift: " + secondDirection);

            //Console.WriteLine("First direction dp: " + firstDirection.DotProduct(pointVertex));
            //Console.WriteLine("Second direction dp: " + secondDirection.DotProduct(pointVertex));

            double minCurvature = (secondDirection - firstDirection).L2Norm() / derivativeStep;


            //max curvature
            firstDirection = originalVectors.Column(2);
            //Console.WriteLine("Max direction: " + firstDirection);

            shiftedPoint = new Point3D(
               point.X + firstDirection[0] * derivativeStep,
               point.Y + firstDirection[1] * derivativeStep,
               point.Z + firstDirection[2] * derivativeStep
               );

            secondDirection = GetPCABasis(d, shiftedPoint, spacing, radius).Column(2);
            //Console.WriteLine("Max direction after shift: " + secondDirection);

            //Console.WriteLine("First direction dp: " + firstDirection.DotProduct(blbost));
            //Console.WriteLine("Second direction dp: " + secondDirection.DotProduct(blbost));

            double maxCurvature = (secondDirection - firstDirection).L2Norm() / derivativeStep;

            /*
            Console.WriteLine("Min radius: " + (1 / minCurvature));
            Console.WriteLine("Max radius: " + (1 / maxCurvature));
            Console.WriteLine("Actual radius: " + Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2) + Math.Pow(point.Z, 2)));
            */
            

            return new Curvatures(minCurvature, maxCurvature);
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
        private static Bounds GetSphereBounds(double x, double y, double radius)
        {
            double rSquared = Math.Pow(radius, 2);
            double zSquared = rSquared - Math.Pow(x, 2) - Math.Pow(y, 2);

            if (zSquared < 0)
                throw new ArgumentException("No values are within bounds for the given X, Y coordinates and given radius");

            double minZ = -Math.Sqrt(zSquared);
            double maxZ = -minZ;

            return new Bounds(minZ, maxZ);
        }

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            double radius = Math.Max(Math.Max(d.XSpacing, d.YSpacing), d.ZSpacing)/2;

            double h = 0.01;
            Curvatures curvature = CalculateCurvature(p, d, 0.1, radius, h);

            return new FeatureVector(p, new double[] { curvature.MinCurvature, curvature.MaxCurvature });
        }

        /// <summary>
        /// This is a messenger class for passing curvature in a direction of min variance and curvature in a direction of max variance
        /// </summary>
        class Curvatures
        {
            private double minCurvature;
            private double maxCurvature;

            public Curvatures(double minCurvature, double maxCurvature)
            {
                this.minCurvature= minCurvature;
                this.maxCurvature = maxCurvature;
            }

            //GETTERS
            public double MinCurvature
            {
                get { return minCurvature; }
            }

            public double MaxCurvature
            {
                get { return maxCurvature; }
            }
        }
    }
}
