using System;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    public class Point3D
    {
        /// <summary>
        /// [0] = x
        /// [1] = y
        /// [2] = z
        /// </summary>
        private Vector<double> coordinates;

        /// <summary>
        /// Initializes a point with [0, 0, 0] coordinates
        /// </summary>
        public Point3D()
        {
            Constructor(0, 0, 0);
        }

        /// <summary>
        /// Initializes a point with given [x, y, z] coordinates
        /// </summary>
        /// <param name="x">Coordinate x</param>
        /// <param name="y">Coordinate y</param>
        /// <param name="z">Coordinate z</param>
        public Point3D(double x, double y, double z)
        {
            Constructor(x, y, z);
        }

        /// <summary>
        /// Vector with 3 values
        /// [0] = x
        /// [1] = y
        /// [2] = z
        /// Vector's dimension has to be 3
        /// </summary>
        /// <param name="coordinates">Vector [x,y,z]</param>
        public Point3D(Vector<double> coordinates)
        {
            if (coordinates.Count != 3)
                throw new ArgumentException("Vector's dimension has to be 3");

            this.coordinates = coordinates;
        }

        private void Constructor(double x, double y, double z)
        {
            this.coordinates = Vector<double>.Build.Dense(3);

            this.coordinates[0] = x;
            this.coordinates[1] = y;
            this.coordinates[2] = z;
        }

        /// <summary>
        /// Apply transformation on this point in order: firstly rotate, then translate
        /// </summary>
        /// <param name="transformation">Transformation applied on the point</param>
        /// <returns>Returns transformed point</returns>
        public Point3D ApplyRotationTranslation(Transform3D transformation)
        {
            //Console.WriteLine("Rotating point using: " + transformation.RotationMatrix);
            //Console.WriteLine("Translating point using: " + transformation.TranslationVector);

            //Console.WriteLine("Before transformation: {0}", ToString());
            Point3D resultPoint = Rotate(transformation.RotationMatrix);

            //Console.WriteLine("Point after applying rotation: {0}", resultPoint);
            resultPoint = resultPoint.Translate(transformation.TranslationVector);
            //Console.WriteLine("Point after applying translation: {0}", resultPoint);
            return resultPoint;
        }

        /// <summary>
        /// Apply transformation on this point in order: firstly translate, then rotate
        /// </summary>
        /// <param name="transformation">Transformation applied on the point</param>
        /// <returns>Returns transformed point</returns>
        public Point3D ApplyTranslationRotation(Transform3D transformation)
        {
            Point3D resultPoint = Translate(transformation.TranslationVector);
            return resultPoint.Rotate(transformation.RotationMatrix);
        }

        /// <summary>
        /// Calculates coordinates for point rotated using given rotation matrix
        /// </summary>
        /// <param name="m">Rotation matrix</param>
        /// <returns>Returns new coordinates for the original point</returns>
        public Point3D Rotate(Matrix<double> m)
        {
            Vector<double> newp = m.Multiply(coordinates);

            return new Point3D(newp);
        }

        public Point3D Translate(Vector<double> t)
        {
            if (t.Count != 3)
                throw new ArgumentException("Translation vector needs to have dimension 3");

            return new Point3D(this.coordinates + t);
        }

        public Point3D Translate(double x, double y, double z)
        {
            Vector<double> translationVector = Vector<double>.Build.DenseOfArray(new double[]
            {
                x, y, z
            });

            return Translate(translationVector);
        }

        /// <summary>
        /// Moves the point by coordinates in the passed array
        /// They need to be in the order bellow
        /// [offsetX, offsetY, offsetZ]
        /// </summary>
        /// <param name="t">Array with offsets</param>
        /// <returns>Returns the new coordinates of a point</returns>
        public Point3D Translate(double[] t)
        {
            Vector<double> translationVector = Vector<double>.Build.DenseOfArray(t);
            return Translate(translationVector);
        }

        /// <summary>
        /// Creates a copy of this instance
        /// </summary>
        /// <returns>Returns instance of a coppied point</returns>
        public Point3D Copy()
        {
            return new Point3D(this.X, this.Y, this.Z);
        }

        public double X { get => this.coordinates[0]; set => this.coordinates[0] = value; }
        public double Y { get => this.coordinates[1]; set => this.coordinates[1] = value; }
        public double Z { get => this.coordinates[2]; set => this.coordinates[2] = value; }
        public Vector<double> Coordinates { get => this.coordinates; set => this.coordinates = value; }

        /// <summary>
        /// ToString method shows basic information about the point
        /// </summary>
        /// <returns>Gives string with X, Y, Z coordinates for the given point</returns>
        public override string ToString()
        {
            return "x:" + Math.Round(X, 2) + " y:" + Math.Round(Y, 2) + " z:" + Math.Round(Z, 2);
        }

        /// <summary>
        /// Calculates the distance between this point and the passed one
        /// </summary>
        /// <param name="differentPoint">Point which is used to calculate the distance</param>
        /// <returns></returns>
        public double Distance(Point3D differentPoint)
        {
            return (this.coordinates - differentPoint.Coordinates).L2Norm();
        }

        public static Point3D operator+ (Point3D a, Point3D b)
        {
            return new Point3D(a.Coordinates + b.Coordinates);
        }

        public static Point3D operator -(Point3D a, Point3D b)
        {
            return new Point3D(a.Coordinates - b.Coordinates);
        }
    }
}
