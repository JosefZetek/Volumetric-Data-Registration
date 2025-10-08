using System;

namespace DataView
{
    /// <summary>
    /// Fake Feature Computer uses point coordinates as the descriptor.
    /// </summary>
    public class FakeFeatureComputer : AFeatureComputer
    {
        private Transform3D transformation;
        private bool transformedSampling;

        public FakeFeatureComputer(Transform3D transformation)
        {
            this.transformation = transformation;
            this.transformedSampling = false;
        }

        public override void ComputeFeatureVector(AData d, Point3D p, double[] array, int startIndex)
        {
            CheckArrayDimensions(array, startIndex);

            Point3D point = !transformedSampling ? p : p.Copy().ApplyRotationTranslation(transformation);

            array[startIndex] = point.X;
            array[startIndex + 1] = point.Y;
            array[startIndex + 2] = point.Z;
        }

        public override int NumberOfFeatures => 3;

        public void SetTransformedSampling(bool transformedSampling)
        {
            this.transformedSampling = transformedSampling;
        }

        
    }
}

