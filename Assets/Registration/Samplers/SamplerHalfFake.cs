using System;

namespace DataView
{
    public class SamplerHalfFake : ISampler
    {
        private int radius;

        public SamplerHalfFake(int radius)
        {
            this.radius = radius;
        }

        int translationX = 0;
        int translationY = 0;
        int translationZ = 0;

        // for macro data
        Point3D[] pointsMax;
        Point3D[] pointsMin;

        public Point3D[] Sample(AData d, int count, int radius)
        {
            this.pointsMax = new Point3D[count];
            this.pointsMin = new Point3D[count];
            int[] measures = d.Measures;
            Random r = new Random(); // change rnd

            for (int i = 0; i < count; i++)
            {
                double x = r.Next(translationX + radius, measures[0] - radius);
                double y = r.Next(translationY + radius, measures[1] - radius);
                double z = r.Next(translationZ + radius, measures[2] - radius);

                pointsMax[i] = new Point3D(x, y, z);
            }
            GetSamples2();
            return pointsMax;
        }

        public void SetTranslation(int[] translation)
        {
            this.translationX = translation[0];
            this.translationY = translation[1];
            this.translationZ = translation[2];
        }

        private void GetSamples2()
        {
            Random r = new Random();
            
            for (int i = 0; i < pointsMax.Length; i++)
            {
                double d = r.NextDouble();
                this.pointsMin[i] = new Point3D(pointsMax[i].X - translationX + d, pointsMax[i].Y - translationY + d, pointsMax[i].Z - translationZ + d);
            }
        }

        public Point3D[] Sample(AData d, int count)
        {
            return Sample(d, count, this.radius);

        }

        internal Point3D[] PointsMin { get => pointsMin; set => pointsMin = value; }

    }

}