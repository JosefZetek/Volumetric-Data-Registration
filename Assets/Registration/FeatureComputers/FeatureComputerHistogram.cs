using System;
using System.Collections.Generic;

namespace DataView
{
    public class FeatureComputerHistogram : IFeatureComputer
    {
        private const int DEFAULT_SIZE = 20;
        private double radiusMultiplier;

        public FeatureComputerHistogram()
        {
            this.radiusMultiplier = 5;
        }

        public FeatureComputerHistogram(double radiusMultiplier)
        {
            this.radiusMultiplier = radiusMultiplier;
        }

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            double[] numberOfOccurences = new double[DEFAULT_SIZE];

            double radius = Math.Max(Math.Max(radiusMultiplier * d.XSpacing, radiusMultiplier * d.YSpacing), radiusMultiplier * d.ZSpacing);
            double samplingRate = Math.Min(Math.Min(d.XSpacing, d.YSpacing), d.ZSpacing) / 2;

            List<Point3D> points = GetSphere(p, radius, samplingRate, d);

            foreach (Point3D point in points)
                AddValue(d, point, numberOfOccurences);

            return new FeatureVector(p, numberOfOccurences);
        }

        private void AddValue(AData d, Point3D p, double[] numberOfOccurences)
        {
            double value = d.GetValue(p);

            int closestSmallerInteger = (int)Math.Floor(value);
            int closestBiggerInteger = (int)Math.Ceiling(value);

            int biggerIndex = (int)((closestBiggerInteger - d.MinValue) / (d.MaxValue - d.MinValue) * (DEFAULT_SIZE - 1));
            int smallerIndex = (int)((closestSmallerInteger - d.MinValue) / (d.MaxValue - d.MinValue) * (DEFAULT_SIZE - 1));

            if(closestBiggerInteger == closestSmallerInteger)
            {
                numberOfOccurences[smallerIndex] += 1.0;
                return;
            }

            double percentageSmaller = (value - closestSmallerInteger) / (closestBiggerInteger - closestSmallerInteger);
            double percentageBigger = 1 - percentageSmaller;

            numberOfOccurences[smallerIndex] += percentageSmaller;
            numberOfOccurences[biggerIndex] += percentageBigger;
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
    }
}