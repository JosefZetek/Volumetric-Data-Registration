using System;
using UnityEngine;

namespace DataView
{
	class DataSlicer
	{
		private IData data;
        private const int DIMENSIONS = 3;

		public DataSlicer(IData data)
		{
			this.data = data;
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
            //t = Math.Min(Math.Max(0, t), 1);

            double cutPosition = t * (data.Measures[axis] - 1);

            Color[][] cutData = new Color[resolution.Height][];
            double[] coordinates = new double[DIMENSIONS];
            coordinates[axis] = cutPosition;

            /* Assigning index for axes that are going to vary in each iteration */
            int firstVariableIndex = (axis == 0) ? 1 : 0, secondVariableIndex = (axis == 2) ? 1 : 2;

            float currentNormalizedValue;

            for (int i = 0; i < resolution.Height; i++)
            {
                cutData[i] = new Color[resolution.Width];

                double secondDimensionProgress = ((double)i / ((double)resolution.Height - 1)) * (data.Measures[secondVariableIndex] - 1);
                coordinates[secondVariableIndex] = secondDimensionProgress;

                for (int j = 0; j < resolution.Width; j++)
                {
                    double firstDimensionProgress = ((double)j / ((double)resolution.Width - 1)) * (data.Measures[firstVariableIndex] - 1);
                    coordinates[firstVariableIndex] = firstDimensionProgress;

                    currentNormalizedValue = NormalizeValue(data.GetValue(coordinates[0], coordinates[1], coordinates[2]));

                    cutData[i][j] = new Color(currentNormalizedValue, currentNormalizedValue, currentNormalizedValue);

                    //Debug.Log("Cut data for [" + i + "][" + j + "] = " + currentValue + "from coordinates [" + coordinates[0] + ", " + coordinates[1] + ", " + coordinates[2] + "]");
                }
            }

            return cutData;
        }

        private float NormalizeValue(double value)
        {
            return (float)((value - data.MinValue) / (data.MaxValue - data.MinValue));
        }
    }
}

