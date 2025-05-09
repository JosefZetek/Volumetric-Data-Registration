﻿using System;
using System.IO;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace DataView
{
    /// <summary>
    /// This class represents the data
    /// </summary>
    public class VolumetricData : AData
    {
        /* Data itself */
        private int[][,] vData;

        /* Spacings */
        private double xSpacing;
        private double ySpacing;
        private double zSpacing;

        /* Data information */
        private Data data;
        private VolumetricDataDistribution dataDistribution;

        /// <summary>
        /// Initializes the spacings between points, loads the data using Read method
        /// </summary>
        /// <param name="dataFileName">Path to metadata (mhd) file</param>
        public VolumetricData(FilePathDescriptor filePathDescriptor)
        {
            LoadMetadata(filePathDescriptor);
            ReadData(filePathDescriptor);
        }

        private void LoadMetadata(FilePathDescriptor filePathDescriptor)
        {
            this.Data = new Data(filePathDescriptor.MHDFilePath);

            xSpacing = this.Data.ElementSpacing[0];
            ySpacing = this.Data.ElementSpacing[1];
            zSpacing = this.Data.ElementSpacing[2];
        }

        public VolumetricData(Data data)
        {
            xSpacing = data.ElementSpacing[0];
            ySpacing = data.ElementSpacing[1];
            zSpacing = data.ElementSpacing[2];
            this.Data = data;
        }

        /// <summary>
        /// Reads the raw data from a file
        /// </summary>
        /// <returns>Returns array with the data</returns>
        private int[][,] ReadData(FilePathDescriptor filePathDescriptor)
        {
            using (BinaryReader br = new BinaryReader(new FileStream(filePathDescriptor.DataFilePath, FileMode.Open)))
            {
                int width = Data.DimSize[0];
                int depth = Data.DimSize[1];
                int height = Data.DimSize[2];

                VData = new int[height][,];
                int c = 0;

                if (Data.ElementType == "MET_USHORT")
                {
                    dataDistribution = new VolumetricDataDistribution();
                    for (int k = 0; k < height; k++)
                    {
                        VData[k] = new int[width, depth];
                        for (int j = 0; j < depth; j++)
                        {
                            for (int i = 0; i < width; i++)
                            {
                                byte a = br.ReadByte();
                                byte b = br.ReadByte();
                                c = 256 * b + a;

                                VData[k][i, j] = c;

                                dataDistribution.AddValue(c);
                            }
                        }
                    }
                }

                else if (Data.ElementType == "MET_UCHAR")
                {
                    dataDistribution = new VolumetricDataDistribution();

                    for (int k = 0; k < height; k++)
                    {
                        VData[k] = new int[width, depth];
                        for (int i = 0; i < width; i++)
                        {
                            for (int j = 0; j < depth; j++)
                            {
                                c = br.ReadByte();

                                VData[k][i, j] = c;

                                dataDistribution.AddValue(c);
                            }
                        }
                    }

                }
                else
                    Console.WriteLine("Wrong element type.");

                br.Close();
                return VData;
            }
        }

        public override double GetValue(double x, double y, double z) // Interpolation3D in real coordinates 
        {
            return GetValue(new Point3D(x, y, z));
        }

        public override double GetValue(Point3D point)
        {

            // coordinates of left down corner of the rectangle in the array in which the pixel is situated
            int xIndexLower = ConstrainIndex((int)(point.X / XSpacing), Data.DimSize[0] - 1);
            int yIndexLower = ConstrainIndex((int)(point.Y / YSpacing), Data.DimSize[1] - 1);
            int zIndexLower = ConstrainIndex((int)(point.Z / ZSpacing), Data.DimSize[2] - 1);

            int zIndexHigher = ConstrainIndex(zIndexLower + 1, Data.DimSize[2] - 1);

            double interpolatedXYLowerZ = InterpolationXYPlane(point.X, point.Y, zIndexLower, xIndexLower, yIndexLower);
            double interpolatedXYHigherZ = InterpolationXYPlane(point.X, point.Y, zIndexHigher, xIndexLower, yIndexLower);

            return InterpolationReal(interpolatedXYLowerZ, interpolatedXYHigherZ, point.Z, zIndexLower*ZSpacing, ZSpacing);
        }

        /// <summary>
        /// Method constrain given index for array to satisfy
        ///  index >= 0 && index <= maxIndex
        /// </summary>
        /// <param name="currentIndex">Current index</param>
        /// <param name="maxIndex">Maximum index</param>
        /// <returns></returns>
        private int ConstrainIndex(int currentIndex, int maxIndex)
        {
            return Math.Max(Math.Min(currentIndex, maxIndex), 0);
        }

        /// <summary>
        /// 1D Interpolation between valueA and valueB
        /// </summary>
        /// <param name="valueA">The value at the closest sampled coordinate that is smaller</param>
        /// <param name="valueB">The value at the closest sampled coordinate that is higher</param>
        /// <param name="interpolationCoordinate">Interpolated point's X/Y/Z coordinate<param>
        /// <param name="aPosition">Position of A (coordinates)</param>
        /// <param name="spacing">Spacing between A and B</param>
        /// <returns>Returns interpolated value</returns>
        private double InterpolationReal(double valueA, double valueB, double interpolationCoordinate, double aPosition, double spacing)
        {
            double ratio = (interpolationCoordinate - aPosition) / spacing;
            return ratio * valueB + (1 - ratio) * valueA;
        }

        /// <summary>
        /// Interpolation for X,Y plane
        /// </summary>
        /// <param name="xInterpolationCoordinate">Interpolated point's X coordinate</param>
        /// <param name="yInterpolationCoordinate">Interpolated point's Y coordinate</param>
        /// <param name="zIndex">Z index - constant (higher or lower zIndex)</param>
        /// <param name="xIndexLower">Lower X index</param>
        /// <param name="yIndexLower">Lower Y index</param>
        /// <returns>Returns interpolated value at given X, Y coordinates</returns>
        private double InterpolationXYPlane(double xInterpolationCoordinate, double yInterpolationCoordinate, int zIndex, int xIndexLower, int yIndexLower)
        {
            int xIndexHigher = ConstrainIndex(xIndexLower + 1, Data.DimSize[0] - 1);
            int yIndexHigher = ConstrainIndex(yIndexLower + 1, Data.DimSize[1] - 1);

            int valueA = VData[zIndex][xIndexLower, yIndexLower];
            int valueB = VData[zIndex][xIndexHigher, yIndexLower];

            double interpolationLowerY = InterpolationReal(valueA, valueB, xInterpolationCoordinate, xIndexLower*XSpacing, XSpacing);

            int valueC = VData[zIndex][xIndexLower, yIndexHigher];
            int valueD = VData[zIndex][xIndexHigher, yIndexHigher];

            double interpolationHigherY = InterpolationReal(valueC, valueD, xInterpolationCoordinate, xIndexLower*XSpacing, XSpacing);

            return InterpolationReal(interpolationLowerY, interpolationHigherY, yInterpolationCoordinate, yIndexLower*YSpacing, YSpacing);
        }

        public int GetMax()
        {
            int max = Int16.MinValue;
            int width = Data.DimSize[0];
            int depth = Data.DimSize[1];
            int height = Data.DimSize[2];

            for (int k = 0; k < height; k++)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < depth; j++)
                    {
                        int c = VData[k][i, j];
                        if (c > max)
                        {
                            max = c;
                        }
                    }
                }
            }
            return max;
        }

        public int GetMin()
        {
            int min = Int16.MaxValue;
            int width = Data.DimSize[0];
            int depth = Data.DimSize[1];
            int height = Data.DimSize[2];

            for (int k = 0; k < height; k++)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < depth; j++)
                    {
                        int c = VData[k][i, j];
                        if (c < min)
                        {
                            min = c;
                        }
                    }
                }
            }
            return min;
        }

        public int[] GetHistogram()
        {
            int max = this.GetMax();
            int[] histo = new int[max + 1];

            int width = Data.DimSize[0];
            int depth = Data.DimSize[1];
            int height = Data.DimSize[2];

            for (int k = 0; k < height; k++)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < depth; j++)
                    {
                        int c = VData[k][i, j];
                        histo[c]++;
                    }
                }
            }
            return histo;
        }

        public override double GetPercentile(double value)
        {
            return this.dataDistribution.GetDistributionPercentage(value);
        }

        public override int[] Measures { get => Data.DimSize; }

        public override double XSpacing { get => xSpacing; }
        public override double YSpacing { get => ySpacing; }
        public override double ZSpacing { get => zSpacing; }

        internal Data Data { get => data; set => data = value; }

        public int[][,] VData { get => vData; set => vData = value; }

        public override double MinValue { get => dataDistribution.MinValue; }
        public override double MaxValue { get => dataDistribution.MaxValue; }

        
    }
}
