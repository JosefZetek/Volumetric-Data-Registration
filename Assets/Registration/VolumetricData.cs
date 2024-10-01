using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    /// <summary>
    /// This class represents the data
    /// </summary>
    class VolumetricData : IData
    {
        private int[][,] vData;
        private VolumetricDataDistribution dataDistribution;
        private double xSpacing;
        private double ySpacing;
        private double zSpacing;
        private Data data;

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

            XSpacing = this.Data.ElementSpacing[0];
            YSpacing = this.Data.ElementSpacing[1];
            ZSpacing = this.Data.ElementSpacing[2];
        }

        public VolumetricData(Data data)
        {
            XSpacing = data.ElementSpacing[0];
            YSpacing = data.ElementSpacing[1];
            ZSpacing = data.ElementSpacing[2];
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

        public double GetValue(double x, double y, double z) // Interpolation3D in real coordinates 
        {

            if(x < 0 || y < 0 || z < 0)
                throw new ArgumentException("This value is not within bounds");

            int xLDC = (int)(x / xSpacing); // coordinates of left down corner of the rectangle in the array in which the pixel is situated
            int yLDC = (int)(y / ySpacing);
            int zLDC = (int)(z / zSpacing);
            

            //Interpolated value is within bounds
            if (xLDC < Data.DimSize[0] && yLDC < Data.DimSize[1] && zLDC < Data.DimSize[2] && xLDC >= 0 && yLDC >= 0 && zLDC >= 0)
            {
                int zRDC = zLDC + 1;
                if (zLDC == this.Data.DimSize[2] - 1)
                {
                    zRDC = zLDC;
                }

                double valueA = Interpolation2DReal(x, y, zLDC, xLDC, yLDC);
                double valueB = Interpolation2DReal(x, y, zRDC, xLDC, yLDC);

                return InterpolationReal(valueA, valueB, z, zLDC, ZSpacing);
            }

            throw new ArgumentException("This value is not within bounds");
        }

        public double GetValue(Point3D point)
        {
            return GetValue(point.X, point.Y, point.Z);
        }

        public double GetSampledValue(double x, double y, double z)
        {
            double xOrder = x / xSpacing;
            double yOrder = y / ySpacing;
            double zOrder = z / zSpacing;

            int xIndex = (int)xOrder; // coordinates of left down corner of the rectangle in the array in which the pixel is situated
            int yIndex = (int)yOrder;
            int zIndex = (int)zOrder;

            if (xOrder > xIndex || yOrder > yIndex || zOrder > zIndex)
                throw new ArgumentException("Value at given point was not specifically sampled (use GetValue) instead to get interpolated result");

            if (xIndex > Data.DimSize[0] || yIndex > Data.DimSize[1] || zIndex > Data.DimSize[2] || xIndex < 0 || yIndex < 0 || zIndex < 0)
                throw new ArgumentException("Value is out of bounds");

            return VData[zIndex][xIndex, yIndex];
        }

        private double InterpolationReal(double valueA, double valueB, double coordinateOfPixel, int indexOfA, double spacing)
        {
            double d = coordinateOfPixel - indexOfA * spacing; //TODO positive/zero?
            double r = d / spacing;
            return r * valueB + (1 - r) * valueA;
        }

        private double Interpolation2DReal(double pixelX, double pixelY, int indexLDCZ, int xLDC, int yLDC)
        {
            int xRDC = xLDC + 1;
            int yRDC = yLDC + 1;

            if (xLDC == this.Data.DimSize[0] - 1)
            {
                xRDC = xLDC;
            }

            if (yLDC == this.Data.DimSize[1] - 1)
            {
                yRDC = yLDC;
            }

            int valueA = VData[indexLDCZ][xLDC, yLDC];
            int valueB = VData[indexLDCZ][xRDC, yLDC];


            double helpValueA = InterpolationReal(valueA, valueB, pixelX, xLDC, XSpacing);

            int valueC = VData[indexLDCZ][xLDC, yRDC];
            int valueD = VData[indexLDCZ][xRDC, yRDC];

            double helpValueB = InterpolationReal(valueC, valueD, pixelX, xLDC, XSpacing);

            return InterpolationReal(helpValueA, helpValueB, pixelY, yLDC, YSpacing);
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

        public double GetPercentile(double value)
        {
            return this.dataDistribution.GetDistributionPercentage(value);
        }

        private double NormalizeValue(double value)
        {
            return (value - MinValue) / (MaxValue - MinValue);
        }

        public int[] Measures { get => Data.DimSize; set => Data.DimSize = value; }

        public double XSpacing { get => xSpacing; set => xSpacing = value; }
        public double YSpacing { get => ySpacing; set => ySpacing = value; }
        public double ZSpacing { get => zSpacing; set => zSpacing = value; }

        internal Data Data { get => data; set => data = value; }

        public int[][,] VData { get => vData; set => vData = value; }

        public double MinValue { get => dataDistribution.MinValue; }
        public double MaxValue { get => dataDistribution.MaxValue; }
    }
}
