using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra;

public class Equation
{
	private Svd<double> leftSide;
	private Matrix<double> rightSide;

	public Equation(Svd<double> leftSide, Matrix<double> rightSide)
	{
		this.leftSide = leftSide;
		this.rightSide = rightSide;
	}

	public bool CheckCondition()
	{
		return leftSide.ConditionNumber < 1e10;
	}

	public Vector<double> GetEquationResult()
	{
		return leftSide.Solve(rightSide).Column(0);
	}
}

