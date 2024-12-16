using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class FeatureComputerISOSurfaceCurvature : IFeatureComputer
    {
        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {

            Point3D nearestGridPoint = new Point3D(
                RoundToNearestSpacingMultiplier(p.X, d.XSpacing),
                RoundToNearestSpacingMultiplier(p.Y, d.YSpacing),
                RoundToNearestSpacingMultiplier(p.Z, d.ZSpacing)
            );

            Curvature closerNeighborhood = ComputeCurvature(p, d, nearestGridPoint, 1);
            Curvature furtherNeighborhood = ComputeCurvature(p, d, nearestGridPoint, 2);

            return new FeatureVector(p, new double[] { closerNeighborhood.GaussianCurvature, closerNeighborhood.MeanCurvature, furtherNeighborhood.GaussianCurvature, furtherNeighborhood.MeanCurvature });
        }

        private Matrix<double> ConstructAdjointMatrix(Matrix<double> hessianMatrix)
        {
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {
                    hessianMatrix[1, 1] * hessianMatrix[2, 2] - hessianMatrix[1, 2] * hessianMatrix[2, 1],
                    hessianMatrix[0, 2] * hessianMatrix[2, 1] - hessianMatrix[0, 1] * hessianMatrix[2, 2],
                    hessianMatrix[0, 1] * hessianMatrix[1, 2] - hessianMatrix[0, 2] * hessianMatrix[1, 1],
                },
                {
                    hessianMatrix[1, 2] * hessianMatrix[2, 0] - hessianMatrix[1, 0] * hessianMatrix[2, 2],
                    hessianMatrix[0, 0] * hessianMatrix[2, 2] - hessianMatrix[0, 2] * hessianMatrix[2, 0],
                    hessianMatrix[0, 2] * hessianMatrix[1, 0] - hessianMatrix[0, 0] * hessianMatrix[1, 2],

                },
                {
                    hessianMatrix[1, 0] * hessianMatrix[2, 1] - hessianMatrix[1, 1] * hessianMatrix[2, 0],
                    hessianMatrix[0, 1] * hessianMatrix[2, 0] - hessianMatrix[0, 0] * hessianMatrix[2, 1],
                    hessianMatrix[0, 0] * hessianMatrix[1, 1] - hessianMatrix[0, 1] * hessianMatrix[1, 0]
                }
            });
        }

        private Matrix<double> ConstructHessianMatrix(Vector<double> coeficients)
        {
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                { 2*coeficients[0], coeficients[3], coeficients[4] },
                { coeficients[3], 2*coeficients[1], coeficients[5] },
                { coeficients[4], coeficients[5], 2*coeficients[2] },
            });
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

        private Curvature ComputeCurvature(Point3D point, AData d, Point3D centerPoint, int radius)
        {
            List<Point3D> surroundingPoints = GetSurroundingPoints(centerPoint, d, radius);

            Vector<double> coeficients = CalculateCoeficients(surroundingPoints, d);
            Matrix<double> hessianMatrix = ConstructHessianMatrix(coeficients);
            Matrix<double> adjointHessian = ConstructAdjointMatrix(hessianMatrix);

            Vector<double> functionGradient = GetFunctionGradient(point, coeficients);
            double functionGradientNorm = functionGradient.L2Norm();

            return new Curvature(
                adjointHessian.LeftMultiply(functionGradient).DotProduct(functionGradient) / Math.Pow(functionGradientNorm, 4), /* Gaussian curvature */
                (hessianMatrix.LeftMultiply(functionGradient).DotProduct(functionGradient) - Math.Pow(functionGradientNorm, 2) * hessianMatrix.Trace()) / 2 * Math.Pow(functionGradientNorm, 3) /* Mean curvature */
            );
        }

        private double GetValue(Vector<double> coeficients, double x, double y, double z)
        {
            //a* x^2 + b * y ^ 2 + c * z ^ 2 + d * x * y + e * x * z + f * y * z + g * x + h * y + i * z = f(x, y, z)
            return coeficients[0] * Math.Pow(x, 2) + coeficients[1] * Math.Pow(y, 2) + coeficients[2] * Math.Pow(z, 2) + coeficients[3] * x * y + coeficients[4] * x * z + coeficients[5] * y * z + coeficients[6] * x + coeficients[7] * y + coeficients[8] * z + coeficients[9];
        }

        public void Compare(Point3D p, AData d)
        {
            Point3D nearestGridPoint = new Point3D(
                RoundToNearestSpacingMultiplier(p.X, d.XSpacing),
                RoundToNearestSpacingMultiplier(p.Y, d.YSpacing),
                RoundToNearestSpacingMultiplier(p.Z, d.ZSpacing)
            );

            List<Point3D> surroundingPoints = GetSurroundingPoints(nearestGridPoint, d, 1);

            Vector<double> coeficients = CalculateCoeficients(surroundingPoints, d);

            for (double x = -1; x <= 1; x++)
            {
                for (double y = -1; y <= 1; y++)
                {
                    for (double z = -1; z <= 1; z++)
                    {
                        Point3D current = new Point3D(p.X + x, p.Y + y, p.Z + z);

                        Console.WriteLine($"Difference: {d.GetValue(current) - GetValue(coeficients, current.X, current.Y, current.Z)}");
                    }
                }
            }
        }



        private Vector<double> CalculateCoeficients(List<Point3D> surroundingPoints, AData d)
        {
            int NUMBER_OF_VARIABLES = 10;

            Matrix<double> qMatrix = Matrix<double>.Build.Dense(surroundingPoints.Count, NUMBER_OF_VARIABLES);
            Vector<double> values = Vector<double>.Build.Dense(surroundingPoints.Count);
            Matrix<double> rightSide = Matrix<double>.Build.Dense(qMatrix.ColumnCount, 1);

            for (int i = 0; i < surroundingPoints.Count; i++)
            {
                qMatrix[i, 0] = Math.Pow(surroundingPoints[i].X, 2);
                qMatrix[i, 1] = Math.Pow(surroundingPoints[i].Y, 2);
                qMatrix[i, 2] = Math.Pow(surroundingPoints[i].Z, 2);
                qMatrix[i, 3] = surroundingPoints[i].X * surroundingPoints[i].Y;
                qMatrix[i, 4] = surroundingPoints[i].X * surroundingPoints[i].Z;
                qMatrix[i, 5] = surroundingPoints[i].Y * surroundingPoints[i].Z;
                qMatrix[i, 6] = surroundingPoints[i].X;
                qMatrix[i, 7] = surroundingPoints[i].Y;
                qMatrix[i, 8] = surroundingPoints[i].Z;
                qMatrix[i, 9] = 1;
            }

            /* Values at corresponding points of surroundingPoints */
            for (int i = 0; i < values.Count; i++)
                values[i] = d.GetValue(surroundingPoints[i]);

            /* Constructing right side */
            for (int i = 0; i < rightSide.RowCount; i++)
                rightSide[i, 0] = qMatrix.Column(i).DotProduct(values);


            return (qMatrix.Transpose() * qMatrix).Solve(rightSide).Column(0);
        }

        private List<Point3D> GetSurroundingPoints(Point3D nearestGridPoint, AData d, int radius)
        {
            List<Point3D> surroundingPoints = new List<Point3D>();

            int minSpacingMulitplierX = MinSpacingMulitplier(nearestGridPoint.X, d.XSpacing, radius), maxSpacingMultiplierX = MaxSpacingMultiplier(nearestGridPoint.X, d.MaxValueX, d.XSpacing, radius);
            int minSpacingMulitplierY = MinSpacingMulitplier(nearestGridPoint.Y, d.YSpacing, radius), maxSpacingMultiplierY = MaxSpacingMultiplier(nearestGridPoint.Y, d.MaxValueY, d.YSpacing, radius);
            int minSpacingMulitplierZ = MinSpacingMulitplier(nearestGridPoint.Z, d.ZSpacing, radius), maxSpacingMultiplierZ = MaxSpacingMultiplier(nearestGridPoint.Z, d.MaxValueZ, d.ZSpacing, radius);

            for (int x = -minSpacingMulitplierX; x <= maxSpacingMultiplierX; x++)
            {
                for (double y = -minSpacingMulitplierY; y <= maxSpacingMultiplierY; y++)
                {
                    for (double z = -minSpacingMulitplierZ; z <= maxSpacingMultiplierZ; z++)
                    {
                        surroundingPoints.Add(
                            new Point3D(
                                nearestGridPoint.X + x * d.XSpacing,
                                nearestGridPoint.Y + y * d.YSpacing,
                                nearestGridPoint.Z + z * d.ZSpacing
                            )
                        );
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

        private class Curvature
        {
            private double gaussianCurvature;
            private double meanCurvature;

            public Curvature(double gaussianCurvature, double meanCurvature)
            {
                this.gaussianCurvature = gaussianCurvature;
                this.meanCurvature = meanCurvature;
            }

            public double GaussianCurvature { get => gaussianCurvature; }
            public double MeanCurvature { get => meanCurvature; }

        }
    }
}