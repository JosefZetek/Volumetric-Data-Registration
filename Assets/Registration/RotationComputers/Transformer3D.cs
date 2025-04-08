using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace DataView
{
    public class Transformer3D : ITransformer
    {
        public Transform3D GetTransformation(Match m, AData dataMicro, AData dataMacro)
        {
            Point3D pMicro = m.microFV.Point.Copy();
            Point3D pMacro = m.macroFV.Point.Copy();

            Vector<double> translationVector = Vector<double>.Build.Dense(3);
            Matrix<double> rotationMatrix;

            //Select min spacing
            double[] spacings = new double[] { dataMicro.XSpacing, dataMicro.YSpacing, dataMicro.ZSpacing, dataMacro.XSpacing, dataMacro.YSpacing, dataMacro.ZSpacing };
            double minSpacing = spacings.Min();

            try { rotationMatrix = UniformRotationComputerPCA.CalculateRotation(dataMicro, dataMacro, pMicro, pMacro, minSpacing); }
            catch (Exception e) { throw e; }

            pMicro = pMicro.Rotate(rotationMatrix);

            translationVector[0] = pMacro.X - pMicro.X; // real coordinates
            translationVector[1] = pMacro.Y - pMicro.Y;
            translationVector[2] = pMacro.Z - pMicro.Z;

            return new Transform3D(rotationMatrix, translationVector);
        }

        public void AppendTransformation(Match m, AData dataMicro, AData dataMacro, ref List<Transform3D> transformations)
        {
            Point3D pMicro = m.microFV.Point.Copy();
            Point3D pMacro = m.macroFV.Point.Copy();

            //Select min spacing
            double[] spacings = new double[] { dataMicro.XSpacing, dataMicro.YSpacing, dataMicro.ZSpacing, dataMacro.XSpacing, dataMacro.YSpacing, dataMacro.ZSpacing };
            double minSpacing = spacings.Min();

            try
            {
                Matrix<double> rotationMatrix = UniformRotationComputerPCA.CalculateRotation(dataMicro, dataMacro, pMicro, pMacro, minSpacing);

                Vector<double> translationVector = Vector<double>.Build.Dense(3);
                Transform3D currentTransformation;

                pMicro = pMicro.Rotate(rotationMatrix);

                translationVector[0] = pMacro.X - pMicro.X; // real coordinates
                translationVector[1] = pMacro.Y - pMicro.Y;
                translationVector[2] = pMacro.Z - pMicro.Z;

                currentTransformation = new Transform3D(rotationMatrix, translationVector);
                transformations.Add(currentTransformation);


            }
            catch (Exception e) { throw e; }
        }
    }
}