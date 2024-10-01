using System;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{

    /// <summary>
    /// Class that uses Gradient Descent to find point with closest value to target value
    /// This found point should be in the surrounding of given original point
    /// Doesn't significantly improve accuracy of points at the moment, tested with cost function implemented as difference of feature vectors and difference in points distribution values
    /// </summary>
    class PointApproximation
	{

		private IData data;
		private IFeatureComputer featureComputer;

		private FeatureVector targetPointValue;
		private double learningRate;
		private double maxStep;
		private double convergenceValue;

        /// <summary>
		/// 
		/// </summary>
        /// <param name="data">Interface for getting data for a given point to be moved towards optimum (to have as close value to target value as possible)</param>
		/// <param name="featureComputer">Class used for computing feature vectors of given points</param>
        /// <param name="learningRate">Coeficient that influences programs sensitivity to calculate result. The lower it is, the more accurate the result is, but the more time it takes to be fully calculated.</param>
		/// <param name="maxStep">Maximum possible step in a direction of a derivative</param>
        public PointApproximation(IData data, IFeatureComputer featureComputer, double learningRate, double maxStep, double convergenceValue)
		{
			this.data = data;
			this.featureComputer = featureComputer;

			this.learningRate = learningRate;
			this.maxStep = maxStep;
			this.convergenceValue = convergenceValue;
		}

        public Point3D FindClosePoint(Point3D originalPoint, FeatureVector targetPointValue, double epsilon)
		{
			Point3D currentPoint = originalPoint.Copy();

			this.targetPointValue = targetPointValue;

			double previousDx = double.MaxValue;
            double previousDy = double.MaxValue;
            double previousDz = double.MaxValue;

			double dx;
			double dy;
			double dz;

            while (true)
			{

				//ShowSurroundingValues(currentPoint.X, currentPoint.Y, currentPoint.Z, epsilon);

				Point3D offsetPointX = new Point3D(currentPoint.X + epsilon, currentPoint.Y, currentPoint.Z);
				Point3D offsetPointY = new Point3D(currentPoint.X, currentPoint.Y + epsilon, currentPoint.Z);
                Point3D offsetPointZ = new Point3D(currentPoint.X, currentPoint.Y, currentPoint.Z + epsilon);


                if (OutOfBounds(offsetPointX.X, offsetPointY.Y, offsetPointZ.Z))
                    break;


				try
				{
                    dx = Math.Min(maxStep, learningRate * DerivativeFunction(currentPoint, offsetPointX, epsilon));
                    dy = Math.Min(maxStep, learningRate * DerivativeFunction(currentPoint, offsetPointY, epsilon));
                    dz = Math.Min(maxStep, learningRate * DerivativeFunction(currentPoint, offsetPointZ, epsilon));
                }
				catch
				{
					return currentPoint;
				}
                

				dx = Math.Max(-maxStep, dx);
                dy = Math.Max(-maxStep, dy);
                dz = Math.Max(-maxStep, dz);


                double newX = currentPoint.X - dx;
                double newY = currentPoint.Y - dy;
                double newZ = currentPoint.Z - dz;

				if (FunctionDiverges(previousDx, dx) && FunctionDiverges(previousDy, dy) && FunctionDiverges(previousDz, dz))
					break;

				if (FunctionConverges(dx, convergenceValue) && FunctionConverges(dy, convergenceValue) && FunctionConverges(dz, convergenceValue))
					break;

                if (OutOfBounds(newX, newY, newZ))
                    break;

                currentPoint.X = newX;
                currentPoint.Y = newY;
                currentPoint.Z = newZ;

				previousDx = dx;
				previousDy = dy;
				previousDz = dz;
            }

			return currentPoint;
		}

		private double DerivativeFunction(Point3D currentPoint, Point3D offsetPoint, double epsilon)
		{
			return (CalculateFunction(offsetPoint) - CalculateFunction(currentPoint)) / epsilon;
		}

		private double CalculateFunction(Point3D point)
		{
			return targetPointValue.DistTo2(featureComputer.ComputeFeatureVector(data, point));
			//return Math.Abs(targetPointValue - data.GetValueDistribution(data.GetValue(point)));
		}

		private bool FunctionConverges(double derivative, double epsilon)
		{
			return Math.Abs(derivative) < epsilon;
        }

		private bool FunctionDiverges(double previousDerivative, double currentDerivative)
		{
			return (previousDerivative < currentDerivative);
		}

		private bool OutOfBounds(double x, double y, double z)
		{
			if (data.Measures[0] <= x)
				return true;

            if (data.Measures[1] <= y)
                return true;

            if (data.Measures[2] <= z)
                return true;

			return (x < 0 || y < 0 || z < 0);
        }
	}
}

