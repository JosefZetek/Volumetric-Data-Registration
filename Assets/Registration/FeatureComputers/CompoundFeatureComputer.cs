using System;
using System.Linq;

namespace DataView
{
    public class CompoundFeatureComputer : AFeatureComputer
    {
        private AFeatureComputer[] featureComputers;
        private int numberOfFeatures;


        public CompoundFeatureComputer(AFeatureComputer[] featureComputers)
        {
            this.featureComputers = featureComputers;
            this.numberOfFeatures = featureComputers.Sum(fc => fc.NumberOfFeatures);
        }

        public override int NumberOfFeatures => numberOfFeatures;

        public override void ComputeFeatureVector(AData d, Point3D p, double[] array, int startIndex)
        {
            int currentIndex = startIndex;

            CheckArrayDimensions(array, startIndex);

            for(int i = 0; i<featureComputers.Length; i++)
            {
                featureComputers[i].ComputeFeatureVector(d, p, array, currentIndex);
                currentIndex += featureComputers[i].NumberOfFeatures;
            }
        }
    }
}
