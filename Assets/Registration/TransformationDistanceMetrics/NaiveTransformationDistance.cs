using System;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{

    /// <summary>
    /// Class calculating distance between two given transformations as squared differece between each vertex after applying transformation on it
    /// Highly ineficient, use for debug purposes only
    /// </summary>
    class NaiveTransformationDistance : ITransformationDistance
    {

        private IData data;

        /// <summary>
        /// Constructor takes in data object and saves it
        /// </summary>
        /// <param name="data">Instance of IData for object on which the transformations are going to be applied</param>
        public NaiveTransformationDistance(IData data)
        {
            this.data = data;
        }

        public double GetSqrtTransformationDistance(Transform3D transformation1, Transform3D transformation2)
        {
            return Math.Sqrt(GetTransformationsDistance(transformation1, transformation2));
        }

        public double GetTransformationsDistance(Transform3D transformation1, Transform3D transformation2)
        {
            double distanceSquared = 0;
            Vector<double> currentVector;
            Vector<double> firstTransformationResult;
            Vector<double> secondTransformationResult;

            for(double x = 0; x < data.Measures[0]; x += data.XSpacing)
            {
                for (double y = 0; y < data.Measures[1]; y += data.YSpacing)
                {
                    for(double z = 0; z < data.Measures[2]; z += data.ZSpacing)
                    {
                        GetVector(out currentVector, x, y, z);

                        firstTransformationResult = transformation1.RotationMatrix.Multiply(currentVector);
                        firstTransformationResult += transformation1.TranslationVector;

                        secondTransformationResult = transformation2.RotationMatrix.Multiply(currentVector);
                        secondTransformationResult += transformation2.TranslationVector;

                        distanceSquared += (firstTransformationResult - secondTransformationResult).L2Norm();
                    }
                }
            }

            return distanceSquared;
        }

        private void GetVector(out Vector<double> currentVector, double x, double y, double z)
        {
            currentVector = Vector<double>.Build.DenseOfArray(new double[]
            {
                x, y, z
            });
        }
    }
}

