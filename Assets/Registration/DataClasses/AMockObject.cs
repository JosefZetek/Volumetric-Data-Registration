using System;
using DataView;

public abstract class AMockObject
{
    public abstract double XSpacing { get; }
    public abstract double YSpacing { get; }
    public abstract double ZSpacing { get; }

    public abstract int[] Measures { get; }

    public abstract double GetValue(double x, double y, double z);
    public abstract double GetValue(Point3D p);

    public bool PointWithinBounds(Point3D p)
    {
        return (p.X < Measures[0] && p.Y < Measures[1] && p.Z < Measures[2] && p.X >= 0 && p.Y >= 0 && p.Z >= 0);
    }
}

