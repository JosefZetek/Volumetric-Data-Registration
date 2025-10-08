using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DataView
{
    public class FeatureComputerQuantiles : AFeatureComputer
    {

        private UniformSphereSampler uniformSphereSampler;

        public FeatureComputerQuantiles()
        {
            this.uniformSphereSampler = new UniformSphereSampler();
        }

        public FeatureComputerQuantiles(UniformSphereSampler uniformSphereSampler)
        {
            this.uniformSphereSampler = uniformSphereSampler;
        }

        public override int NumberOfFeatures => 5;

        private static List<double> CalculateValues(List<Point3D> points, AData d)
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

        public override void ComputeFeatureVector(AData d, Point3D p, double[] array, int startIndex)
        {
            List<Point3D> points = uniformSphereSampler.GetDistributedPoints(d, p);
            List<double> values = CalculateValues(points, d);

            //Save(outputFilename, "unfiltered", points, values);

            /* Threshold to filter insignificant  values */
            QuickSelectClass quickSelectClass = new QuickSelectClass();
            double value1 = quickSelectClass.QuickSelect(values, 0);
            double value2 = quickSelectClass.QuickSelect(values, (int)(values.Count * 0.25));
            double value3 = quickSelectClass.QuickSelect(values, values.Count / 2);
            double value4 = quickSelectClass.QuickSelect(values, (int)(values.Count * 0.75));
            double value5 = quickSelectClass.QuickSelect(values, values.Count-1);

            array[startIndex] = value1;
            array[startIndex + 1] = value2;
            array[startIndex + 2] = value3;
            array[startIndex + 3] = value4;
            array[startIndex + 4] = value5;
        }
    }
}