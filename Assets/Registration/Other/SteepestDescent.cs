using System;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
	public class SteepestDescent
	{

		public SteepestDescent(Matrix<double> coeficientMatrix)
		{
			Matrix<double> modifiedMatrixA = ModifyCoeficientMatrix(coeficientMatrix);
			Vector<double> rightSide = Vector<double>.Build.DenseOfArray(new double[] { 31, 20, 12 });

            Console.WriteLine(calculateEquations(coeficientMatrix, rightSide));
			Console.WriteLine(calculateEquations(modifiedMatrixA, rightSide));
		}


		private Matrix<double> ModifyCoeficientMatrix(Matrix<double> coeficientMatrix)
		{
            return coeficientMatrix.Multiply(0.5) + coeficientMatrix.Transpose();
        }

		private Vector<double> calculateEquations(Matrix<double> coeficientMatrix, Vector<double> rightSide)
		{

			Matrix<double> equationMatrix = Matrix<double>.Build.Dense(coeficientMatrix.RowCount, coeficientMatrix.ColumnCount + 1);


			for(int i = 0; i<coeficientMatrix.ColumnCount; i++)
			{
				equationMatrix.SetColumn(i, coeficientMatrix.Column(i));
			}

			equationMatrix.SetColumn(equationMatrix.ColumnCount - 1, rightSide);

			return EquationComputer.CalculateSolution(equationMatrix);
		}


	}
}

