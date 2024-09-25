using System;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
    class Transform3D
    {
        private Matrix<double> rotationMatrix;
        private Vector<double> translationVector;        

        public Transform3D(Matrix<double> rotationMatrix, Vector<double> translationVector)
        {
            RotationMatrix = rotationMatrix;
            TranslationVector = translationVector;
        }

        public Matrix<double> RotationMatrix { get => rotationMatrix; set => rotationMatrix = value; }
        public Vector<double> TranslationVector { get => translationVector; set => translationVector = value; }

        public override string ToString()
        {
            return RotationMatrix.ToString() + TranslationVector.ToString();
        }
    }
}
