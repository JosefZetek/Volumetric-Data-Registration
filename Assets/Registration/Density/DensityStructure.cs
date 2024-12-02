using System;
using System.Collections.Generic;

namespace DataView
{
	public class DensityStructure
	{
		private DensityTree rootNode;
		private List<Transform3D> transformations;

		public DensityStructure(List<Transform3D> transformations)
		{
			rootNode = new DensityTree(transformations);
			this.transformations = transformations;
		}


		/// <summary>
		/// Finds density peak and outputs particular 
		/// </summary>
		/// <param name="threshold">Minimal value to be considered</param>
		/// <param name="spreadParameter">Parameter defines how spread the transformations are</param>
		/// <returns></returns>
		public Transform3D FindBestTransformation(double threshold, double spreadParameter)
		{
            return TransformationsDensityFilter(threshold, spreadParameter);
		}

		private Transform3D TransformationsDensityFilter(double threshold, double spreadParameter)
		{
            double maxDistance = Math.Sqrt(Math.Log(threshold) / (-spreadParameter));

            double bestDensity = 0;
			Transform3D bestTransformation = null;

			for (int i = 0; i < transformations.Count; i++)
			{
				List<Transform3D> result = FindPointsWithinRadius(transformations[i], maxDistance);
				double currentDensity = 0;

				//Density is calculated like SUM of e^(-(spreadParameter * distance)^2) for all close transformations
				foreach (Transform3D currentTransformation in result)
				{
					currentDensity += Math.Exp(-spreadParameter * Math.Pow(transformations[i].RelativeDistanceTo(currentTransformation), 2));
				}

				if (bestDensity < currentDensity)
				{
					bestDensity = currentDensity;
					bestTransformation = transformations[i];
				}

			}

			//If there is no pair of transformations close to each other, increase the threshold
			return bestDensity == 0 ? TransformationsDensityFilter(maxDistance * 1.1, spreadParameter) : bestTransformation;
		}

		/// <summary>
		/// Finds Transformations within Radius calculated as
		/// r = sqrt(-ln(threshold))/spreadParameter
		/// </summary>
		/// <param name="queryPoint">Transformation to find </param>
		/// <param name="distance"></param>
		/// <returns></returns>
		private List<Transform3D> FindPointsWithinRadius(Transform3D queryPoint, double distance)
		{
			List<Transform3D> resultList = new List<Transform3D>();

			rootNode.ProximityQuery(queryPoint, distance, resultList);
			return resultList;
		}
	}
}

