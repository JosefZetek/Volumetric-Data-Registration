using System;

namespace DataView
{
    public abstract class AFeatureComputer
    {
        public abstract int NumberOfFeatures { get; }
        public abstract void ComputeFeatureVector(AData d, Point3D p, double[] array, int startIndex);

        protected void CheckArrayDimensions(double[] array, int startIndex)
        {
            if (array.Length < (startIndex + NumberOfFeatures))
                throw new ArgumentException("Invalid array size");
        }

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            double[] features = new double[NumberOfFeatures];
            ComputeFeatureVector(d, p, features, 0);

            return new FeatureVector(p, features);
        }
    }
}
