using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace DataView
{
    public class FeatureComputerPCALength : IFeatureComputer
    {

        public static double radius = 2;

        private static Vector<double> CrossProduct(Vector<double> firstVector, Vector<double> secondVector)
        {
            return Vector<double>.Build.DenseOfArray(new double[] {
                firstVector[1] * secondVector[2] - firstVector[2] * secondVector[1],
                firstVector[2] * secondVector[0] - firstVector[0] * secondVector[2],
                firstVector[0] * secondVector[1] - firstVector[1] * secondVector[0]
            });
        }

        // od pana Váši
        public static Vector<double> GetPointBasis(AData d, Point3D point, double spacing, double radius)
        {
            List<Point3D> points = GetSphere(point, radius, spacing, d);
            List<double> values = CalculateValues(points, d);

            //Save(outputFilename, "unfiltered", points, values);

            /* Threshold to filter insignificant  values */
            QuickSelectClass quickSelectClass = new QuickSelectClass();
            double threshold = quickSelectClass.QuickSelect(values, values.Count / 2);
            FilterPoints(ref points, ref values, threshold);


            //Save(outputFilename, "filtered", points, values);

            Vector<double> meanVector = CalculateWeightedMeanVector(points);
            Matrix<double> covarianceMatrix = CalculateCovarianceMatrix(points, meanVector);

            Vector<double> eigenValues = covarianceMatrix.Evd().EigenValues.Real();

            for (int i = 0; i < eigenValues.Count; i++)
                eigenValues[i] = Math.Abs(eigenValues[i]);

            return eigenValues / eigenValues.L2Norm();
        }

        private static List<double> CalculateValues(List<Point3D> points, AData d)
        {
            List<double> values = new List<double>();
            double min = double.MaxValue, max = double.MinValue;

            for (int i = 0; i < points.Count; i++)
            {
                values.Add(d.GetValue(points[i]));
                min = Math.Min(min, values[values.Count - 1]);
                max = Math.Max(max, values[values.Count - 1]);
            }

            if (Math.Abs(min - max) < Double.Epsilon)
                throw new ArgumentException("Basis cannot be calculated because all sampled values in the point surrounding are the same.");

            return values;
        }

        private static void FilterPoints(ref List<Point3D> points, ref List<double> values, double threshold)
        {

            List<Point3D> filteredPoints = new List<Point3D>();
            List<double> filteredValues = new List<double>();

            for (int i = 0; i < points.Count; i++)
            {
                if (values[i] >= threshold)
                {
                    filteredValues.Add(values[i]);
                    filteredPoints.Add(points[i]);
                }
            }

            points = filteredPoints;
            values = filteredValues;
        }

        private static Matrix<double> CalculateCovarianceMatrix(List<Point3D> pointsInSphere, Vector<double> meanVector)
        {
            int N = pointsInSphere.Count;
            Matrix<double> A = Matrix<double>.Build.Dense(N, 3); // Each row is a point (N x 3)

            for (int i = 0; i < N; i++)
            {
                A.SetRow(i, new double[] {
                    pointsInSphere[i].X - meanVector[0],
                    pointsInSphere[i].Y - meanVector[1],
                    pointsInSphere[i].Z - meanVector[2]
                });
            }

            return (A.Transpose() * A) / (N - 1); // (3xN) * (Nx3) = 3x3 covariance matrix
        }

        private static Vector<double> CalculateWeightedMeanVector(List<Point3D> pointsInSphere)
        {
            Vector<double> meanVector = Vector<double>.Build.Dense(3);

            for (int i = 0; i < pointsInSphere.Count; i++)
            {
                meanVector[0] += pointsInSphere[i].X;
                meanVector[1] += pointsInSphere[i].Y;
                meanVector[2] += pointsInSphere[i].Z;
            }

            meanVector /= pointsInSphere.Count;
            return meanVector;
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

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            double spacing = Math.Min(Math.Min(d.XSpacing, d.YSpacing), d.ZSpacing) / 2.0;
            double radius = 3;

            return new FeatureVector(p, GetPointBasis(d, p, spacing, radius).ToArray());
        }
    }
}