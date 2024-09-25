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
        private double iSpacing;
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
                     //   VData[k] = new int[width, depth];

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
                    //int numberOfBits = 8; //One byte data type
                    //volumetricDataDistribution = new VolumetricDataDistribution(step: 1, (1 << numberOfBits) - 1);

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
                {
                    Console.WriteLine("Wrong element type.");
                }

                br.Close();
                return VData;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="xRes"></param>
        /// <param name="yRes"></param>
        /// <param name="spacing"></param>
        /// <returns></returns>
        public double[,] Cut(double[] point, double[] v1, double[] v2, int xRes, int yRes, double spacing)
        {
            ISpacing = spacing;

            double[] vertical2 = Orthogonalize2D(v1, v2);
            double lengthV1 = Math.Sqrt(ScalarProduct(v1, v1));
            double lengthV2 = Math.Sqrt(ScalarProduct(vertical2, vertical2));

            double[] unitVector1 = new double[3];
            double[] unitVector2 = new double[3];
            double[] spacings = new double[] { XSpacing, YSpacing, ZSpacing };
            for (int i = 0; i < v1.Length; i++)
            {
                unitVector1[i] = v1[i] * ISpacing / lengthV1;
                unitVector2[i] = vertical2[i] * ISpacing / lengthV2;
            }

            double[,] cut = new double[xRes, yRes];
            double x, y, z;
            for (int i = 0; i < yRes; i++)
            {
                x = point[0] + i * unitVector2[0];
                y = point[1] + i * unitVector2[1];
                z = point[2] + i * unitVector2[2];

                for (int j = 0; j < xRes; j++)
                {
                    cut[j, i] = GetValue(x, y, z);

                    x += unitVector1[0];
                    y += unitVector1[1];
                    z += unitVector1[2];
                }
            }
            return cut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <param name="xRes"></param>
        /// <param name="yRes"></param>
        /// <param name="zRes"></param>
        /// <param name="spacing"></param>
        /// <returns></returns>
        public VolumetricData Cut3D(double[] point, double[] v1, double[] v2, double[] v3, int xRes, int yRes, int zRes, double spacing)
        {
            ISpacing = spacing;

            double[] vertical2 = Orthogonalize2D(v1, v2);
            double[] vertical3 = Orthogonalize3D(v1, vertical2, v3);
            double lengthV1 = Math.Sqrt(ScalarProduct(v1, v1));
            double lengthV2 = Math.Sqrt(ScalarProduct(vertical2, vertical2));
            double lengthV3 = Math.Sqrt(ScalarProduct(vertical3, vertical3));

            double[] unitVector1 = new double[3];
            double[] unitVector2 = new double[3];
            double[] unitVector3 = new double[3];
            for (int i = 0; i < v1.Length; i++)
            {
                unitVector1[i] = v1[i] * ISpacing / lengthV1;
                unitVector2[i] = vertical2[i] * ISpacing / lengthV2;
                unitVector3[i] = vertical3[i] * ISpacing / lengthV3;
            }

            int[][,] cut = new int[zRes][,];
            double x, y, z;

            for (int k = 0; k < zRes; k++)
            {
                cut[k] = new int[xRes, yRes];
                for (int i = 0; i < xRes; i++)
                {
                    x = point[0] + i * unitVector2[0] + k * unitVector3[0];
                    y = point[1] + i * unitVector2[1] + k * unitVector3[1];
                    z = point[2] + i * unitVector2[2] + k * unitVector3[2];

                    for (int j = 0; j < yRes; j++)
                    {
                        cut[k][i, j] = (int)GetValueMatrixCoordinates(x, y, z);

                        x += unitVector1[0];
                        y += unitVector1[1];
                        z += unitVector1[2];
                    }
                }
            }

            VolumetricData vDnew = new VolumetricData(Data);
            vDnew.VData = cut;

            return vDnew;
        }

        public VolumetricData CutVol(int[] translation)
        {
            int xRes = Measures[0] - translation[0];
            int yRes = Measures[1] - translation[1];
            int zRes = Measures[2] - translation[2];

            int[][,] cut = new int[zRes][,];

            for (int k = 0; k < zRes; k++)
            {
                cut[k] = new int[xRes, yRes];
                for (int i = 0; i < xRes; i++)
                {
                    for (int j = 0; j < yRes; j++)
                    {
                        cut[k][i, j] = VData[translation[2] + k][translation[0] + i, translation[1] + j];
                    }
                }
            }

            Data copy = new Data(this.Data);
            VolumetricData vDnew = new VolumetricData(copy);
            vDnew.VData = cut;
            vDnew.Measures = new int[] { xRes, yRes, zRes };

            return vDnew;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <param name="xRes"></param>
        /// <param name="yRes"></param>
        /// <param name="zRes"></param>
        /// <param name="spacing"></param>
        /// <returns></returns>
        public int[][,] Cut3D2(double[] point, double[] v1, double[] v2, double[] v3, int xRes, int yRes, int zRes, double spacing)
        {
            ISpacing = spacing;

            double[] vertical2 = Orthogonalize2D(v1, v2);
            double[] vertical3 = Orthogonalize3D(v1, vertical2, v3);
            double lengthV1 = Math.Sqrt(ScalarProduct(v1, v1));
            double lengthV2 = Math.Sqrt(ScalarProduct(vertical2, vertical2));
            double lengthV3 = Math.Sqrt(ScalarProduct(vertical3, vertical3));

            double[] unitVector1 = new double[3];
            double[] unitVector2 = new double[3];
            double[] unitVector3 = new double[3];
            for (int i = 0; i < v1.Length; i++)
            {
                unitVector1[i] = v1[i] * ISpacing / lengthV1;
                unitVector2[i] = vertical2[i] * ISpacing / lengthV2;
                unitVector3[i] = vertical3[i] * ISpacing / lengthV3;
            }

            int[][,] cut = new int[zRes][,];
            for (int k = 0; k < zRes; k++)
            {
                cut[k] = new int[xRes, yRes];
                for (int i = 0; i < xRes; i++)
                {
                    double x = point[0] + i * unitVector2[0] + k * unitVector3[0];
                    double y = point[1] + i * unitVector2[1] + k * unitVector3[1];
                    double z = point[2] + i * unitVector2[2] + k * unitVector3[2];
                    for (int j = 0; j < yRes; j++)
                    {
                        cut[k][i, j] = (int)GetValueMatrixCoordinates(x, y, z);

                        x += unitVector1[0];
                        y += unitVector1[1];
                        z += unitVector1[2];
                    }
                }
            }
            return cut;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private double[] Orthogonalize2D(double[] v1, double[] v2)
        {
            double alpha = -ScalarProduct(v1, v2) / ScalarProduct(v1, v1);

            double[] verticalVector = new double[3];
            for (int i = 0; i < verticalVector.Length; i++)
            {
                verticalVector[i] = v2[i] + alpha * v1[i];
            }
            return verticalVector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        /// <returns></returns>
        private double[] Orthogonalize3D(double[] v1, double[] v2, double[] v3)
        {
            double betha1 = -ScalarProduct(v1, v3) / ScalarProduct(v1, v1);
            double betha2 = -ScalarProduct(v2, v3) / ScalarProduct(v2, v2);

            double[] verticalVector = new double[3];
            for (int i = 0; i < verticalVector.Length; i++)
            {
                verticalVector[i] = v3[i] + betha1 * v1[i] + betha2 * v2[i];
            }
            return verticalVector;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private double ScalarProduct(double[] v1, double[] v2)
        {
            return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public double GetValueMatrixCoordinates(double x, double y, double z) // Interpolation3D in matrix coordinates
        {
            //TODO CUTS - real or matrix coordinates

            int xLDC = (int)(x); // coordinates of left down corner of the rectangle in the array in which the pixel is situated
            int yLDC = (int)(y);
            int zLDC = (int)(z);

            if (xLDC < Data.DimSize[0] && yLDC < Data.DimSize[1] && zLDC < Data.DimSize[2] && xLDC >= 0 && yLDC >= 0 && zLDC >= 0)
            {
                int zRDC = zLDC + 1;
                if (zLDC == this.Data.DimSize[2] - 1)
                {
                    zRDC = zLDC;
                }

                double valueA = Interpolation2DWithinMatrix(x, y, zLDC, xLDC, yLDC);
                double valueB = Interpolation2DWithinMatrix(x, y, zRDC, xLDC, yLDC);

                return InterpolationWithinMatrix(valueA, valueB, z, zLDC);
            }
            else
            {
                //Console.WriteLine(-1);
                return 0;
            }
        }

        public double GetValueMatrixCoordinates(Point3D point)
        {
            return GetValueMatrixCoordinates(point.X, point.Y, point.Z);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueA"></param>
        /// <param name="valueB"></param>
        /// <param name="coordinateOfPixel"></param>
        /// <param name="indexOfA"></param>
        /// <returns></returns>
        private double InterpolationWithinMatrix(double valueA, double valueB, double coordinateOfPixel, int indexOfA)
        {
            double d = coordinateOfPixel - indexOfA;
            return d * valueB + (1 - d) * valueA;
        }

        private double InterpolationReal(double valueA, double valueB, double coordinateOfPixel, int indexOfA, double spacing)
        {
            double d = coordinateOfPixel - indexOfA * spacing; //TODO positive/zero?
            double r = d / spacing;
            return r * valueB + (1 - r) * valueA;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pixelX"></param>
        /// <param name="pixelY"></param>
        /// <param name="indexLDCZ"></param>
        /// <param name="xLDC"></param>
        /// <param name="yLDC"></param>
        /// <returns></returns>
        private double Interpolation2DWithinMatrix(double pixelX, double pixelY, int indexLDCZ, int xLDC, int yLDC)
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

            double helpValueA = InterpolationWithinMatrix(valueA, valueB, pixelX, xLDC);

            int valueC = VData[indexLDCZ][xLDC, yRDC];
            int valueD = VData[indexLDCZ][xRDC, yRDC];

            double helpValueB = InterpolationWithinMatrix(valueC, valueD, pixelX, xLDC);

            return InterpolationWithinMatrix(helpValueA, helpValueB, pixelY, yLDC);
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

        public double GetValueDistribution(double value)
        {
            return this.dataDistribution.GetDistributionPercentage(value);
        }

        public double XSpacing { get => xSpacing; set => xSpacing = value; }
        public double YSpacing { get => ySpacing; set => ySpacing = value; }
        public double ZSpacing { get => zSpacing; set => zSpacing = value; }
        public double ISpacing { get => iSpacing; set => iSpacing = value; }
        internal Data Data { get => data; set => data = value; }
        public int[] Measures { get => Data.DimSize; set => Data.DimSize = value; }
        public int[][,] VData { get => vData; set => vData = value; }
        public double MinValue { get => dataDistribution.MinValue; }
        public double MaxValue { get => dataDistribution.MaxValue; }
    }
}
