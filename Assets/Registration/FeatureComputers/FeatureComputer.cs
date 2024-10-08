using System;
using System.Collections.Generic;

namespace DataView
{
    /// <summary>
    /// 
    /// </summary>
    public class FeatureComputer : IFeatureComputer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="d"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public List<Point3D> GetSphere(Point3D x, double r)
        {
            return GetSphere(x, r, 500);
        }

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
                {
                    points.Add(new Point3D(xCoordinate, yCoordinate, zCoordinate));
                }
            } while (
                points.Count < count
            );

            return points;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            double[] fv = new double[5];
            double norm = 0;

            int i = 0;
            double delta = 1;
            for (double r = delta; r <= 5*delta; r += delta)
            {
                double sum = 0;
                //List<Point3D> points = GetSphere(p, r, (i + 1) * 500);
                List<Point3D> points = GetSpheresIntersticePoints(p, r, r, 500, 20);

                foreach (Point3D point in points)
                {
                    try
                    {
                        sum += d.GetValue(point); //real coordinates
                    }
                    catch
                    {
                        //Catches if the point is out of bounds
                        continue;
                    }
                    
                }
                double avg = sum / points.Count;
                norm += avg * avg;
                fv[i] = avg;
                i++;
            }

            norm = (norm == 0) ? 1 : Math.Sqrt(norm);

            return new FeatureVector(p, fv[0] / norm, fv[1] / norm, fv[2] / norm, fv[3] / norm, fv[4] / norm);
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
                double radius = GetRandomDouble(minRadius, maxRadius+0.0001, random);
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
