using System;
using System.Collections.Generic;

namespace DataView
{
    public class FeatureComputerRings : IFeatureComputer
    {
        private List<Point3D> GetSphere(Point3D p, double r, int count)
        {
            List<Point3D> points = new List<Point3D>();

            Random rnd = new Random();
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

        private List<Point3D> GetRing(Point3D p, double r1, double r2, int count)
        {
            List<Point3D> points = new List<Point3D>();
            Random rnd = new Random();
            do
            {
                double r = GetRandomDouble(r1, r2, rnd);
                double phi = GetRandomDouble(0, 2 * Math.PI, rnd);
                double theta = GetRandomDouble(0, Math.PI, rnd);

                points.Add(new Point3D(p.X + r * Math.Sin(theta) * Math.Cos(phi), p.Y + r * Math.Sin(theta) * Math.Sin(phi), p.Z + r * Math.Cos(theta)));
            } while (
                points.Count < count
            );

            return points;
        }

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            double[] fv = new double[5];

            int i = 0;
            double sum;
            double delta = 0.4;
            for (double r = delta; r <= 5*delta; r += delta)
            {
                int count = 5000;
                int actualCount = 0;
                List<Point3D> points = GetRing(p, r - delta, r, count);
                sum = 0;
                foreach (Point3D point in points)
                {
                    try
                    {
                        sum += d.GetValue(point); //real coordinates
                        actualCount++;
                    }
                    catch { continue; }
                    
                }
                double avg = sum / count;

                if (actualCount == 0)
                    avg = 0;
                else
                    avg = sum / actualCount;

                fv[i] = avg;
                i++;
            }

            return new FeatureVector(p, fv[0], fv[1], fv[2], fv[3], fv[4]);
        }

        private double GetRandomDouble(double minimum, double maximum, Random r)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
