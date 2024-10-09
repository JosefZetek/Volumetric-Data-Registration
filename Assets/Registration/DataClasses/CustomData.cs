using System;

namespace DataView
{
    public class CustomData : AData
    {
        private int[] measures = new int[] { 3, 3, 3 };

        private double xSpacing = 1;
        private double ySpacing = 1;
        private double zSpacing = 1;

        public override double GetPercentile(double value)
        {
            throw new NotImplementedException();
        }

        public override double GetValue(double x, double y, double z)
        {

            if (x >= Measures[0] || y >= Measures[1] || z >= Measures[2])
                throw new ArgumentException("Out of bounds");

            double roundedX = ThirdsDivision(x);
            double roundedY = ThirdsDivision(y);
            double roundedZ = ThirdsDivision(z);

            return roundedX + roundedY * 3 + roundedZ * 9;
        }

        public override double GetValue(Point3D p)
        {
            return GetValue(p.X, p.Y, p.Z);
        }

        private double ThirdsDivision(double value)
        {
            if (value < (2 / 3.0))
                return 0;

            if (value < (4 / 3.0))
                return 1;

            return 2;
        }

        public override double XSpacing { get => xSpacing; }
        public override double YSpacing { get => ySpacing; }
        public override double ZSpacing { get => zSpacing; }

        public override int[] Measures { get => measures; }

        public override double MinValue => 0;

        public override double MaxValue => 26;
    }
}
