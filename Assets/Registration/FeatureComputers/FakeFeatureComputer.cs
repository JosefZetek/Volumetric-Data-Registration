namespace DataView
{
    /// <summary>
    /// Fake Feature Computer uses point coordinates as the descriptor.
    /// </summary>
    public class FakeFeatureComputer : IFeatureComputer
    {
        private Transform3D transformation;
        private bool transformedSampling;

        public FakeFeatureComputer(Transform3D transformation)
        {
            this.transformation = transformation;
            this.transformedSampling = false;
        }

        public void SetTransformedSampling(bool transformedSampling)
        {
            this.transformedSampling = transformedSampling;
        }

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            Point3D point = p.Copy();

            if (transformedSampling)
                point = point.ApplyRotationTranslation(transformation);

            return new FeatureVector(p, new double[] { point.X, point.Y, point.Z });
        }
    }
}

