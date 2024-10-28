using System;
using System.Collections.Generic;

namespace DataView
{
	public class DensityStructure
	{
		private DensityTree rootNode;
		private Transform3D[] transformations;

		public DensityStructure(Transform3D[] transformations)
		{
			rootNode = new DensityTree(transformations);
			this.transformations = transformations;
		}

		/// <summary>
		/// Selects transformation in list of transformations with biggest density parameter
		/// </summary>
		/// <param name="threshold">
		/// Default threshold - will be adjusted so that
		/// threshold > 0 && threshold <= 1. This parameter will be automatically
		/// changed if not suitable for given transformations
		/// </param>
		/// <param name="spreadParameter">Spread parameter - should be positive and not equal to 0</param>
		/// <returns>Returns transformation in biggest cluster of transformations</returns>
		public Transform3D FindBestTransformation(double threshold, double spreadParameter)
		{
			if (spreadParameter == 0)
				throw new Exception("Spread parameter can't be equal to 0.");

			spreadParameter = Math.Abs(spreadParameter);

            //Threshold needs to be within certain bounds not to cause exception in the calculations bellow
            threshold = Math.Max(double.Epsilon, threshold);
            threshold = Math.Min(1, threshold);

            return TransformationsDensityFilter(threshold, spreadParameter);
		}

		private Transform3D TransformationsDensityFilter(double threshold, double spreadParameter)
		{
			if (threshold > 2)
				throw new Exception("There is no pair of close transformations.");

            double radius = Math.Log(threshold);
            radius = Math.Sqrt(-radius) / spreadParameter;

            double bestDensity = 0;
            Transform3D bestTransformation = null;

            for (int i = 0; i < transformations.Length; i++)
            {
				List<Transform3D> result = FindPointsWithinRadius(transformations[i], radius);
				double currentDensity = 0;

				double exponent;
                //Density is calculated like SUM of e^(-(spreadParameter * distance)^2) for all close transformations
                foreach (Transform3D currentTransformation in result)
				{
					exponent = spreadParameter * transformations[i].SqrtDistanceTo(currentTransformation);
                    currentDensity += Math.Exp(-exponent*exponent);

                    if (bestDensity < currentDensity)
                    {
                        bestDensity = currentDensity;
                        bestTransformation = transformations[i];
                    }
                }
                    
            }

			//If there is no pair of transformations close to each other, increase the threshold
			return bestDensity == 0 ? TransformationsDensityFilter(threshold * 1.1, spreadParameter) : bestTransformation;
        }

		/// <summary>
		/// Finds Transformations within Radius calculated as
		/// r = sqrt(-ln(threshold))/spreadParameter
		/// </summary>
		/// <param name="queryPoint">Transformation to find </param>
		/// <param name="radius"></param>
		/// <returns></returns>
		private List<Transform3D> FindPointsWithinRadius(Transform3D queryPoint, double radius)
		{
			List<Transform3D> resultList = new List<Transform3D>();

			rootNode.ProximityQuery(queryPoint, radius, resultList);
			return resultList;
		}

		private class DensityTree
		{
			/// <summary>
			/// Transformation of this current node - leaf nodes have more than 1 element
			/// </summary>
			private Transform3D[] currentTransformation;

			/// <summary>
			/// Threshold 
			/// </summary>
			private double threshold;

			private DensityTree closeNode = null;
			private DensityTree farNode = null;


			public DensityTree(Transform3D[] transformations)
			{
				// If this is the leaf node
				if (transformations.Length <= 5)
				{
					this.currentTransformation = transformations;
					return;
				}

				this.currentTransformation = new Transform3D[] { transformations[0] };

				List<double> transformationDistances = new List<double>();

				List<Transform3D> furtherList = new List<Transform3D>();
				List<Transform3D> closerList = new List<Transform3D>();

				const int pivotIndex = 0;

				for (int i = 1; i < transformations.Length; i++)
				{
					transformationDistances.Add(transformations[pivotIndex].DistanceTo(transformations[i]));
				}

				QuickSelectClass testClass = new QuickSelectClass();
				this.threshold = testClass.QuickSelect<double>(transformationDistances, transformationDistances.Count / 2);

				for (int i = 0; i < transformationDistances.Count; i++)
				{
					if (transformationDistances[i] > threshold)
					{
						furtherList.Add(transformations[i]);
						continue;
					}

					closerList.Add(transformations[i]);
				}

				if (closerList.Count > 0)
					this.closeNode = new DensityTree(closerList.ToArray());

				if (furtherList.Count > 0)
					this.farNode = new DensityTree(furtherList.ToArray());
			}

            public void ProximityQuery(Transform3D queryPoint, double radius, List<Transform3D> result)
            {
                double distance = queryPoint.DistanceTo(this.currentTransformation[0]);

                // Check if the current node is within the radius
                if (distance < radius)
                {
                    result.AddRange(this.currentTransformation);
                }

                // Recursively search the subtrees based on distance and threshold
                if (this.closeNode != null && distance < this.threshold + radius)
                {
                    this.closeNode.ProximityQuery(queryPoint, radius, result);
                }

                if (this.farNode != null && distance > this.threshold - radius)
                {
                    this.farNode.ProximityQuery(queryPoint, radius, result);
                }
            }

            public Transform3D[] GetTransformation { get { return this.currentTransformation; } }
		}
	}


}

