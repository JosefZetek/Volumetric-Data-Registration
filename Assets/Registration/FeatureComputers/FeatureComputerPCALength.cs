using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class FeatureComputerPCALength : AFeatureComputer
    {
        private UniformSphereSampler sphereSampler;

        public FeatureComputerPCALength()
        {
            this.sphereSampler = new UniformSphereSampler();
        }

        public FeatureComputerPCALength(UniformSphereSampler sphereSampler)
        {
            this.sphereSampler = sphereSampler;
        }

        public override int NumberOfFeatures => 3;

        public override void ComputeFeatureVector(AData d, Point3D p, double[] array, int startIndex)
        {
            List<Point3D> points = sphereSampler.GetDistributedPoints(d, p);
            List<double> values = CalculateValues(points, d);

            /* Threshold to filter insignificant  values */
            QuickSelectClass quickSelectClass = new QuickSelectClass();
            double threshold = quickSelectClass.QuickSelect(values, values.Count / 2);
            FilterPoints(ref points, ref values, threshold);

            Vector<double> meanVector = CalculateWeightedMeanVector(points);
            Matrix<double> covarianceMatrix = CalculateCovarianceMatrix(points, meanVector);

            Vector<double> eigenValues = covarianceMatrix.Evd().EigenValues.Real();
            eigenValues /= eigenValues.L2Norm();

            array[startIndex] = Math.Abs(eigenValues[0]);
            array[startIndex + 1] = Math.Abs(eigenValues[1]);
            array[startIndex + 2] = Math.Abs(eigenValues[2]);

            Array.Sort(array, startIndex, NumberOfFeatures);
        }

        private List<double> CalculateValues(List<Point3D> points, AData d)
        {
            List<double> values = new List<double>();
            double min = double.MaxValue, max = double.MinValue;

            for (int i = 0; i < points.Count; i++)
            {
                values.Add(d.GetValue(points[i]));
                min = Math.Min(min, values[values.Count - 1]);
                max = Math.Max(max, values[values.Count - 1]);
            }

            if (Math.Abs(min - max) < Double.Epsilon)
                throw new ArgumentException("Basis cannot be calculated because all sampled values in the point surrounding are the same.");

            return values;
        }

        private void FilterPoints(ref List<Point3D> points, ref List<double> values, double threshold)
        {

            List<Point3D> filteredPoints = new List<Point3D>();
            List<double> filteredValues = new List<double>();

            for (int i = 0; i < points.Count; i++)
            {
                if (values[i] >= threshold)
                {
                    filteredValues.Add(values[i]);
                    filteredPoints.Add(points[i]);
                }
            }

            points = filteredPoints;
            values = filteredValues;
        }

        private Matrix<double> CalculateCovarianceMatrix(List<Point3D> pointsInSphere, Vector<double> meanVector)
        {
            int N = pointsInSphere.Count;
            Matrix<double> A = Matrix<double>.Build.Dense(N, 3); // Each row is a point (N x 3)

            for (int i = 0; i < N; i++)
            {
                A.SetRow(i, new double[] {
                    pointsInSphere[i].X - meanVector[0],
                    pointsInSphere[i].Y - meanVector[1],
                    pointsInSphere[i].Z - meanVector[2]
                });
            }

            return (A.Transpose() * A) / (N - 1); // (3xN) * (Nx3) = 3x3 covariance matrix
        }

        private Vector<double> CalculateWeightedMeanVector(List<Point3D> pointsInSphere)
        {
            Vector<double> meanVector = Vector<double>.Build.Dense(3);

            for (int i = 0; i < pointsInSphere.Count; i++)
            {
                meanVector[0] += pointsInSphere[i].X;
                meanVector[1] += pointsInSphere[i].Y;
                meanVector[2] += pointsInSphere[i].Z;
            }

            meanVector /= pointsInSphere.Count;
            return meanVector;
        }
    }
}