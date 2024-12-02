using DataView;

public class MockObject : AData
{
    private int xSize;
    private int ySize;
    private int zSize;

    public MockObject(int xSize, int ySize, int zSize)
    {
        this.xSize = xSize;
        this.ySize = ySize;
        this.zSize = zSize;
    }

    public override double MinValue => 0;

    public override double MaxValue => 1;

    public override double XSpacing => 1;

    public override double YSpacing => 1;

    public override double ZSpacing => 1;

    public override int[] Measures => new int[] { xSize, ySize, zSize };

    public override double GetPercentile(double value)
    {
        return 0;
    }

    public override double GetValue(double x, double y, double z)
    {
        return 0;
    }

    public override double GetValue(Point3D p)
    {
        return 0;
    }
}