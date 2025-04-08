using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra;

public class Equation
{
	private QR<double> leftSide;
	private Matrix<double> rightSide;

	public Equation(QR<double> leftSide, Matrix<double> rightSide)
	{
		this.leftSide = leftSide;
		this.rightSide = rightSide;
	}

	public Vector<double> GetEquationResult()
	{
		return leftSide.Solve(rightSide).Column(0);
	}
}

