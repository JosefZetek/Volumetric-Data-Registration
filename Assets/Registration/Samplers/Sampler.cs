using System;

namespace DataView
{
    class Sampler : ISampler
    {
        //private int rSphere = 5; //Included in config

        private int rSphere;

        public Point3D[] Sample(AData d, int count)
        {
            Point3D[] points = new Point3D[count];

            int[] measures = d.Measures;
            Random r = new Random();
            for (int i = 0; i < count; i++)
            {
                double x = GetRandomDouble(rSphere, measures[0]* d.XSpacing - rSphere, r) ;
                double y = GetRandomDouble(rSphere, measures[1]* d.YSpacing - rSphere, r) ;
                double z = GetRandomDouble(rSphere, measures[2]* d.ZSpacing - rSphere, r) ;

                points[i] = new Point3D(x, y, z);
            }
            return points;
        }

        private double GetRandomDouble(double minimum, double maximum, Random r)
        { 
            return r.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
