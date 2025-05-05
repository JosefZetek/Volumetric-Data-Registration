using System;

namespace DataView
{
    public class CompoundFeatureComputer : IFeatureComputer
    {
        private IFeatureComputer[] featureComputers;

        public CompoundFeatureComputer(IFeatureComputer[] featureComputers)
        {
            this.featureComputers = featureComputers;
        }

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            FeatureVector[] featureVectors = new FeatureVector[featureComputers.Length];

            int numberOfFeatures = CalculateFeatureVectors(featureVectors, d, p);
            double[] features = ConcatenateFeatures(featureVectors, numberOfFeatures);

            return new FeatureVector(p, features);
        }

        private int CalculateFeatureVectors(FeatureVector[] featureVectors, AData d, Point3D p)
        {
            int numberOfFeatures = 0;

            for (int i = 0; i < featureComputers.Length; i++)
            {
                featureVectors[i] = featureComputers[i].ComputeFeatureVector(d, p);
                numberOfFeatures += featureVectors[i].Features.Length;
            }

            return numberOfFeatures;
        }

        private double[] ConcatenateFeatures(FeatureVector[] featureVectors, int numberOfFeatures)
        {
            double[] features = new double[numberOfFeatures];
            int index = 0;

            for (int i = 0; i < featureVectors.Length; i++)
            {
                Array.Copy(featureVectors[i].Features, 0, features, index, featureVectors[i].Features.Length);
                index += featureVectors[i].Features.Length;
            }

            return features;
        }
    }
}
