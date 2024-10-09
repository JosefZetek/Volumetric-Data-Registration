using System;
using DataView;

public class EllipsoidMockData : AMockObject
{

	private const int DIMENSIONS = 3;
	private int a;
	private int b;
	private int c;

    private int[] measures;
    private int[] spacings;

    public override int[] Measures { get => measures; }

    public override double XSpacing { get => spacings[0]; }
    public override double YSpacing { get => spacings[1]; }
    public override double ZSpacing { get => spacings[2]; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="Measures"></param>
    /// <param name="Spacings"></param>
    public EllipsoidMockData(int a, int b, int c, int[] Measures, int[] Spacings)
	{
		this.a = a;
		this.b = b;
		this.c = c;

        ParameterIntegrityCheck(Measures, "Measures");
        ParameterIntegrityCheck(Spacings, "Spacings");

        this.measures = Measures;
        this.spacings = Spacings;
    }

    private void ParameterIntegrityCheck(int[] parameter, string parameterName)
    {
        if (parameter.Length != DIMENSIONS)
            throw new ArgumentException(parameterName + " should have length = " + DIMENSIONS);

        if (!CheckArrayPositiveValues(parameter))
            throw new ArgumentException(parameterName + " array should contain values higher than 0");
    }

    /// <summary>
    /// Checks whether all values in array are more than 0. Made for check of measures and sizing properties.
    /// </summary>
    /// <param name="CheckedArray">Array thats going to be checked.</param>
    /// <returns>Returns false if at least one value is 0 or bellow.</returns>
    private bool CheckArrayPositiveValues(int[] CheckedArray)
    {
        for (int i = 0; i<CheckedArray.Length; i++)
        {
            if (CheckedArray[i] <= 0)
                return false;
        }

        return true;
    }

    public override double GetValue(double x, double y, double z)
    {
        double currentValue = Math.Pow(x, 2) / Math.Pow(a, 2) + Math.Pow(y, 2) / Math.Pow(b, 2) + Math.Pow(z, 2) / Math.Pow(c, 2);
        if (currentValue <= 1)
            return currentValue * 4000;

        return 5000;
    }

    public override double GetValue(Point3D p)
    {
        return GetValue(p.X, p.Y, p.Z);
    }
}

