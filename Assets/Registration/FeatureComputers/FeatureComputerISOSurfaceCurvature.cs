using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class FeatureComputerISOCurvature : IFeatureComputer
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

            //CalculateSpreadParameter(d, 0.2);
            //Curvature closerNeighborhood = ComputeCurvature(p, d, nearestGridPoint, 1);

            //CalculateSpreadParameter(d, 0.3);
            //Curvature furtherNeighborhood = ComputeCurvature(p, d, nearestGridPoint, 2);

            CalculateSpreadParameter(d, 0.8);
            Curvature furthestNeighborhood = ComputeCurvature(p, d, nearestGridPoint, 5);

            //CalculateSpreadParameter(d, 0.5);

            return new FeatureVector(p, new double[] { furthestNeighborhood.GaussianCurvature, furthestNeighborhood.MeanCurvature, d.GetValue(p) });


            //double[] filteredCurvatures = FilterCurvatures(closerNeighborhood, furtherNeighborhood, furthestNeighborhood);

            //return new FeatureVector(p, new double[] { closerNeighborhood.GaussianCurvature, closerNeighborhood.MeanCurvature, furtherNeighborhood.GaussianCurvature, furtherNeighborhood.MeanCurvature, furthestNeighborhood.GaussianCurvature, furthestNeighborhood.GaussianCurvature});
            //return new FeatureVector(p, new double[] { furthestNeighborhood.GaussianCurvature, furthestNeighborhood.MeanCurvature });
        }

        private double[] FilterCurvatures(Curvature closerNeighborhood, Curvature furtherNeighborhood, Curvature furthestNeighborhood)
        {
            //double[] gaussianCurvatures = new double[] { closerNeighborhood.GaussianCurvature, furtherNeighborhood.GaussianCurvature, furthestNeighborhood.GaussianCurvature };
            //double[] meanCurvatures = new double[] { closerNeighborhood.MeanCurvature, furtherNeighborhood.MeanCurvature, furthestNeighborhood.MeanCurvature };

            //Array.Sort(gaussianCurvatures);
            //Array.Sort(meanCurvatures);

            double filteredGaussian = FindMedian(closerNeighborhood.GaussianCurvature, furtherNeighborhood.GaussianCurvature, furthestNeighborhood.GaussianCurvature);
            double filteredMean = FindMedian(closerNeighborhood.MeanCurvature, furtherNeighborhood.MeanCurvature, furthestNeighborhood.MeanCurvature);

            return new double[] { filteredGaussian, filteredMean };
        }

        private double FindMedian(double a, double b, double c)
        {
            if ((b <= a && a <= c) || (c <= a && a <= b))
                return a;

            if ((a <= b && b <= c) || (c <= b && b <= a))
                return b;

            return c;
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
                    hessianMatrix[1, 2] * hessianMatrix[2, 0] - hessianMatrix[0, 1] * hessianMatrix[2, 2],
                    hessianMatrix[1, 0] * hessianMatrix[2, 1] - hessianMatrix[1, 1] * hessianMatrix[2, 0],
                },
                {
                    hessianMatrix[0, 2] * hessianMatrix[2, 1] - hessianMatrix[0, 1] * hessianMatrix[2, 2],
                    hessianMatrix[0, 0] * hessianMatrix[2, 2] - hessianMatrix[0, 2] * hessianMatrix[2, 0],
                    hessianMatrix[0, 1] * hessianMatrix[2, 0] - hessianMatrix[0, 0] * hessianMatrix[2, 1],

                },
                {
                    hessianMatrix[0, 1] * hessianMatrix[1, 2] - hessianMatrix[0, 2] * hessianMatrix[1, 1],
                    hessianMatrix[1, 0] * hessianMatrix[0, 2] - hessianMatrix[0, 0] * hessianMatrix[1, 2],
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
            List<Point3D> surroundingPoints = CalculateSurroundingPoints(point, d, radius);
            Vector<double> coeficients = GetApproximationEquation(surroundingPoints, point, centerPoint, d);

            Matrix<double> hessianMatrix = ConstructHessianMatrix(coeficients);
            Vector<double> functionGradient = GetFunctionGradient(point - centerPoint, coeficients);
            Matrix<double> adjointHessian = ConstructAdjointMatrix(hessianMatrix);

            double functionGradientNorm = functionGradient.L2Norm();

            if (functionGradientNorm == 0)
                return new Curvature(
                    double.PositiveInfinity,
                    double.PositiveInfinity
                );

            double gaussianCurvature = adjointHessian.LeftMultiply(functionGradient).DotProduct(functionGradient) / Math.Pow(functionGradientNorm, 4); /* Gaussian curvature */
            double meanCurvature = (hessianMatrix.LeftMultiply(functionGradient).DotProduct(functionGradient) - Math.Pow(functionGradientNorm, 2) * hessianMatrix.Trace()) / (2 * Math.Pow(functionGradientNorm, 3)); /* Mean curvature */

            //meanCurvature = Math.Abs(meanCurvature);
            //gaussianCurvature = Math.Abs(gaussianCurvature);

            //return new Curvature(
            //    meanCurvature,
            //    gaussianCurvature
            //);

            double sqDiff = Math.Pow(meanCurvature, 2) - gaussianCurvature;
            sqDiff = Math.Max(sqDiff, 0);
            sqDiff = Math.Sqrt(sqDiff);

            return new Curvature(
                meanCurvature + sqDiff,
                meanCurvature - sqDiff
            );
        }

        //private Curvature ComputeCurvatureAlternative(Point3D point, AData d, Point3D centerPoint, int radius)
        //{
        //    double isoValue = d.GetValue(point);

        //    List<Point3D> surroundingPoints = GetIntersectionPoints(centerPoint, d, radius, isoValue);
        //    //List<Point3D> intersectionPoints = GetIntersectionPointsAlternative(centerPoint, d, radius, isoValue);

        //    Vector<double> coeficients = GetApproximationEquation(surroundingPoints, point, centerPoint, d);

        //    double fx = 2 * coeficients[0] * point.X + coeficients[2] * point.Y + coeficients[3];
        //    double fy 

        //    double sqDiff = Math.Max(Math.Pow(meanCurvature, 2) - gaussianCurvature, 0);
        //    sqDiff = Math.Sqrt(sqDiff);

        //    return new Curvature(
        //        meanCurvature + sqDiff,
        //        meanCurvature - sqDiff
        //    );
        //}

        private double GetValue(Vector<double> coeficients, double x, double y, double z)
        {
            //a* x^2 + b * y ^ 2 + c * z ^ 2 + d * x * y + e * x * z + f * y * z + g * x + h * y + i * z = f(x, y, z)
            return coeficients[0] * Math.Pow(x, 2) + coeficients[1] * Math.Pow(y, 2) + coeficients[2] * Math.Pow(z, 2) + coeficients[3] * x * y + coeficients[4] * x * z + coeficients[5] * y * z + coeficients[6] * x + coeficients[7] * y + coeficients[8] * z + coeficients[9];
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

        private List<Point3D> IntersectionPointsAlternative(Point3D nearestGridPoint, AData d, int radius, double isoValue)
        {
            List<Point3D> intersectionPoints = new List<Point3D>();

            return intersectionPoints;
        }

        private List<Point3D> GetIntersectionPoints(Point3D nearestGridPoint, AData d, int radius, double isoValue)
        {
            List<Point3D> intersectionPoints = new List<Point3D>();
            //HashSet<ProcessedEdge> processedEdges = new HashSet<ProcessedEdge>();

            //List<Point3D> surroundingPoints;
            //ProcessedEdge processedEdge;

            //double valA, valB, t;

            //int minSpacingMulitplierX = MinSpacingMulitplier(nearestGridPoint.X, d.XSpacing, radius), maxSpacingMultiplierX = MaxSpacingMultiplier(nearestGridPoint.X, d.MaxValueX, d.XSpacing, radius);
            //int minSpacingMulitplierY = MinSpacingMulitplier(nearestGridPoint.Y, d.YSpacing, radius), maxSpacingMultiplierY = MaxSpacingMultiplier(nearestGridPoint.Y, d.MaxValueY, d.YSpacing, radius);
            //int minSpacingMulitplierZ = MinSpacingMulitplier(nearestGridPoint.Z, d.ZSpacing, radius), maxSpacingMultiplierZ = MaxSpacingMultiplier(nearestGridPoint.Z, d.MaxValueZ, d.ZSpacing, radius);

            //for (int x = -minSpacingMulitplierX; x <= maxSpacingMultiplierX; x++)
            //{
            //    for (int y = -minSpacingMulitplierY; y <= maxSpacingMultiplierY; y++)
            //    {
            //        for (int z = -minSpacingMulitplierZ; z <= maxSpacingMultiplierZ; z++)
            //        {
            //            Point3D currentPoint = new Point3D(
            //                nearestGridPoint.X + x * d.XSpacing,
            //                nearestGridPoint.Y + y * d.YSpacing,
            //                nearestGridPoint.Z + z * d.ZSpacing
            //            );

            //            valA = d.GetValue(currentPoint);
            //            surroundingPoints = CalculateSurroundingPoints(currentPoint, d);

            //            foreach (Point3D shift in surroundingPoints)
            //            {
            //                processedEdge = new ProcessedEdge(x, y, z, (int)shift.X, (int)shift.Y, (int)shift.Z);

            //                if (processedEdges.Contains(processedEdge))
            //                    continue;

            //                Point3D anotherPoint = new Point3D(
            //                    currentPoint.X + shift.X * d.XSpacing,
            //                    currentPoint.Y + shift.Y * d.YSpacing,
            //                    currentPoint.Z + shift.Z * d.ZSpacing
            //                );

            //                valB = d.GetValue(anotherPoint);

            //                if ((valA < isoValue && valB >= isoValue) || (valB < isoValue && valA >= isoValue))
            //                {
            //                    t = (isoValue - valA) / (valB - valA);
            //                    intersectionPoints.Add(Lerp(currentPoint, anotherPoint, t));
            //                }

            //            }
            //        }
            //    }
            //}

            return intersectionPoints;
        }

        private Point3D Lerp(Point3D a, Point3D b, double t)
        {
            return new Point3D(
                a.X + (b.X - a.X) * t,
                a.Y + (b.Y - a.Y) * t,
                a.Z + (b.Z - a.Z) * t
            );
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

        private class ProcessedEdge
        {
            private int firstX;
            private int firstY;
            private int firstZ;

            private int secondX;
            private int secondY;
            private int secondZ;

            public ProcessedEdge(int firstX, int firstY, int firstZ, int secondX, int secondY, int secondZ)
            {
                this.firstX = firstX;
                this.firstY = firstY;
                this.firstZ = firstZ;

                this.secondX = secondX;
                this.secondY = secondY;
                this.secondZ = secondZ;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is ProcessedEdge))
                    return false;

                ProcessedEdge secondInstance = (ProcessedEdge)obj;

                if (firstX != secondInstance.firstX && firstX != secondInstance.secondX)
                    return false;

                if (firstY != secondInstance.firstY && firstY != secondInstance.secondY)
                    return false;

                if (firstZ != secondInstance.firstZ && firstZ != secondInstance.secondZ)
                    return false;

                return true;
            }

            public override int GetHashCode()
            {
                return firstX + firstY + firstZ + secondX + secondY + secondZ;
            }
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