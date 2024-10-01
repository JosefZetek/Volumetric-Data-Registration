using System;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace DataView
{
    class FeatureComputerVectorDeviation : IFeatureComputer
    {
        private QuickSelectClass quickSelectClass;

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {

            //return new FeatureVector(p, p.X, p.Y, p.Z, 0, 0);

            const int COUNT = 10_000;
            const int RADIUS = 5;

            Random random = new Random();
            quickSelectClass = new QuickSelectClass();

            int seed = random.Next();

            seed = 20; //Currently set to 20 for test purposes

            PointSurrounding pointSurrounding = GetPointSurrounding(d, p, COUNT, RADIUS, seed);
            return new FeatureVector(p, pointSurrounding.HighConcentrationValue, pointSurrounding.LowConcentrationValue, pointSurrounding.AngleXY, pointSurrounding.AngleXZ, pointSurrounding.DistributionValueAvg);
        }

        private PointSurrounding GetPointSurrounding(AData d, Point3D point, int count, double radius, int seed)
        {
            List<Point3D> pointsInSphere = GetSphere(point, radius, count, seed);

            List<double> pointValues = new List<double>();
            List<Point3D> lowPoints = new List<Point3D>();
            List<Point3D> highPoints = new List<Point3D>();

            for (int i = 0; i < pointsInSphere.Count; i++)
                pointValues.Add(d.GetValue(pointsInSphere[i]));

            double maxLow = quickSelectClass.QuickSelect<double>(pointValues, pointValues.Count / 2);

            double minLow = double.MaxValue;
            double minHigh = double.MaxValue;
            double maxHigh = double.MinValue;

            for (int i = 0; i<pointValues.Count; i++)
            {
                if (pointValues[i] <= maxLow)
                {
                    minLow = Math.Min(minLow, pointValues[i]);
                    lowPoints.Add(pointsInSphere[i]);
                    continue;
                }

                minHigh = Math.Min(minHigh, pointValues[i]);
                maxHigh = Math.Max(maxHigh, pointValues[i]);
                highPoints.Add(pointsInSphere[i]);
            }

            double distributionValueAvg = 0;
            Point3D wAvgPoint = new Point3D();
            double weightSum = 0;

            //Values min and max are the same
            if (Math.Abs(minHigh - maxHigh) < Double.Epsilon || Math.Abs(minLow - maxLow) < Double.Epsilon)
                throw new ArgumentException("Basis cannot be calculated because all sampled values in the point surrounding are the same.");

            Vector<double> highConcentrationVector;
            Vector<double> lowConcentrationVector;
            double highConcentrationValue;
            double lowConcentrationValue;

            //Min values
            for (int i = 0; i<lowPoints.Count; i++)
            {
                double currentValue = d.GetValue(lowPoints[i]);
                
                distributionValueAvg += d.GetPercentile(currentValue); //Add value distribution

                double pointWeight = (currentValue - minHigh) / (maxHigh - minHigh); //percentage from the overall range
                weightSum += pointWeight;

                wAvgPoint.X += pointsInSphere[i].X * pointWeight;
                wAvgPoint.Y += pointsInSphere[i].Y * pointWeight;
                wAvgPoint.Z += pointsInSphere[i].Z * pointWeight;
            }

            wAvgPoint.X /= weightSum;
            wAvgPoint.Y /= weightSum;
            wAvgPoint.Z /= weightSum;

            lowConcentrationVector = Vector<double>.Build.DenseOfArray(new double[] {
                wAvgPoint.X - point.X,
                wAvgPoint.Y - point.Y,
                wAvgPoint.Z - point.Z
            });


            try { lowConcentrationValue = d.GetValue(wAvgPoint); }
            catch
            {
                throw new ApplicationException("The average for low concentration vector has been calculated, but it is outside of bounds. This indicates theres a bug in the FeatureComputerVectorDeviation class that needs to be resolved.");
            }

            weightSum = 0;
            wAvgPoint = new Point3D();

            //Max values
            for (int i = 0; i < highPoints.Count; i++)
            {
                double currentValue = d.GetValue(highPoints[i]);

                distributionValueAvg += d.GetPercentile(currentValue); //Add value distribution

                double pointWeight = (currentValue - minHigh) / (maxHigh - minHigh); //percentage from the overall range
                weightSum += pointWeight;

                wAvgPoint.X += pointsInSphere[i].X * pointWeight;
                wAvgPoint.Y += pointsInSphere[i].Y * pointWeight;
                wAvgPoint.Z += pointsInSphere[i].Z * pointWeight;
            }

            wAvgPoint.X /= weightSum;
            wAvgPoint.Y /= weightSum;
            wAvgPoint.Z /= weightSum;

            highConcentrationVector = Vector<double>.Build.DenseOfArray(new double[] {
                wAvgPoint.X - point.X,
                wAvgPoint.Y - point.Y,
                wAvgPoint.Z - point.Z
            });

            try { highConcentrationValue = d.GetValue(wAvgPoint); }
            catch
            {
                throw new ApplicationException("The average for high concentration vector has been calculated, but it is outside of bounds. This indicates theres a bug in the FeatureComputerVectorDeviation class that needs to be resolved.");
            }


            double highConcentrationVectorLength = highConcentrationVector.L2Norm();
            highConcentrationVector = NormalizeVector(highConcentrationVector);
            lowConcentrationVector = NormalizeVector(lowConcentrationVector);

            //Normalize the highConcentration and lowConcentration values - percentage range (normalizing based on the distribution should be considered)
            highConcentrationValue = d.GetPercentile(highConcentrationValue);
            lowConcentrationValue = d.GetPercentile(lowConcentrationValue);

            //Calculation of the angle XY between concentration vectors
            Vector<double> firstVector = Vector<double>.Build.DenseOfArray(new double[] { highConcentrationVector[0], highConcentrationVector[1] });
            Vector<double> secondVector = Vector<double>.Build.DenseOfArray(new double[] { lowConcentrationVector[0], lowConcentrationVector[1] });
            double angleXY = calculateAngle(firstVector, secondVector);
            angleXY /= Math.PI;

            //Calculation of the angle XZ between concentration vectors
            firstVector = Vector<double>.Build.DenseOfArray(new double[] { highConcentrationVector[0], highConcentrationVector[2] });
            secondVector = Vector<double>.Build.DenseOfArray(new double[] { lowConcentrationVector[0], lowConcentrationVector[2] });
            double angleXZ = calculateAngle(firstVector, secondVector);
            angleXZ /= Math.PI;

            firstVector = Vector<double>.Build.DenseOfArray(new double[] { highConcentrationVector[1], highConcentrationVector[2] });
            secondVector = Vector<double>.Build.DenseOfArray(new double[] { lowConcentrationVector[1], lowConcentrationVector[2] });
            double angleYZ = calculateAngle(firstVector, secondVector);
            angleYZ /= Math.PI;

            return new PointSurrounding(highConcentrationValue, lowConcentrationValue, angleXY, angleXZ, angleYZ);
        }

        private static double calculateAngle(Vector<double> firstVector, Vector<double> secondVector)
        {
            return Math.Acos(DotProduct(firstVector, secondVector) / (firstVector.L2Norm() * secondVector.L2Norm()));
        }

        /// <summary>
        /// Calculates the dot product for the two given vectors
        /// </summary>
        /// <param name="vector1">First vector</param>
        /// <param name="vector2">Second vector</param>
        /// <returns>Returns the scalar product of the two given vectors</returns>
        /// <exception cref="ArgumentException"></exception>
        private static double DotProduct(Vector<double> vector1, Vector<double> vector2)
        {
            int upperBound = vector1.AsArray().Length;

            if (upperBound != vector2.AsArray().Length)
                throw new ArgumentException("Vectors should be the same size");

            double result = 0;
            for (int i = 0; i < upperBound; i++)
                result += vector1[i] * vector2[i];

            return result;
        }

        private static bool compareWithTolerance(double numberA, double numberB)
        {
            double epsilon = 0.00000001;
            if ((numberA + epsilon > numberB) && (numberA - epsilon < numberB))
                return true;

            return false;
        }

        /// <summary>
        /// Normalizes vector passsed as a parameter
        /// </summary>
        /// <param name="vector">Vector to normalize</param>
        /// <returns>
        /// Returns normalized vector if its magnitude is not equal to 0.
        /// Otherwise it returns Vector with 3 rows and 0 columns.
        /// </returns>
        private static Vector<double> NormalizeVector(Vector<double> vector)
        {

            double magnitude = vector.L2Norm();

            //Prevents from crashing when denominator(magnitude) is equal to 0
            if (magnitude == 0)
                return Vector<double>.Build.Dense(3, 0);

            return vector / magnitude;
        }

        /// <summary>
        /// This method returns list of points whose maximum distance is defined as radius from the reference point passed as parameter.
        /// </summary>
        /// <param name="p">Point around which should the points be generated</param>
        /// <param name="r">Maximum distance of generated points to the reference point</param>
        /// <param name="count">Number of points</param>
        /// <param name="seed">Seed for generating the points</param>
        /// <returns>This method returns list of points. None of them is further than radius from the reference point.</returns>
        private List<Point3D> GetSphere(Point3D p, double r, int count, int seed)
        {
            List<Point3D> points = GetSpheresIntersticePoints(point: p, minRadius: 0, maxRadius: r, count: count, seed: seed);
            return points;
        }

        /// <summary>
        /// This method returns list of points that are within certain distance defined as minRadius and maxRadius from the center of the sphere passed as a point
        /// </summary>
        /// <param name="point">Center of a sphere</param>
        /// <param name="minRadius">Generated point's minimum distance from the center of a sphere</param>
        /// <param name="maxRadius">Generated point's maximum distance from the center of a sphere</param>
        /// <returns>Returns list of points that are within certain distance from the center of the sphere.</returns>
        private List<Point3D> GetSpheresIntersticePoints(Point3D point, double minRadius, double maxRadius, int count, int seed)
        {
            if (minRadius > maxRadius)
                throw new ArgumentException("The min radius should be smaller than the max radius");

            Random random = new Random(seed);

            List<Point3D> listOfPoints = new List<Point3D>();
            while (listOfPoints.Count < count)
            {
                double radius = GetRandomDouble(minRadius, maxRadius, random);
                double angleTheta = random.NextDouble() * 2 * Math.PI;
                double anglePhi = random.NextDouble() * 2 * Math.PI;
                double x = radius * Math.Cos(anglePhi) * Math.Sin(angleTheta);
                double y = radius * Math.Sin(angleTheta) * Math.Sin(anglePhi);
                double z = radius * Math.Cos(angleTheta);


                if (x < 0 || y < 0 || z < 0)
                    continue;

                listOfPoints.Add(new Point3D(x, y, z));
            }

            return listOfPoints;
        }

        /// <summary>
        /// Returns random values within specified range
        /// </summary>
        /// <param name="minValue">Min value that could be generated</param>
        /// <param name="maxValue">Max value that could be generated</param>
        /// <returns>Returns random value within a specified range</returns>
        private double GetRandomDouble(double minValue, double maxValue, Random random)
        {
            return random.NextDouble() * (maxValue - minValue) + minValue;
        }

        private double QuickSelect()
        {
            /*
             * function quickSelect (list, left, right, k)
  if left = right
    return list [left]
  Select a pivotIndex between left and right
  pivotIndex := partition (list, left, right, pivotIndex)
  if k = pivotIndex
    return list [k]
  else if k < pivotIndex
    right := pivotIndex - 1
  else
    left := pivotIndex + 1
             */
            return 0;
        }

        /// <summary>
        /// This class is a messenger for important
        /// values describing point's surrounding
        /// These values are then used as for feature vector creating within this class
        /// </summary>
        private class PointSurrounding
        {
            private double distributionValueAvg;

            /// <summary>
            /// Value in the high concentration position (point + highConcentratoinVector)
            /// </summary>
            private double highConcentrationValue;

            /// <summary>
            /// Value in the low concentration position (point + lowConcentratoinVector)
            /// </summary>
            private double lowConcentrationValue;

            /// <summary>
            /// Angle between the vectors - view from above
            /// </summary>
            private double angleXY;

            /// <summary>
            /// Angle between the vectors - vew from side
            /// </summary>
            private double angleXZ;


            public PointSurrounding(double distributionValueAvg, double highConcentrationValue, double lowConcentrationValue, double angleXY, double angleXZ)
            {
                //DistributionValueAverage
                this.distributionValueAvg = distributionValueAvg;

                //Values for the places
                this.highConcentrationValue = highConcentrationValue;
                this.lowConcentrationValue = lowConcentrationValue;

                //Angles
                this.angleXY = angleXY;
                this.angleXZ = angleXZ;
            }


            //GETTERS
            public double DistributionValueAvg
            {
                get { return distributionValueAvg; }
            }

            public double HighConcentrationValue
            {
                get { return highConcentrationValue; }
            }

            public double LowConcentrationValue
            {
                get { return lowConcentrationValue; }
            }

            public double AngleXY
            {
                get { return angleXY; }
            }

            public double AngleXZ
            {
                get { return angleXZ; }
            }
        }
    }


}
