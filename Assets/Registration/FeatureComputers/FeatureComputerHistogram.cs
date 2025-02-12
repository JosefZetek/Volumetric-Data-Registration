using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataView
{
    public class FeatureComputerHistogram : IFeatureComputer
    {
        private const int DEFAULT_SIZE = 20;

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            double[] numberOfOccurences = new double[DEFAULT_SIZE];

            double radius = Math.Max(Math.Max(2 * d.XSpacing, 2 * d.YSpacing), 2 * d.ZSpacing);
            double samplingRate = Math.Min(Math.Min(d.XSpacing, d.YSpacing), d.ZSpacing) / 2;

            List<Point3D> points = GetSphere(p, radius, samplingRate, d);

            foreach (Point3D point in points)
                AddValue(numberOfOccurences, d.GetValue(point));

            return new FeatureVector(p, numberOfOccurences);
        }

        private void AddValue(double[] numberOfOccurences, double value)
        {
            int closestSmallerInteger = (int)Math.Floor(value);
            int closestBiggerInteger = (int)Math.Ceiling(value);

            double percentageSmaller = (value - closestSmallerInteger) / (closestBiggerInteger - closestSmallerInteger);
            double percentageBigger = 1 - percentageSmaller;

            numberOfOccurences[closestSmallerInteger] += percentageSmaller;
            numberOfOccurences[closestBiggerInteger] += percentageBigger;
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
            {
                Debug.Log($"No values within radius");
                throw new ArgumentException("No values are within bounds for the given X, Y coordinates and given radius");
            }
                

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

        ///// <summary>
        ///// This is a messenger class for passing min and max bounds
        ///// </summary>
        //class SphereBounds
        //{
        //    private double minCoordinate;
        //    private double maxCoordinate;

        //    public SphereBounds(double minCoordinate, double maxCoordinate)
        //    {
        //        this.minCoordinate = minCoordinate;
        //        this.maxCoordinate = maxCoordinate;
        //    }

        //    //GETTERS
        //    public double MinCoordinate
        //    {
        //        get { return minCoordinate; }
        //    }

        //    public double MaxCoordinate
        //    {
        //        get { return maxCoordinate; }
        //    }
        //}
    }
}