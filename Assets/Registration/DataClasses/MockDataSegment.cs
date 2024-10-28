using MathNet.Numerics.LinearAlgebra;
using DataView;

public class MockDataSegment : AMockObject
{
    private const int OUT_OF_BOUNDS = 5000;

    private AMockObject referenceObject;
    private Transform3D transformation;

    private int[] measures;
    private double[] spacings;


    private Vector<double> canonicalBaseXTransformed;
    private Vector<double> canonicalBaseYTransformed;
    private Vector<double> canonicalBaseZTransformed;


    public MockDataSegment(AMockObject referenceObject, int[] measures, double[] spacings)
    {
        this.referenceObject = referenceObject;
        this.measures = measures;
        this.spacings = spacings;
        TransformObject(new Transform3D());
    }

    public void TransformObject(Transform3D expectedTransformation)
    {
        this.transformation = expectedTransformation;

        this.canonicalBaseXTransformed = new Point3D(1, 0, 0)
            .Rotate(expectedTransformation.RotationMatrix)
            .Coordinates;

        this.canonicalBaseYTransformed = new Point3D(0, 1, 0)
            .Rotate(expectedTransformation.RotationMatrix)
            .Coordinates;

        this.canonicalBaseZTransformed = new Point3D(0, 0, 1)
            .Rotate(expectedTransformation.RotationMatrix)
            .Coordinates;
    }


    public override double GetValue(double x, double y, double z)
    {
        return GetValue(new Point3D(x, y, z));
    }

    public double GetValueAlternative(Point3D p)
    {
        Vector<double> a = p.Coordinates[0] * canonicalBaseXTransformed +
            p.Coordinates[1] * canonicalBaseYTransformed +
            p.Coordinates[2] * canonicalBaseZTransformed;

        a += transformation.TranslationVector;

        Point3D transformedPoint = new Point3D(a);

        if (referenceObject.PointWithinBounds(a[0], a[1], a[2]))
            return referenceObject.GetValue(transformedPoint);

        return OUT_OF_BOUNDS;
    }

    public override double GetValue(Point3D p)
    {
        Point3D currentPoint = p.ApplyRotationTranslation(transformation);
        //Debug.Log("Bod micro: " + p + " odpovida na macro: " + currentPoint);

        if (referenceObject.PointWithinBounds(currentPoint))
            return referenceObject.GetValue(currentPoint);

        return OUT_OF_BOUNDS;
    }

    public override double XSpacing => spacings[0];
    public override double YSpacing => spacings[1];
    public override double ZSpacing => spacings[2];

    public override int[] Measures => measures;
}

