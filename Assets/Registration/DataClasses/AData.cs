namespace DataView
{
    public abstract class AData : AMockObject
    {
        /* Additional methods */
        public abstract double MinValue { get; }
        public abstract double MaxValue { get; }

        public abstract double GetPercentile(double value);

        public double GetNormalizedValue(Point3D p)
        {
            return (GetValue(p) - MinValue) / (MaxValue - MinValue);
        }

        public double GetNormalizedValue(double x, double y, double z)
        {
            return (GetValue(x, y, z) - MinValue) / (MaxValue - MinValue);
        }
    }
}
