using UnityEngine;
using DataView;

public class MockDataSegment : AMockObject
{
    private const int OUT_OF_BOUNDS = 10000;

    private AMockObject mockObject;
    private Transform3D transformation;

    private int[] measures;
    private int[] spacings;

	public MockDataSegment(AMockObject mockObject, int[] measures, int[] spacings)
	{
        this.mockObject = mockObject;
        this.measures = measures;
        this.spacings = spacings;
	}

    public void TransformObject(Transform3D expectedTransformation)
    {
        this.transformation = expectedTransformation.GetInverseTransformation();
    }

    public override double GetValue(double x, double y, double z)
    {
        return GetValue(new Point3D(x, y, z));
    }

    public override double GetValue(Point3D p)
    {
        Point3D currentPoint = p.ApplyRotationTranslation(transformation); //opposite order compared to registration algorithm

        if (mockObject.PointWithinBounds(currentPoint))
            return mockObject.GetValue(p);

        return OUT_OF_BOUNDS;
    }

    public override double XSpacing => spacings[0];
    public override double YSpacing => spacings[1];
    public override double ZSpacing => spacings[2];

    public override int[] Measures => measures;
}

