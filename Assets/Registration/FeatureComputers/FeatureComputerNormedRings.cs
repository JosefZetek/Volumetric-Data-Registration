using System;
using System.Collections.Generic;

namespace DataView
{
    public class FeatureComputerNormedRings : IFeatureComputer
    {
        private List<Point3D> GetSphere(Point3D x, double r, int count)
        {
            List<Point3D> points = new List<Point3D>();

            Random rnd = new Random();
            double rSquared = r * r;
            do
            {
                double xCoordinate = GetRandomDouble(x.X - r, x.X + r, rnd);
                double yCoordinate = GetRandomDouble(x.Y - r, x.Y + r, rnd);
                double zCoordinate = GetRandomDouble(x.Z - r, x.Z + r, rnd);

                double distance = (xCoordinate - x.X) * (xCoordinate - x.X) + (yCoordinate - x.Y) * (yCoordinate - x.Y) + (zCoordinate - x.Z) * (zCoordinate - x.Z);

                if (distance <= rSquared)
                    points.Add(new Point3D(xCoordinate, yCoordinate, zCoordinate));

            } while (points.Count < count);

            return points;
        }

        private List<Point3D> GetRing(Point3D p, double r1, double r2, int count)
        {
            List<Point3D> points = new List<Point3D>();
            Random rnd = new Random();
            double[] x = new double[count];
            double[] y = new double[count];
            double[] z = new double[count];
            int i = 0;
            do
            {
                double r = GetRandomDouble(r1, r2, rnd);
                double phi = GetRandomDouble(0, 2 * Math.PI, rnd);
                double theta = GetRandomDouble(0, Math.PI, rnd);
                x[i] = p.X + r * Math.Sin(theta) * Math.Cos(phi);
                y[i] = p.Y + r * Math.Sin(theta) * Math.Sin(phi);
                z[i] = p.Z + r * Math.Cos(theta);
                points.Add(new Point3D(p.X + r * Math.Sin(theta) * Math.Cos(phi), p.Y + r * Math.Sin(theta) * Math.Sin(phi), p.Z + r * Math.Cos(theta)));
                i++;
            } while (points.Count < count
            );

            return points;
        }
        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            double[] fv = new double[5];
            double norm = 0;

            int i = 0;
            double sum;


            double delta = 0.3;
            for (double r = delta; r <= 5 * delta; r += delta)
            {
                int count = (i + 1) * 500;//5000;

                List<Point3D> points = GetRing(p, r - delta, r, count);
                sum = 0;
                foreach (Point3D point in points)
                {
                    try
                    {
                        sum += d.GetValue(point); //real coordinates
                    }
                    catch { continue; }
                    
                }

                norm += sum * sum;
                fv[i] = sum;
                i++;
            }

            norm = norm == 0 ? 1 : Math.Sqrt(norm);

            return new FeatureVector(p, fv[0] / norm, fv[1] / norm, fv[2] / norm, fv[3] / norm, fv[4] / norm);
        }

        private double GetRandomDouble(double minimum, double maximum, Random r)
        {
            return r.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
