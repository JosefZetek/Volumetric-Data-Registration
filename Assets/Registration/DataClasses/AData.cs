namespace DataView
{
    public abstract class AData
    {
        public abstract double XSpacing { get; set; }
        public abstract double YSpacing { get; set; }
        public abstract double ZSpacing { get; set; }

        public abstract int[] Measures { get; set; }

        public abstract double MinValue { get; }
        public abstract double MaxValue { get; }

        public abstract double GetValue(double x, double y, double z);
        public abstract double GetValue(Point3D p);

        public abstract double GetPercentile(double value);

        public double GetNormalizedValue(Point3D p)
        {
            return (GetValue(p) - MinValue) / (MaxValue - MinValue);
        }

        public double GetNormalizedValue(double x, double y, double z)
        {
            return (GetValue(x, y, z) - MinValue) / (MaxValue - MinValue);
        }

        public bool PointWithinBounds(Point3D p)
        {
            return (p.X < Measures[0] && p.Y < Measures[1] && p.Z < Measures[2] && p.X >= 0 && p.Y >= 0 && p.Z >= 0);
        }
    }
}
