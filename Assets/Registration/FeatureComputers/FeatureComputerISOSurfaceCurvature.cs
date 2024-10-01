using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    class FeatureComputerISOSurfaceCurvature : IFeatureComputer
    {
        public FeatureComputerISOSurfaceCurvature()
		{
		}

        private FeatureVector ShapeOperator(IData d, Point3D p)
        {
            Console.WriteLine("Shape operator implementation: ");

            Point3D nearestGridPoint = new Point3D(
                RoundToNearestSpacingMultiplier(p.X, d.XSpacing),
                RoundToNearestSpacingMultiplier(p.Y, d.YSpacing),
                RoundToNearestSpacingMultiplier(p.Z, d.ZSpacing)
            );

            List<Point3D> surroundingPoints = GetSurroundingPoints(nearestGridPoint, d);
            Matrix<double> equationMatrix = ConstructEquationMatrix(surroundingPoints, nearestGridPoint, d);
            Vector<double> coeficients = EquationComputer.CalculateSolution(equationMatrix);

            Matrix<double> hessianMatrix = ConstructHessianMatrix(coeficients);

            Matrix<double> functionGradient = GetFunctionGradient(coeficients, p.X, p.Y, p.Z);
            functionGradient /= functionGradient.L2Norm();

            Matrix<double> identityMatrix = Matrix<double>.Build.DenseIdentity(3);
            Console.WriteLine(identityMatrix);

            Matrix<double> matrix = identityMatrix - functionGradient.Transpose() * functionGradient;

            

            Matrix<double> ShapeOperator = matrix * hessianMatrix * matrix;
            var eigendecomposition = ShapeOperator.Evd();


            double minCurvature = eigendecomposition.EigenValues[0].Real;
            double maxCurvature = eigendecomposition.EigenValues[1].Real;

            Console.WriteLine("Min curvature = {0}", minCurvature);
            Console.WriteLine("Max curvature = {0}", maxCurvature);
            return new FeatureVector(p, new double[] { minCurvature, maxCurvature });

        }

        private FeatureVector OriginalImplementation(IData d, Point3D p)
        {
            Point3D nearestGridPoint = new Point3D(
                RoundToNearestSpacingMultiplier(p.X, d.XSpacing),
                RoundToNearestSpacingMultiplier(p.Y, d.YSpacing),
                RoundToNearestSpacingMultiplier(p.Z, d.ZSpacing)
            );

            List<Point3D> surroundingPoints = GetSurroundingPoints(nearestGridPoint, d);
            Matrix<double> equationMatrix = ConstructEquationMatrix(surroundingPoints, nearestGridPoint, d);
            Vector<double> coeficients = EquationComputer.CalculateSolution(equationMatrix);

            Matrix<double> hessianMatrix = ConstructHessianMatrix(coeficients);
            Matrix<double> adjointHessianMatrix = ConstructAdjointHessianMatrix(hessianMatrix);

            Matrix<double> functionGradient = GetFunctionGradient(coeficients, p.X, p.Y, p.Z);
            double functionGradientNorm = functionGradient.L2Norm();

            double gaussianCurvature = (functionGradient * adjointHessianMatrix * functionGradient.Transpose())[0, 0] / Math.Pow(functionGradientNorm, 4);
            double meanCurvature = ((functionGradient * hessianMatrix * functionGradient.Transpose())[0, 0] - (Math.Pow(functionGradientNorm, 2) * hessianMatrix.Trace())) / (2 * Math.Pow(functionGradientNorm, 3));

            double increment = Math.Sqrt(Math.Pow(meanCurvature, 2) - gaussianCurvature);
            double minCurvature = meanCurvature - increment;
            double maxCurvature = meanCurvature + increment;

            return new FeatureVector(p, new double[] { minCurvature, maxCurvature });
        }

        private FeatureVector AlternativeImplementation(IData d, Point3D p)
        {
            Console.WriteLine("Alternative implementation: ");

            Point3D nearestGridPoint = new Point3D(
                RoundToNearestSpacingMultiplier(p.X, d.XSpacing),
                RoundToNearestSpacingMultiplier(p.Y, d.YSpacing),
                RoundToNearestSpacingMultiplier(p.Z, d.ZSpacing)
            );

            List<Point3D> surroundingPoints = GetSurroundingPoints(nearestGridPoint, d);
            Matrix<double> equationMatrix = ConstructEquationMatrix(surroundingPoints, nearestGridPoint, d);
            Vector<double> coeficients = EquationComputer.CalculateSolution(equationMatrix);

            Matrix<double> hessianMatrix = ConstructHessianMatrix(coeficients);
            Matrix<double> adjointHessianMatrix = ConstructAdjointHessianMatrix(hessianMatrix);

            Matrix<double> functionGradient = GetFunctionGradient(coeficients, p.X, p.Y, p.Z);
            double functionGradientNorm = functionGradient.L2Norm();

            hessianMatrix = -hessianMatrix / functionGradientNorm;


            var eigenvalues = hessianMatrix.Evd();

            double maxCurvature = eigenvalues.EigenValues[0].Real;
            double minCurvature = eigenvalues.EigenValues[1].Real;

            return new FeatureVector(p, new double[] { minCurvature, maxCurvature });
        }

        public FeatureVector ComputeFeatureVector(IData d, Point3D p)
        {
            return OriginalImplementation(d, p);
            //AlternativeImplementation(d, p);

            //return ShapeOperator(d, p);
        }

        private Matrix<double> ConstructAdjointHessianMatrix(Matrix<double> hessianMatrix)
        {

            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {
                    hessianMatrix[1, 1] * hessianMatrix[2, 2] - hessianMatrix[1, 2] * hessianMatrix[2, 1],
                    hessianMatrix[1, 2] * hessianMatrix[2, 0] - hessianMatrix[1, 0] * hessianMatrix[2, 2],
                    hessianMatrix[1, 0] * hessianMatrix[2, 1] - hessianMatrix[1, 1] * hessianMatrix[2, 0]
                },
                {
                    hessianMatrix[0, 2] * hessianMatrix[2, 1] - hessianMatrix[0, 1] * hessianMatrix[2, 2],
                    hessianMatrix[0, 0] * hessianMatrix[2, 2] - hessianMatrix[0, 2] * hessianMatrix[2, 0],
                    hessianMatrix[0, 1] * hessianMatrix[2, 0] - hessianMatrix[0, 0] * hessianMatrix[2, 1]
                },
                {
                    hessianMatrix[0, 1] * hessianMatrix[1, 2] - hessianMatrix[0, 2] * hessianMatrix[1, 1],
                    hessianMatrix[1, 0] * hessianMatrix[0, 2] - hessianMatrix[0, 0] * hessianMatrix[1, 2],
                    hessianMatrix[0, 0] * hessianMatrix[1, 1] - hessianMatrix[0, 1] * hessianMatrix[1, 0]
                }
            });

            /*
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {
                    hessianMatrix[0, 0] * hessianMatrix[2, 2] - hessianMatrix[1, 2] * hessianMatrix[2, 1],
                    hessianMatrix[1, 2] * hessianMatrix[2, 0] - hessianMatrix[1, 0] * hessianMatrix[2, 2],
                    hessianMatrix[1, 0] * hessianMatrix[2, 1] - hessianMatrix[1, 1] * hessianMatrix[2, 0]
                },
                {
                    hessianMatrix[0, 2] * hessianMatrix[2, 1] - hessianMatrix[0, 1] * hessianMatrix[2, 2],
                    hessianMatrix[0, 0] * hessianMatrix[2, 2] - hessianMatrix[0, 2] * hessianMatrix[2, 0],
                    hessianMatrix[0, 1] * hessianMatrix[2, 1] - hessianMatrix[0, 0] * hessianMatrix[2, 1]
                },
                {
                    hessianMatrix[0, 1] * hessianMatrix[1, 2] - hessianMatrix[0, 2] * hessianMatrix[1, 1],
                    hessianMatrix[1, 0] * hessianMatrix[0, 2] - hessianMatrix[0, 0] * hessianMatrix[1, 2],
                    hessianMatrix[0, 0] * hessianMatrix[1, 1] - hessianMatrix[0, 1] * hessianMatrix[1, 0]
                }
            });
            */
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

        private Matrix<double> GetFunctionGradient(Vector<double> coeficients, double x, double y, double z)
        {
            return Matrix<double>.Build.DenseOfArray(new double[,]
            {
                {
                    2*coeficients[0]*x + coeficients[3] * y + coeficients[4] * z + coeficients[6],
                    2*coeficients[1]*y + coeficients[3] * x + coeficients[5] * z + coeficients[7],
                    2*coeficients[2]*z + coeficients[4] * x + coeficients[5] * y + coeficients[8]
                }
            });
        }

        private double GetValue(Vector<double> coeficients, double x, double y, double z)
        {
            //a* x^2 + b * y ^ 2 + c * z ^ 2 + d * x * y + e * x * z + f * y * z + g * x + h * y + i * z = f(x, y, z)
            return coeficients[0] * Math.Pow(x, 2) + coeficients[1] * Math.Pow(y, 2) + coeficients[2] * Math.Pow(z, 2) + coeficients[3] * x * y + coeficients[4] * x * z + coeficients[5] * y * z + coeficients[6] * x + coeficients[7] * y + coeficients[8] * z;
        }


        /// <summary>
        /// Constructs Gram-Schmidt matrix for approximation using least squares
        /// Equation format a * x^2 + b * y^2 + c * z^2 + d * x*y + e * x*z + f * y*z + g*x + h*y + i*z = f(x,y,z)
        /// </summary>
        /// <param name="surroundingPoints">List of points from the surrounding of a given value</param>
        /// <param name="d">Data class</param>
        /// <returns>Returns equation matrix for least squares method</returns>
        private Matrix<double> ConstructEquationMatrix(List<Point3D> surroundingPoints, Point3D centerPoint, IData d)
        {
            //this gets created here to improve efficiency - 9 values corresponding to function format, 10th is the value at given point
            double[] row = new double[10];

            Matrix<double> equationMatrix = Matrix<double>.Build.Dense(9, 10);

            for (int i = 0; i < surroundingPoints.Count; i++)
            {

                try
                {
                    row[0] = Math.Pow(surroundingPoints[i].X, 2);
                    row[1] = Math.Pow(surroundingPoints[i].Y, 2);
                    row[2] = Math.Pow(surroundingPoints[i].Z, 2);
                    row[3] = surroundingPoints[i].X * surroundingPoints[i].Y;
                    row[4] = surroundingPoints[i].X * surroundingPoints[i].Z;
                    row[5] = surroundingPoints[i].Y * surroundingPoints[i].Z;
                    row[6] = surroundingPoints[i].X;
                    row[7] = surroundingPoints[i].Y;
                    row[8] = surroundingPoints[i].Z;
                    row[9] = d.GetValue(surroundingPoints[i].X, surroundingPoints[i].Y, surroundingPoints[i].Z);

                    AddIncrement(row, equationMatrix);
                }
                catch { continue; }
                
            }

            return equationMatrix;
        }

        private Matrix<double> ConstructEquationMatrixAlternative(List<Point3D> surroundingPoints, IData d)
        {
            List<Vector<double>> leftSide = new List<Vector<double>>();
            List<Vector<double>> rightSide = new List<Vector<double>>();

            foreach (Point3D surroundingPoint in surroundingPoints)
            {
                try
                {
                    d.GetValue(surroundingPoint);
                }
                catch { continue; }

                Vector<double> leftSideVector = Vector<double>.Build.Dense(9);
                leftSideVector[0] = Math.Pow(surroundingPoint.X, 2);
                leftSideVector[1] = Math.Pow(surroundingPoint.Y, 2);
                leftSideVector[2] = Math.Pow(surroundingPoint.Z, 2);
                leftSideVector[3] = surroundingPoint.X * surroundingPoint.Y;
                leftSideVector[4] = surroundingPoint.X * surroundingPoint.Z;
                leftSideVector[5] = surroundingPoint.Y * surroundingPoint.Z;
                leftSideVector[6] = surroundingPoint.X;
                leftSideVector[7] = surroundingPoint.Y;
                leftSideVector[8] = surroundingPoint.Z;

                Vector<double> rightSideVector = Vector<double>.Build.Dense(1);
                rightSideVector[0] = d.GetValue(surroundingPoint.X, surroundingPoint.Y, surroundingPoint.Z);

                leftSide.Add(leftSideVector);
                rightSide.Add(rightSideVector);
            }

            Matrix<double> leftSideMatrix = Matrix<double>.Build.Dense(leftSide.Count, 9);
            Matrix<double> rightSideMatrix = Matrix<double>.Build.Dense(rightSide.Count, 1);

            for(int i = 0; i< leftSide.Count; i++)
            {
                leftSideMatrix.SetRow(i, leftSide[i]);
                rightSideMatrix.SetRow(i, rightSide[i]);
            }

            Matrix<double> leftSideFinal = leftSideMatrix.Transpose() * leftSideMatrix;
            Matrix<double> rightSideFinal = leftSideMatrix.Transpose() * rightSideMatrix;

            Matrix<double> result = Matrix<double>.Build.Dense(leftSideFinal.RowCount, leftSideFinal.ColumnCount + rightSideFinal.ColumnCount);
            for(int i = 0; i<leftSideFinal.ColumnCount; i++)
            {
                result.SetColumn(i, leftSideFinal.Column(i));
            }

            result.SetColumn(leftSideFinal.ColumnCount, rightSideFinal.Column(0));

            return result;
        }

        /// <summary>
        /// Adds increments to Gram-Schmidt matrix
        /// </summary>
        /// <param name="row"></param>
        /// <param name="equationMatrix"></param>
        private void AddIncrement(double[] row, Matrix<double> equationMatrix)
        {
            for(int i = 0; i < row.Length - 1; i++) //1 index extra with value at given point
            {
                for(int j = 0; j < row.Length; j++)
                    equationMatrix[i, j] += (row[i] * row[j]);
            }
        }

        private List<Point3D> GetSurroundingPoints(Point3D nearestGridPoint, IData d)
        {
            List<Point3D> surroundingPoints = new List<Point3D>();

            for (double x = nearestGridPoint.X - d.XSpacing; x <= (nearestGridPoint.X + d.XSpacing); x += d.XSpacing)
            {
                for (double y = nearestGridPoint.Y - d.YSpacing; y <= (nearestGridPoint.Y + d.YSpacing); y += d.YSpacing)
                {
                    for (double z = nearestGridPoint.Z - d.ZSpacing; z <= (nearestGridPoint.Z + d.ZSpacing); z += d.ZSpacing)
                    {
                        //if its the nearest point, skip it
                        if (x == nearestGridPoint.X && y == nearestGridPoint.Y && z == nearestGridPoint.Z)
                            continue;

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

            int divider = (int)(value / spacing);
            double smallerNeighborDistance = value - (divider * spacing);
            double biggerNeighborDistance = ((divider + 1) * spacing) - value;

            return smallerNeighborDistance < biggerNeighborDistance ? (divider * spacing) : ((divider + 1) * spacing);
        }
    }
}

