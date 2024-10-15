using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    /// <summary>
    /// This sampler outputs identical points for both micro and macro data
    /// </summary>
    public class SamplerFake : ISampler
	{
        private Random random;
        private List<Point3D> points;
        private double percentage;

        private double randomIncrement;
        private bool isMicro = true;

        Point3D maxBounds;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <param name="microData"></param>
        /// <param name="macroData"></param>
        /// <param name="percentage">Percentage is a value between 0 and 1</param>
		public SamplerFake(AData microData, AData macroData, double percentage, double randomIncrement)
		{
            int seed = 100;
            this.random = new Random(seed);
            this.points = new List<Point3D>();
            this.percentage = Math.Min(Math.Max(0, percentage), 1);
            this.randomIncrement = randomIncrement; 

            double maxX = Math.Min(microData.MaxValueX, macroData.MaxValueX);
            double maxY = Math.Min(microData.MaxValueY, macroData.MaxValueY);
            double maxZ = Math.Min(microData.MaxValueZ, macroData.MaxValueZ);

            //Subtracting 1 since that is the radius for calculating rotation
            maxBounds = new Point3D(maxX, maxY, maxZ);
		}

        public Point3D[] Sample(AData d, int count)
        {
            int numberOfSamePoints = (int)(count * percentage);
            GenerateSamePoints(numberOfSamePoints);

            Point3D[] sampledPoints = new Point3D[count];

            Transform3D transformation = new Transform3D(
                Matrix<double>.Build.DenseIdentity(3),
                Vector<double>.Build.Dense(3)
            );

            if(isMicro)
            {
                VolumetricData volumetricData = (VolumetricData)d;
                transformation = volumetricData.GetTransformation().GetInverseTransformation();
                isMicro = !isMicro;
            }

            Point3D currentPoint;
            /* Common points for both micro and macro data are modified with random increments*/
            for (int i = 0; i < numberOfSamePoints; i++)
            {
                currentPoint = new Point3D(points[i].X, points[i].Y, points[i].Z);
                sampledPoints[i] = currentPoint.ApplyRotationTranslation(transformation);

                Console.WriteLine("sampled point: " + sampledPoints[i]);
                
                currentPoint.X += random.NextDouble() * randomIncrement;
                currentPoint.Y += random.NextDouble() * randomIncrement;
                currentPoint.Z += random.NextDouble() * randomIncrement;
                
            }

            for(int i = numberOfSamePoints; i < count; i++)
            {
                currentPoint = new Point3D(random.NextDouble() * maxBounds.X, random.NextDouble() * maxBounds.Y, random.NextDouble() * maxBounds.Z);
                sampledPoints[i] = currentPoint.ApplyTranslationRotation(transformation);
            }

            //ShuffleArray(sampledPoints);
            return sampledPoints;
        }

        private void ShuffleArray(Point3D[] point)
        {
            for(int i = 0; i<point.Length; i++)
            {
                Point3D temp = point[i];
                int randomIndex = random.Next(point.Length);
                point[i] = point[randomIndex];
                point[randomIndex] = temp;
            }
        }

        private void GenerateSamePoints(int count)
        {
            while(points.Count < count)
            {
                points.Add(new Point3D(random.NextDouble() * maxBounds.X, random.NextDouble() * maxBounds.Y, random.NextDouble() * maxBounds.Z));
            }
        }
    }
}

