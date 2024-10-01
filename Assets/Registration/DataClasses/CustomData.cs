using System;
using UnityEngine;
namespace DataView
{
    class CustomData : IData
    {
        private int[] measures = new int[] { 3, 3, 3 };

        private double xSpacing = 1;
        private double ySpacing = 1;
        private double zSpacing = 1;

        public double GetPercentile(double value)
        {
            throw new NotImplementedException();
        }

        public double GetValue(double x, double y, double z)
        {

            if (x >= Measures[0] || y >= Measures[1] || z >= Measures[2])
                throw new ArgumentException("Out of bounds");

            double roundedX = Math.Min(NearestPoint(x, XSpacing), Measures[0] - 1);
            double roundedY = Math.Min(NearestPoint(y, YSpacing), Measures[1] - 1);
            double roundedZ = Math.Min(NearestPoint(z, ZSpacing), Measures[2] - 1);

            return roundedX + roundedY * 3 + roundedZ * 9;
        }

        public double GetValue(Point3D p)
        {
            return GetValue(p.X, p.Y, p.Z);
        }

        /// <summary>
        /// This function takes in a value and rounds it to the nearest value that is a multiplier of spacing
        /// </summary>
        /// <param name="value">Value to be rounded</param>
        /// <param name="spacing">Spacing</param>
        /// <returns>Returns rounded value</returns>
        private double NearestPoint(double value, double spacing)
        {

            int divider = (int)(value / spacing);
            double smallerNeighborDistance = value - (divider * spacing);
            double biggerNeighborDistance = ((divider + 1) * spacing) - value;

            return smallerNeighborDistance < biggerNeighborDistance ? (divider * spacing) : ((divider + 1) * spacing);
        }

        public double XSpacing { get => xSpacing; set => xSpacing = value > 0 ? value : xSpacing; }
        public double YSpacing { get => ySpacing; set => ySpacing = value > 0 ? value : ySpacing; }
        public double ZSpacing { get => zSpacing; set => zSpacing = value > 0 ? value : zSpacing; }

        public int[] Measures { get => measures; set => measures = value.Length == 3 ? value : measures; }

        public double MinValue => 0;

        public double MaxValue => 26;
    }


}
