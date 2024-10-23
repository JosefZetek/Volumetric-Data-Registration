using System;
using UnityEngine;

namespace DataView
{
	public class DataSlicer
	{
		private AData referenceData;
        private const int DIMENSIONS = 3;

		public DataSlicer(AData data)
		{
			this.referenceData = data;
		}

        /// <summary>
        /// Cuts image and outputs 2D array with data organized as follows
        /// [row][column], [0][0] represents bottom left corner
        /// </summary>
        /// <param name="t">Slice position (between 0-1)</param>
        /// <param name="axis">Number of axis [0 = x, 1 = y, 2 = z]</param>
        /// <param name="resolution">Resolution of the cut (values in between are </param>
        /// <returns></returns>
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

            for (int i = 0; i < resolution.Height; i++)
            {
                cutData[i] = new Color[resolution.Width];

                double secondDimensionProgress = ((double)i / ((double)resolution.Height - 1)) * referenceData.Bounds[secondVariableIndex];
                coordinates[secondVariableIndex] = secondDimensionProgress;

                for (int j = 0; j < resolution.Width; j++)
                {
                    double firstDimensionProgress = ((double)j / ((double)resolution.Width - 1)) * referenceData.Bounds[firstVariableIndex];
                    coordinates[firstVariableIndex] = firstDimensionProgress;

                    currentNormalizedValue = (float)referenceData.GetNormalizedValue(coordinates[0], coordinates[1], coordinates[2]);
                    cutData[i][j] = new Color(currentNormalizedValue, currentNormalizedValue, currentNormalizedValue);
                }
            }

            return cutData;
        }

        /// <summary>
        /// Cuts image and outputs 2D array with data organized as follows
        /// [row][column], [0][0] represents bottom left corner.
        /// Data thats been used in constructor are used as a refference (black tint).
        /// Where microData overlaps with refference data, it replaces the value with green tint.
        /// </summary>
        /// <param name="t">Value between 0-1</param>
        /// <param name="axis">Number of axis [0 = x, 1 = y, 2 = z]</param>
        /// <param name="microData">Micro data that replaces refference data when they overlap.</param>
        /// <param name="transformation">Transformation that aligns microData onto macroData</param>
        /// <returns></returns>
        public Color[][] TransformationCut(double t, int axis, AData microData, Transform3D transformation, CutResolution resolution)
        {
            Point3D microDataPoint;

            /* Constraining t to be within range */
            t = Math.Min(Math.Max(0, t), 1);

            double cutPosition = t * referenceData.Bounds[axis];

            Color[][] cutData = new Color[resolution.Height][];
            double[] coordinates = new double[DIMENSIONS];
            coordinates[axis] = cutPosition;

            /* Assigning index for axes that are going to vary in each iteration */
            int firstVariableIndex = (axis == 0) ? 1 : 0, secondVariableIndex = (axis == 2) ? 1 : 2;

            float currentNormalizedValue;

            for (int i = 0; i < resolution.Height; i++)
            {
                cutData[i] = new Color[resolution.Width];

                double secondDimensionProgress = ((double)i / ((double)resolution.Height - 1)) * referenceData.Bounds[secondVariableIndex];
                coordinates[secondVariableIndex] = secondDimensionProgress;

                for (int j = 0; j < resolution.Width; j++)
                {
                    double firstDimensionProgress = ((double)j / ((double)resolution.Width - 1)) * referenceData.Bounds[firstVariableIndex];
                    coordinates[firstVariableIndex] = firstDimensionProgress;


                    microDataPoint = new Point3D(coordinates[0], coordinates[1], coordinates[2]);
                    microDataPoint = microDataPoint.ApplyTranslationRotation(transformation.GetInverseTransformation());

                    if (microData.PointWithinBounds(microDataPoint))
                    {
                        //currentNormalizedValue = (float)microData.GetNormalizedValue(microDataPoint);
                        currentNormalizedValue = NormalizeValue(microData.GetValue(microDataPoint));
                        cutData[i][j] = new Color(0, currentNormalizedValue, 0);
                    }
                    else
                    {
                        /* If micro point isnt within bounds, macro point is used */
                        currentNormalizedValue = (float)referenceData.GetNormalizedValue(coordinates[0], coordinates[1], coordinates[2]);
                        cutData[i][j] = new Color(currentNormalizedValue, currentNormalizedValue, currentNormalizedValue);
                    }
                }
            }

            return cutData;
        }

        private float NormalizeValue(double value)
        {
            float normalizedValue = (float)((value - this.referenceData.MinValue) / (this.referenceData.MaxValue - this.referenceData.MinValue));
            return Math.Max(Math.Min(normalizedValue, 1), 0);
        }
    }
}

