using System;
using System.Collections.Generic;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class FeatureComputerISOSurfaceCurvature : IFeatureComputer
    {
        private double spreadParameterX;
        private double spreadParameterY;
        private double spreadParameterZ;

        public FeatureVector ComputeFeatureVector(AData d, Point3D p)
        {
            Point3D nearestGridPoint = new Point3D(
                RoundToNearestSpacingMultiplier(p.X, d.XSpacing),
                RoundToNearestSpacingMultiplier(p.Y, d.YSpacing),
                RoundToNearestSpacingMultiplier(p.Z, d.ZSpacing)
            );

            UnityEngine.Debug.Log("---------------------");
            UnityEngine.Debug.Log($"Point X = [{p.X}, {p.Y}, {p.Z}]");

            CalculateSpreadParameter(d, 0.1);
            Curvature closerNeighborhood = ComputeCurvature(p, d, nearestGridPoint, 1);

            CalculateSpreadParameter(d, 0.2);
            Curvature furtherNeighborhood = ComputeCurvature(p, d, nearestGridPoint, 2);

            UnityEngine.Debug.Log($"Curvature closer: {closerNeighborhood.GaussianCurvature}, {closerNeighborhood.MeanCurvature}");
            UnityEngine.Debug.Log($"Curvature further: {furtherNeighborhood.GaussianCurvature}, {furtherNeighborhood.MeanCurvature}");
            UnityEngine.Debug.Log("---------------------");

            return new FeatureVector(p, new double[] { closerNeighborhood.GaussianCurvature, closerNeighborhood.MeanCurvature, furtherNeighborhood.GaussianCurvature, furtherNeighborhood.MeanCurvature });
        }

        private void CalculateSpreadParameter(AData d, double borderPercentage)
        {
            double spreadParameter = -Math.Log(borderPercentage) / 2;
            this.spreadParameterX = spreadParameter / d.XSpacing;
            this.spreadParameterY = spreadParameter / d.YSpacing;
            this.spreadParameterZ = spreadParameter / d.ZSpacing;
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

            Vector<double> coeficients = CalculateCoeficients(surroundingPoints, point, d);

            Vector<double> functionGradient = GetFunctionGradient(point, coeficients);

            UnityEngine.Debug.Log($"Coef: {coeficients}");
            Matrix<double> hessianMatrix = ConstructHessianMatrix(coeficients);
            UnityEngine.Debug.Log($"Hessian: {hessianMatrix}");


            /* If the frobenius norm is too small */
            if (hessianMatrix.FrobeniusNorm() < 1e-5)
                return new Curvature(0, 0);

            Matrix<double> adjointHessian = ConstructAdjointMatrix(hessianMatrix);

            

            double functionGradientNorm = functionGradient.L2Norm();

            double gaussianCurvature = adjointHessian.LeftMultiply(functionGradient).DotProduct(functionGradient) / Math.Pow(functionGradientNorm, 4); /* Gaussian curvature */
            double meanCurvature = (hessianMatrix.LeftMultiply(functionGradient).DotProduct(functionGradient) - Math.Pow(functionGradientNorm, 2) * hessianMatrix.Trace()) / (2 * Math.Pow(functionGradientNorm, 3)); /* Mean curvature */

            double sqDiff = Math.Sqrt(Math.Pow(meanCurvature, 2) - gaussianCurvature);

            return new Curvature(
                meanCurvature + sqDiff,
                meanCurvature-sqDiff
                
            );
        }

        private double GetValue(Vector<double> coeficients, double x, double y, double z)
        {
            //a* x^2 + b * y ^ 2 + c * z ^ 2 + d * x * y + e * x * z + f * y * z + g * x + h * y + i * z = f(x, y, z)
            return coeficients[0] * Math.Pow(x, 2) + coeficients[1] * Math.Pow(y, 2) + coeficients[2] * Math.Pow(z, 2) + coeficients[3] * x * y + coeficients[4] * x * z + coeficients[5] * y * z + coeficients[6] * x + coeficients[7] * y + coeficients[8] * z + coeficients[9];
        }

        private Vector<double> CalculateCoeficients(List<Point3D> surroundingPoints, Point3D referencePoint, AData d)
        {
            int NUMBER_OF_VARIABLES = 10;

            Matrix<double> qMatrixT = Matrix<double>.Build.Dense(NUMBER_OF_VARIABLES, surroundingPoints.Count);
            Matrix<double> qMatrix = Matrix<double>.Build.Dense(surroundingPoints.Count, NUMBER_OF_VARIABLES);
            Vector<double> values = Vector<double>.Build.Dense(surroundingPoints.Count);
            Matrix<double> rightSide = Matrix<double>.Build.Dense(qMatrix.ColumnCount, 1);

            for (int i = 0; i < surroundingPoints.Count; i++)
            {
                double weight = GetGaussianWeight(
                    surroundingPoints[i].X - referencePoint.X,
                    surroundingPoints[i].Y - referencePoint.Y,
                    surroundingPoints[i].Z - referencePoint.Z
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

                //values[i] = d.GetValue(surroundingPoints[i]);
                values[i] = d.GetValue(surroundingPoints[i]) * weight;
            }

            /* Constructing right side */
            for (int i = 0; i < rightSide.RowCount; i++)
                rightSide[i, 0] = qMatrixT.Row(i).DotProduct(values);

            return (qMatrixT * qMatrix).Solve(rightSide).Column(0);
        }

        private double GetGaussianWeight(double x, double y, double z)
        {
            return Math.Exp(-(spreadParameterX * x * x + spreadParameterY * y * y + spreadParameterZ * z * z));
        }

        private List<Point3D> GetSurroundingPoints(Point3D nearestGridPoint, AData d, int radius)
        {
            List<Point3D> surroundingPoints = new List<Point3D>();

            int minSpacingMulitplierX = MinSpacingMulitplier(nearestGridPoint.X, d.XSpacing, radius), maxSpacingMultiplierX = MaxSpacingMultiplier(nearestGridPoint.X, d.MaxValueX, d.XSpacing, radius);
            int minSpacingMulitplierY = MinSpacingMulitplier(nearestGridPoint.Y, d.YSpacing, radius), maxSpacingMultiplierY = MaxSpacingMultiplier(nearestGridPoint.Y, d.MaxValueY, d.YSpacing, radius);
            int minSpacingMulitplierZ = MinSpacingMulitplier(nearestGridPoint.Z, d.ZSpacing, radius), maxSpacingMultiplierZ = MaxSpacingMultiplier(nearestGridPoint.Z, d.MaxValueZ, d.ZSpacing, radius);

            for (int x = -minSpacingMulitplierX; x <= maxSpacingMultiplierX; x++)
            {
                for (int y = -minSpacingMulitplierY; y <= maxSpacingMultiplierY; y++)
                {
                    for (int z = -minSpacingMulitplierZ; z <= maxSpacingMultiplierZ; z++)
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