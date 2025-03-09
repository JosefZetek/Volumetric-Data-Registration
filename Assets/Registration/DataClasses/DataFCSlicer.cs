using System;
using UnityEngine;

namespace DataView
{
    public class DataFCSlicer : IDataSlicer
    {
        private AData referenceData;
        private FeatureComputerISOSurfaceCurvature featureComputer;

        private const int DIMENSIONS = 3;

        public DataFCSlicer(AData data)
        {
            this.referenceData = data;
            this.featureComputer = new FeatureComputerISOSurfaceCurvature();
        }

        public Color[][] Cut(double t, int axis, CutResolution resolution)
        {
            /* Constraining t to be within range */
            t = Math.Min(Math.Max(0, t), 1);

            double cutPosition = t * referenceData.Bounds[axis];

            Color[][] cutData = new Color[resolution.Height][];
            double[] coordinates = new double[DIMENSIONS];
            coordinates[axis] = cutPosition;

            /* Assigning index for axes that are going to vary in each iteration */
            int firstVariableIndex = (axis == 0) ? 1 : 0, secondVariableIndex = (axis == 2) ? 1 : 2;

            float currentNormalizedValue;
            FeatureVector featureVector;

            for (int i = 0; i < resolution.Height; i++)
            {
                cutData[i] = new Color[resolution.Width];

                double secondDimensionProgress = ((double)i / ((double)resolution.Height - 1)) * referenceData.Bounds[secondVariableIndex];
                coordinates[secondVariableIndex] = secondDimensionProgress;

                for (int j = 0; j < resolution.Width; j++)
                {
                    double firstDimensionProgress = ((double)j / ((double)resolution.Width - 1)) * referenceData.Bounds[firstVariableIndex];
                    coordinates[firstVariableIndex] = firstDimensionProgress;

                    featureVector = featureComputer.ComputeFeatureVector(referenceData, new Point3D(coordinates[0], coordinates[1], coordinates[2]));
                    currentNormalizedValue = Constrain((float)-featureVector.Features[0], 0, 1);

                    Point3D currentPoint = new Point3D(coordinates[0], coordinates[1], coordinates[2]);

                    if (currentNormalizedValue > 0.4 & currentPoint.Distance(new Point3D(2.5, 2.5, 2.5)) > 2.5)
                        Debug.Log($"Position: {currentPoint} and current value: {currentNormalizedValue}");



                    cutData[i][j] = new Color(currentNormalizedValue, currentNormalizedValue, currentNormalizedValue);
                }
            }

            return cutData;
        }

        private float Constrain(float constrainedValue, float minValue, float maxValue)
        {
            return Mathf.Max(Mathf.Min(constrainedValue, maxValue), minValue);
        }
    }
}

