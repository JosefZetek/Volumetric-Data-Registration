using System;
using System.Collections.Generic;

namespace DataView
{
    class FeatureComputerDataDistribution : IFeatureComputer
	{
        List<double> hodnoty;

        private double[] numberOfOccurences;

        private const int DEFAULT_SIZE = 20;

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {

            hodnoty = new List<double>();

            SetDefaultValues();

            Random random = new Random();

            while(hodnoty.Count < 1_000)
            {
                List<Point3D> points = GetSpheresIntersticePoints(p, 0, 1, 1000-hodnoty.Count, random.Next());
                
                for (int i = 0; i < points.Count; i++)
                {
                    try { hodnoty.Add(d.GetPercentile(d.GetValue(points[i]))); }
                    catch { continue; }
                }
            }

            hodnoty.Sort();

            return new FeatureVector(p, hodnoty.ToArray());
        }

        private void SetDefaultValues()
        {
            numberOfOccurences = new double[DEFAULT_SIZE];
        }

        private void AddValue(double value)
        {
            int closestSmallerInteger = (int)Math.Floor(value);
            int closestBiggerInteger = (int)Math.Ceiling(value);

            double percentageSmaller = (value - closestSmallerInteger) / (closestBiggerInteger - closestSmallerInteger);
            double percentageBigger = 1 - percentageSmaller;

            numberOfOccurences[closestSmallerInteger] += percentageSmaller;
            numberOfOccurences[closestBiggerInteger] += percentageBigger;
        }

        public void ShowValues(AData d, Point3D p)
        {

            List<double> data = new List<double>();
            List<Point3D> points = GetSpheresIntersticePoints(p, 0, 1, 10000, 0);
            foreach(Point3D point in points)
            {
                try { data.Add(d.GetPercentile(d.GetValue(point)) * 100.0); }
                catch { continue; }
            }

            data.Sort();
            Console.WriteLine("[{0}]", string.Join(", ", data.ToArray()));
        }

        /// <summary>
        /// This method returns list of points that are within certain distance defined as minRadius and maxRadius from the center of the sphere passed as a point
        /// </summary>
        /// <param name="point">Center of a sphere</param>
        /// <param name="minRadius">Generated point's minimum distance from the center of a sphere</param>
        /// <param name="maxRadius">Generated point's maximum distance from the center of a sphere</param>
        /// <returns>Returns list of points that are within certain distance from the center of the sphere.</returns>
        private List<Point3D> GetSpheresIntersticePoints(Point3D point, double minRadius, double maxRadius, int count, int seed)
        {
            if (minRadius > maxRadius)
                throw new ArgumentException("The min radius should be smaller than the max radius");

            Random random = new Random(seed);

            List<Point3D> listOfPoints = new List<Point3D>();
            while (listOfPoints.Count < count)
            {
                double radius = GetRandomDouble(minRadius, maxRadius + 0.0001, random);
                double angleTheta = random.NextDouble() * 2 * Math.PI;
                double anglePhi = random.NextDouble() * 2 * Math.PI;
                double x = radius * Math.Cos(anglePhi) * Math.Sin(angleTheta);
                double y = radius * Math.Sin(angleTheta) * Math.Sin(anglePhi);
                double z = radius * Math.Cos(angleTheta);

                listOfPoints.Add(new Point3D(x, y, z));
            }

            return listOfPoints;
        }

        private double GetRandomDouble(double minimum, double maximum, Random r)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}