using System;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace DataView
{

    /// <summary>
    /// Class calculating distance between two given transformations as squared differece between each vertex after applying transformation on it
    /// Highly ineficient, use for debug purposes only
    /// </summary>
    public class NaiveTransformationDistance : ITransformationDistance
    {

        private AData data;

        /// <summary>
        /// Constructor takes in data object and saves it
        /// </summary>
        /// <param name="data">Instance of IData for object on which the transformations are going to be applied</param>
        public NaiveTransformationDistance(AData data)
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
            //double currentValue;

            Vector<double> currentVector;
            Vector<double> firstTransformationResult;
            Vector<double> secondTransformationResult;

            for (double xIndex = 0; xIndex < data.Measures[0]; xIndex++)
            {
                for (int yIndex = 0; yIndex < data.Measures[1]; yIndex++)
                {
                    for (int zIndex = 0; zIndex < data.Measures[2]; zIndex++)
                    {
                        currentVector = GetVector(xIndex * data.XSpacing, yIndex * data.YSpacing, zIndex * data.ZSpacing);

                        firstTransformationResult = transformation1.RotationMatrix.Multiply(currentVector);
                        firstTransformationResult += transformation1.TranslationVector;

                        secondTransformationResult = transformation2.RotationMatrix.Multiply(currentVector);
                        secondTransformationResult += transformation2.TranslationVector;

                        distanceSquared += Math.Pow((firstTransformationResult - secondTransformationResult).L2Norm(), 2);
                    }
                }
            }

            return distanceSquared;
        }

        private Vector<double> GetVector(double x, double y, double z)
        {
            return Vector<double>.Build.DenseOfArray(new double[]
            {
                x, y, z
            });
        }
    }
}

