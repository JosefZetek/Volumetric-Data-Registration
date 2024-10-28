using System;
using UnityEngine;

namespace DataView
{
	public class TransformedDataSlicer: IDataSlicer
	{
		private AData macroData;
        private AData microData;
        private Transform3D transformation;

        private const int DIMENSIONS = 3;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="macroData">Macro data</param>
        /// <param name="microData">Micro data</param>
        /// <param name="transformation">Transformation that aligns microData onto macroData by calculating (Rx + t)</param>
		public TransformedDataSlicer(AData macroData, AData microData, Transform3D transformation)
		{
			this.macroData = macroData;
            this.microData = microData;
            this.transformation = transformation;
		}

        private float NormalizeValue(double value)
        {
            float normalizedValue = (float)((value - this.macroData.MinValue) / (this.macroData.MaxValue - this.macroData.MinValue));
            return Math.Max(Math.Min(normalizedValue, 1), 0);
        }

        public Color[][] Cut(double t, int axis, CutResolution resolution)
        {
            Point3D microDataPoint;

            /* Constraining t to be within range */
            t = Math.Min(Math.Max(0, t), 1);

            double cutPosition = t * macroData.Bounds[axis];

            Color[][] cutData = new Color[resolution.Height][];
            double[] coordinates = new double[DIMENSIONS];
            coordinates[axis] = cutPosition;

            /* Assigning index for axes that are going to vary in each iteration */
            int firstVariableIndex = (axis == 0) ? 1 : 0, secondVariableIndex = (axis == 2) ? 1 : 2;

            float currentNormalizedValue;

            for (int i = 0; i < resolution.Height; i++)
            {
                cutData[i] = new Color[resolution.Width];

                double secondDimensionProgress = ((double)i / ((double)resolution.Height - 1)) * macroData.Bounds[secondVariableIndex];
                coordinates[secondVariableIndex] = secondDimensionProgress;

                for (int j = 0; j < resolution.Width; j++)
                {
                    double firstDimensionProgress = ((double)j / ((double)resolution.Width - 1)) * macroData.Bounds[firstVariableIndex];
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
                        currentNormalizedValue = (float)macroData.GetNormalizedValue(coordinates[0], coordinates[1], coordinates[2]);
                        cutData[i][j] = new Color(currentNormalizedValue, currentNormalizedValue, currentNormalizedValue);
                    }
                }
            }

            return cutData;
        }
    }
}

