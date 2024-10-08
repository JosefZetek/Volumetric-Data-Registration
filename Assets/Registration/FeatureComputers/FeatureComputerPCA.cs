using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class FeatureComputerPCA : IFeatureComputer
    {
        public static double radius = 3;

        public Matrix<double> CalculatePCAVectors(AData data, Point3D point, double spacing)
        {


            Matrix<double> basis = Matrix<double>.Build.Dense(3,3);
            
            spacing = 0.3;

            try
            {
                basis = GetPointBasis(data, point, spacing, radius);
            }
            catch (Exception e) { throw e; }

            return basis;
        }



        /// <summary>
        /// Normalizes vector passsed as a parameter
        /// </summary>
        /// <param name="vector">Vector to normalize</param>
        /// <returns>
        /// Returns normalized vector if its magnitude is not equal to 0.
        /// Otherwise it returns Vector with 3 rows and 0 columns.
        /// </returns>
        private Vector<double> NormalizeVector(Vector<double> vector) {

            double magnitude = vector.L2Norm();

            //Prevents from crashing when denominator(magnitude) is equal to 0
            if (magnitude == 0)
                return Vector<double>.Build.Dense(3, 0);

            return vector / magnitude;
        }

        // od pana Váši
        private Matrix<double> GetPointBasis(AData d, Point3D point, double spacing, double radius)
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

            for(double x = -r; x<=r; x+=spacing)
            {
                for(double y = -r; y<=r; y+=spacing)
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
            const double radiusX = 2;
            const double radiusY = 2;
            const double radiusZ = 2;

            List<double> featurevector = new List<double>();

            Matrix<double> a = CalculatePCAVectors(d, p, 0.3);

            for(double i = -radiusX; i<=radiusX; i+=0.5)
            {
                for (double j = -radiusY; j <= radiusY; j+=0.5)
                {
                    for(double k = -radiusZ; k <= radiusZ; k+=0.5)
                    {
                        Vector<double> additionI = a.Column(0) * i;
                        Vector<double> additionJ = a.Column(1) * j;
                        Vector<double> additionK = a.Column(2) * k;

                        try
                        {
                            featurevector.Add(d.GetPercentile(d.GetValue(new Point3D(
                                p.X + additionI[0] + additionJ[0] + additionK[0],
                                p.Y + additionI[1] + additionJ[1] + additionK[1],
                                p.Z + additionI[2] + additionJ[2] + additionK[2]))));
                        }
                        catch
                        {
                            throw new Exception("too close to border");
                        }

                    }

                }
            }

            return new FeatureVector(p, featurevector.ToArray());
        }
    }

    /// <summary>
    /// This is a messenger class for passing min and max bounds
    /// </summary>
    class Bounds
    {
        private double minCoordinate;
        private double maxCoordinate;

        public Bounds(double minCoordinate, double maxCoordinate)
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
}