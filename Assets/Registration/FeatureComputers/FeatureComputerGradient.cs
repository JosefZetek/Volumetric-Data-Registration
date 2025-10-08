using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class FeatureComputerGradient : AFeatureComputer
    {
        private double spreadParameterX;
        private double spreadParameterY;
        private double spreadParameterZ;

        public override int NumberOfFeatures => 2;

        public override void ComputeFeatureVector(AData d, Point3D p, double[] array, int startIndex)
        {
            Point3D nearestGridPoint = new Point3D(
                RoundToNearestSpacingMultiplier(p.X, d.XSpacing),
                RoundToNearestSpacingMultiplier(p.Y, d.YSpacing),
                RoundToNearestSpacingMultiplier(p.Z, d.ZSpacing)
            );


            CalculateSpreadParameter(d, 0.8);
            double a = ComputeGradient(p, d, nearestGridPoint, 5);

            CalculateSpreadParameter(d, 0.9);
            double b = ComputeGradient(p, d, nearestGridPoint, 7);

            array[startIndex] = a;
            array[startIndex + 1] = b;
        }

        private void CalculateSpreadParameter(AData d, double borderPercentage)
        {
            double spreadParameter = -Math.Log(borderPercentage) / 2;
            this.spreadParameterX = spreadParameter / d.XSpacing;
            this.spreadParameterY = spreadParameter / d.YSpacing;
            this.spreadParameterZ = spreadParameter / d.ZSpacing;
        }

        private Vector<double> GetFunctionGradient(Point3D p, Vector<double> coeficients)
        {
            return Vector<double>.Build.DenseOfArray(new double[]
            {
                2*coeficients[0]*p.X + coeficients[3] * p.Y + coeficients[4] * p.Z + coeficients[6],
                2*coeficients[1]*p.Y + coeficients[3] * p.X + coeficients[5] * p.Z + coeficients[7],
                2*coeficients[2]*p.Z + coeficients[4] * p.X + coeficients[5] * p.Y + coeficients[8]
            });
        }

        private double ComputeGradient(Point3D point, AData d, Point3D centerPoint, int radius)
        {
            List<Point3D> surroundingPoints = CalculateSurroundingPoints(point, d, radius);
            Vector<double> coeficients = GetApproximationEquation(surroundingPoints, point, centerPoint, d);
            Vector<double> functionGradient = GetFunctionGradient(point - centerPoint, coeficients);
            return functionGradient.L2Norm();
        }

        private Vector<double> GetApproximationEquation(List<Point3D> surroundingPoints, Point3D referencePoint, Point3D centerPoint, AData d)
        {
            int NUMBER_OF_VARIABLES = 10;

            Matrix<double> qMatrixT = Matrix<double>.Build.Dense(NUMBER_OF_VARIABLES, surroundingPoints.Count);
            Matrix<double> qMatrix = Matrix<double>.Build.Dense(surroundingPoints.Count, NUMBER_OF_VARIABLES);
            Vector<double> values = Vector<double>.Build.Dense(surroundingPoints.Count);
            Vector<double> weightedValues = Vector<double>.Build.Dense(surroundingPoints.Count);
            Matrix<double> rightSide = Matrix<double>.Build.Dense(qMatrix.ColumnCount, 1);

            Point3D centeredPoint = new Point3D(
                referencePoint.X - centerPoint.X,
                referencePoint.Y - centerPoint.Y,
                referencePoint.Z - centerPoint.Z
            );

            for (int i = 0; i < surroundingPoints.Count; i++)
            {
                double weight = GetGaussianWeight(
                    surroundingPoints[i].X - centeredPoint.X,
                    surroundingPoints[i].Y - centeredPoint.Y,
                    surroundingPoints[i].Z - centeredPoint.Z
                );

                qMatrixT[0, i] = Math.Pow(surroundingPoints[i].X, 2);
                qMatrixT[1, i] = Math.Pow(surroundingPoints[i].Y, 2);
                qMatrixT[2, i] = Math.Pow(surroundingPoints[i].Z, 2);
                qMatrixT[3, i] = surroundingPoints[i].X * surroundingPoints[i].Y;
                qMatrixT[4, i] = surroundingPoints[i].X * surroundingPoints[i].Z;
                qMatrixT[5, i] = surroundingPoints[i].Y * surroundingPoints[i].Z;
                qMatrixT[6, i] = surroundingPoints[i].X;
                qMatrixT[7, i] = surroundingPoints[i].Y;
                qMatrixT[8, i] = surroundingPoints[i].Z;
                qMatrixT[9, i] = 1;

                for (int j = 0; j < qMatrixT.RowCount; j++)
                    qMatrix[i, j] = qMatrixT[j, i] * weight;

                values[i] = d.GetValue(surroundingPoints[i] + centerPoint);

                weightedValues[i] = values[i] * weight;
            }

            if (CheckSameValues(values, 1))
                return Vector<double>.Build.Dense(NUMBER_OF_VARIABLES);

            /* Constructing right side */
            for (int i = 0; i < rightSide.RowCount; i++)
                rightSide[i, 0] = qMatrixT.Row(i).DotProduct(weightedValues);

            Matrix<double> left = qMatrixT.Multiply(qMatrix);
            return left.Solve(rightSide).Column(0).Map(x => double.IsNaN(x) ? 0 : x);
        }

        private bool CheckSameValues(Vector<double> values, double threshold)
        {
            double minValue = double.MaxValue, maxValue = double.MinValue;

            for (int i = 0; i < values.Count; i++)
            {
                if (minValue > values[i])
                    minValue = values[i];

                if (maxValue < values[i])
                    maxValue = values[i];
            }

            return Math.Abs(minValue - maxValue) < threshold;
        }

        private double GetGaussianWeight(double x, double y, double z)
        {
            return Math.Exp(-(spreadParameterX * x * x + spreadParameterY * y * y + spreadParameterZ * z * z));
        }

        private List<Point3D> CalculateSurroundingPoints(Point3D point, AData d, int radius)
        {
            List<Point3D> surroundingPoints = new List<Point3D>();

            int minSpacingMulitplierX = MinSpacingMulitplier(point.X, d.XSpacing, radius), maxSpacingMultiplierX = MaxSpacingMultiplier(point.X, d.MaxValueX, d.XSpacing, radius);
            int minSpacingMulitplierY = MinSpacingMulitplier(point.Y, d.YSpacing, radius), maxSpacingMultiplierY = MaxSpacingMultiplier(point.Y, d.MaxValueY, d.YSpacing, radius);
            int minSpacingMulitplierZ = MinSpacingMulitplier(point.Z, d.ZSpacing, radius), maxSpacingMultiplierZ = MaxSpacingMultiplier(point.Z, d.MaxValueZ, d.ZSpacing, radius);

            for (int x = -minSpacingMulitplierX; x <= maxSpacingMultiplierX; x++)
            {
                for (int y = -minSpacingMulitplierY; y <= maxSpacingMultiplierY; y++)
                {
                    for (int z = -minSpacingMulitplierZ; z <= maxSpacingMultiplierZ; z++)
                    {
                        surroundingPoints.Add(new Point3D(x, y, z));
                    }
                }
            }

            return surroundingPoints;
        }

        /// <summary>
        /// This function takes in a value and rounds it to the nearest value that is a multiplier of spacing
        /// </summary>
        /// <param name="value">Value to be rounded</param>
        /// <param name="spacing">Spacing</param>
        /// <returns>Returns rounded value</returns>
        private double RoundToNearestSpacingMultiplier(double value, double spacing)
        {

            int unitDistance = (int)(value / spacing);
            double smallerNeighborDistance = value - (unitDistance * spacing);
            double biggerNeighborDistance = ((unitDistance + 1) * spacing) - value;

            return smallerNeighborDistance < biggerNeighborDistance ? (unitDistance * spacing) : ((unitDistance + 1) * spacing);
        }


        public static int MinSpacingMulitplier(double currentCoordinate, double spacing, int desiredShift)
        {
            return (int)Math.Min(currentCoordinate / spacing, desiredShift);
        }

        public static int MaxSpacingMultiplier(double currentCoordinate, double maxValue, double spacing, int desiredShift)
        {
            return (int)Math.Min((maxValue - currentCoordinate) / spacing, desiredShift);
        }      
    }
}