using System;
using System.IO;
using MathNet.Numerics.LinearAlgebra;

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
        /// This transformation modifies the implementation of GetValue by moving centroid to origin
        /// </summary>
        private Transform3D transformation;

        /// <summary>
        /// Initializes the spacings between points, loads the data using Read method
        /// </summary>
        /// <param name="dataFileName">Path to metadata (mhd) file</param>
        public VolumetricData(FilePathDescriptor filePathDescriptor)
        {
            LoadMetadata(filePathDescriptor);
            ReadData(filePathDescriptor);
            InitializeTransformation();
        }

        /// <summary>
        /// Sets the transformation to have no effect
        /// </summary>
        private void InitializeTransformation()
        {
            this.transformation = new Transform3D(
                Matrix<double>.Build.DenseIdentity(3),
                Vector<double>.Build.Dense(3)
            );
        }

        /// <summary>
        /// This method returns transformation, thats been applied onto this object
        /// </summary>
        /// <returns>Returns transformation, that modifies this data</returns>
        public Transform3D GetTransformation()
        {
            return new Transform3D(this.transformation.RotationMatrix, this.transformation.TranslationVector);
        }

        /// <summary>
        /// This method translates the object to the origin.
        /// </summary>
        public void CenterObjectAroundOrigin()
        {
            Vector<double> translationVector = GetCenteringTranslation();
            Matrix<double> rotationMatrix = Matrix<double>.Build.DenseIdentity(3);
            //Matrix<double> rotationMatrix = GetCenteringRotation(translationVector);

            this.transformation = new Transform3D(
                rotationMatrix,
                translationVector
            );
        }

        private Vector<double> GetCenteringTranslation()
        {
            return Vector<double>.Build.DenseOfArray(new double[] {
                data.DimSize[0]/2.0, data.DimSize[1]/2.0, data.DimSize[2]/2.0
            });
        }

        private Matrix<double> GetCenteringRotation(Vector<double> translationVector)
        {
            Matrix<double> covarianceMatrix = Matrix<double>.Build.Dense(3, 3);

            for (double x = 0; x < data.DimSize[0]; x += XSpacing)
            {
                for (double y = 0; y < data.DimSize[1]; y += YSpacing)
                {
                    for (double z = 0; z < data.DimSize[2]; z += ZSpacing)
                        AddOuterProduct(covarianceMatrix, x - translationVector[0], y - translationVector[1], z - translationVector[2]);
                }
            }

            var evd = covarianceMatrix.Evd();

            //EigenVectors is the rotation matrix
            return evd.EigenVectors;
        }

        /// <summary>
        /// Modifies given matrix by adding outer product of vector [x,y,z]
        /// </summary>
        /// <param name="matrix">Matrix that gets modified</param>
        private void AddOuterProduct(Matrix<double> matrix, double x, double y, double z)
        {
            if (matrix.ColumnCount != 3 || matrix.RowCount != 3)
                throw new ArgumentException("");

            //Diagonals squares
            matrix[0, 0] += x * x;
            matrix[1, 1] += y * y;
            matrix[2, 2] += z * z;


            double multipliedXY = x * y;
            double multipliedXZ = x * z;
            double multipliedYZ = y * z;

            matrix[1, 0] += multipliedXY;
            matrix[0, 1] += multipliedXY;

            matrix[2, 0] += multipliedXZ;
            matrix[0, 2] += multipliedXZ;

            matrix[2, 1] += multipliedYZ;
            matrix[1, 2] += multipliedYZ;
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
            point = point.ApplyTranslationRotation(transformation);

            if (point.X < 0 || point.Y < 0 || point.Z < 0)
                throw new ArgumentException("This value is not within bounds");

            int xLDC = (int)(point.X / xSpacing); // coordinates of left down corner of the rectangle in the array in which the pixel is situated
            int yLDC = (int)(point.Y / ySpacing);
            int zLDC = (int)(point.Z / zSpacing);


            if (!IndicesWithinBounds(xLDC, yLDC, zLDC))
                throw new ArgumentException("This value is not within bounds");
            
            int zRDC = zLDC + 1;
            if (zLDC == this.Data.DimSize[2] - 1)
            {
                zRDC = zLDC;
            }

            double valueA = Interpolation2DReal(point.X, point.Y, zLDC, xLDC, yLDC);
            double valueB = Interpolation2DReal(point.X, point.Y, zRDC, xLDC, yLDC);

            return InterpolationReal(valueA, valueB, point.Z, zLDC, ZSpacing);            
        }

        private bool IndicesWithinBounds(double x, double y, double z)
        {
            return (x < Data.DimSize[0] && y < Data.DimSize[1] && z < Data.DimSize[2] && x >= 0 && y >= 0 && z >= 0);
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

        public override double GetPercentile(double value)
        {
            return this.dataDistribution.GetDistributionPercentage(value);
        }

        private double NormalizeValue(double value)
        {
            return (value - MinValue) / (MaxValue - MinValue);
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
