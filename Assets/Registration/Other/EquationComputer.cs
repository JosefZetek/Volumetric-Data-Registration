using System;
using MathNet.Numerics.LinearAlgebra;

namespace DataView
{
	public class EquationComputer
	{
        /// <summary>
        /// Calculates the solution for given equation in a form of matrix
        /// </summary>
        /// <param name="equationMatrix">Matrix with equation</param>
        /// <returns>Returns vector for variables in the columns</returns>
        /// <exception cref="ArgumentException">Throws exception when the data are invalid</exception>
        public static Vector<double> CalculateSolution(Matrix<double> equationMatrix)
		{
			if ((equationMatrix.ColumnCount) != (equationMatrix.RowCount + 1))
				throw new ArgumentException("Matrix should have n rows and n+1 columns");

			//Gauss elimination
			for(int i = 0; i<equationMatrix.RowCount-1; i++)
			{
				MakeTriangularMatrix(equationMatrix, i);
				
                double primaryElement = equationMatrix[i, i];

				for(int j = i+1; j<equationMatrix.RowCount; j++)
				{
					double secondaryElement = equationMatrix[j, i];
					double factor = - secondaryElement / primaryElement;

					Vector<double> resultRow = equationMatrix.Row(i).Multiply(factor);
					resultRow = resultRow.Add(equationMatrix.Row(j));

					equationMatrix.SetRow(j, resultRow);
                }
			}

            Vector<double> equationSolution = Vector<double>.Build.Dense(equationMatrix.RowCount);

            //Finding solution
            for (int i = equationMatrix.RowCount-1; i>= 0; i--)
			{
				if (equationMatrix[i, i] == 0)
					throw new ArgumentException("The matrix has linearly dependent rows, ie. multiple solutions exist");


                double result = equationMatrix[i, equationMatrix.ColumnCount - 1];

                //There are other variables right from the currently calculated variable
                for (int j = i+1; j< equationMatrix.ColumnCount - 1; j++)
                    result -= equationMatrix[i, j] * equationSolution[j];

                result /= equationMatrix[i, i];
				equationSolution[i] = result;
			}

			return equationSolution;
		}

		/// <summary>
		/// Aligns the rows so that pivot on the next line is not on lower index (shifted towards left)
		/// </summary>
		/// <param name="equationMatrix">Matrix where the rows are equations</param>
		/// <param name="row">Row underr which the sorting need to be done</param>
		private static void MakeTriangularMatrix(Matrix<double> equationMatrix, int row)
		{
			Vector<double>[] vectorRows = new Vector<double>[equationMatrix.RowCount - row];

			//Making a copy of the rows
			for (int i = 0; i < vectorRows.Length; i++)
                vectorRows[i] = equationMatrix.Row(i + row);

            try
			{
                Array.Sort(vectorRows, (o1, o2) => CalculateVectorValue(o1).CompareTo(CalculateVectorValue(o2)));
				Array.Reverse(vectorRows);
            }
			catch (Exception e)
			{
				throw e;
			}
            
			//Setting them in the correct order
			for(int i = 0; i<vectorRows.Length; i++)
                equationMatrix.SetRow(row + i, vectorRows[i]);
		}

		/// <summary>
		/// Calculates a value of a vector
		/// The calculations are inverted so when using to sort an array, it needs to be reversed
		/// This method is used to sort rows of matrix in a way where the pivots of lower rows are not located at lower column index (shifted to left)
		/// </summary>
		/// <param name="vector">Vector to calculate the value</param>
		/// <returns>Returns a value of a vector</returns>
		private static int CalculateVectorValue(Vector<double> vector)
		{
			int vectorValue = 0;

			for(int i = 0; i<vector.Count; i++)
                vectorValue += (vector[i] != 0) ? (1 << (vector.Count-1-i)) : 0;

            if (vectorValue == 0)
				throw new ArgumentException("Equations are linearly dependent, ie. multiple solutions exist.");

			if (vectorValue == 1)
				throw new ArgumentException("There is no solution to the given set of equations.");

            return vectorValue;
		}
	}
}

