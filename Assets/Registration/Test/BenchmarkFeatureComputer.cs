﻿using System;
using System.Collections.Generic;

namespace DataView
{
    public class BenchmarkFeatureComputer
	{
		private AData d;
		private IFeatureComputer featureComputer;
		private Random random;
		private List<Point2D> listOfPoints;

		public BenchmarkFeatureComputer(AData d, IFeatureComputer featureComputer)
		{
			this.d = d;
			this.featureComputer = featureComputer;

			random = new Random();
			listOfPoints = new List<Point2D>();
		}

		public void RunBenchmark(string outputFileName, int numberOfPoints)
		{
			Point3D firstPoint;
			Point3D secondPoint;

			while(listOfPoints.Count < numberOfPoints)
			{
				try
				{
                    firstPoint = GenerateRandomPoint();
                    secondPoint = GenerateRandomPoint();

                    double featureVectorDistance = featureComputer.ComputeFeatureVector(d, firstPoint).DistTo2(featureComputer.ComputeFeatureVector(d, secondPoint));
                    double pointDistance = firstPoint.Distance(secondPoint);

                    listOfPoints.Add(new Point2D(pointDistance, featureVectorDistance));
					if(listOfPoints.Count%100 == 0)
					{
						Console.WriteLine(listOfPoints.Count);
					}
                }
				catch
				{
					continue; //FeatureVector couldnt be calculated
				}
			}

			CSVWriter.WriteResult(outputFileName, "Real point distance", "FeatureVector distance squared", listOfPoints);
        }

		/// <summary>
		/// Generates random point in the data object
		/// </summary>
		/// <returns></returns>
		private Point3D GenerateRandomPoint()
		{
			return new Point3D(
				random.NextDouble() * d.MaxValueX,
				random.NextDouble() * d.MaxValueY,
				random.NextDouble() * d.MaxValueZ
			);
		}
	}
}

