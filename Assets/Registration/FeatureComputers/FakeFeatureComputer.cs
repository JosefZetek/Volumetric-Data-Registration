using System;

namespace DataView
{
    /// <summary>
    /// Fake Feature Computer uses point coordinates as the descriptor.
    /// </summary>
    class FakeFeatureComputer : IFeatureComputer
    {
        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            return new FeatureVector(p, new double[] { p.X, p.Y, p.Z });
        }
    }
}

