using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{

    /// <summary>
    /// This implementation of FeatureComputer calculates average vector in two rings.
    /// It then calcualtes magnitude of these vectors, average values and the angle between them and uses these values as features.
    /// </summary>
    public class TestFeatureComputer : IFeatureComputer
    {
        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            Random random = new Random();
            List<Point3D> sampledPoints;

            double avgFirst = 0;
            sampledPoints = GetRingPoints(0, 1, 10000, random.Next());
            Vector<double> directionFirst = GetDirectionVector(sampledPoints, p, d, ref avgFirst);

            double avgSecond = 0;
            sampledPoints = GetRingPoints(1, 2, 10000, random.Next());
            Vector<double> directionSecond = GetDirectionVector(sampledPoints, p, d, ref avgSecond);

            double firstDirectionMagnitude = directionFirst.L2Norm();
            double secondDirectionMagnitude = directionSecond.L2Norm();

            double angle = Math.Acos(directionFirst.DotProduct(directionSecond) / (firstDirectionMagnitude * secondDirectionMagnitude));

            return new FeatureVector(p, new double[] { firstDirectionMagnitude, secondDirectionMagnitude, avgFirst, avgSecond, angle });
        }

        /// <summary>
        /// This method returns list of coordinates with given min and max distance from origin
        /// </summary>
        /// <param name="minRadius">Minimum radius from origin</param>
        /// <param name="maxRadius">Maximum radius from origin</param>
        /// <param name="count">Number of points</param>
        /// <param name="seed">Seed used for generation of random points</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private List<Point3D> GetRingPoints(double minRadius, double maxRadius, int count, int seed)
        {
            if (minRadius > maxRadius)
                throw new ArgumentException("Min radius is expected to be lower than max radius");

            if (count < 0)
                throw new ArgumentException("Count needs to be positive");

            List<Point3D> points = new List<Point3D>();
            Random random = new Random(seed);

            for (int i = 0; i < count; i++)
            {
                double randomRadius = random.NextDouble() * (maxRadius - minRadius) + minRadius;
                double randomAngle1 = random.NextDouble() * 2 * Math.PI;
                double randomAngle2 = random.NextDouble() * 2 * Math.PI;

                //The calculations are based on formula in section Generalization here https://en.wikipedia.org/wiki/Spherical_coordinate_system

                double xCoordinate = randomRadius * Math.Sin(randomAngle1) * Math.Cos(randomAngle2);
                double yCoordinate = randomRadius * Math.Sin(randomAngle1) * Math.Sin(randomAngle2);
                double zCoordinate = randomRadius * Math.Cos(randomAngle1);

                points.Add(new Point3D(xCoordinate, yCoordinate, zCoordinate));
            }

            return points;
        }

        private Vector<double> GetDirectionVector(List<Point3D> sampledPoints, Point3D centerPoint, AData data, ref double percentage)
        {
            Vector<double> directionVector = Vector<double>.Build.Dense(3);

            double[] values = new double[sampledPoints.Count];
            double min = double.MaxValue;
            double max = double.MinValue;

            Point3D wAvg = new Point3D(0, 0, 0);
            double ws = 0;

            //Get min & max values and load data to values array
            for (int i = 0; i < sampledPoints.Count; i++)
            {
                values[i] = data.GetValue(sampledPoints[i] + centerPoint);

                min = Math.Min(min, values[i]);
                max = Math.Max(max, values[i]);
            }

            for (int i = 0; i < values.Length; i++)
            {

                double w = (values[i] - min) / (max - min); //percentage from the overall range
                ws += w;

                wAvg.X += (sampledPoints[i].X + centerPoint.X) * w;
                wAvg.Y += (sampledPoints[i].Y + centerPoint.Y) * w;
                wAvg.Z += (sampledPoints[i].Z + centerPoint.Z) * w;
            }


            wAvg.X /= ws;
            wAvg.Y /= ws;
            wAvg.Z /= ws;

            percentage = (data.GetValue(wAvg) - min)/(max-min);

            double diffX = wAvg.X - centerPoint.X;
            double diffY = wAvg.Y - centerPoint.Y;
            double diffZ = wAvg.Z - centerPoint.Z;

            directionVector[0] = diffX;
            directionVector[1] = diffY;
            directionVector[2] = diffZ;

            return directionVector;
        }
    }
}

