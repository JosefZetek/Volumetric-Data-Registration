using UnityEngine;
using DataView;

public interface IDataSlicer
{
    /// <summary>
    /// Cuts image and outputs 2D array with data organized as follows
    /// [row][column], [0][0] represents bottom left corner.
    /// Data thats been used in constructor are used as a refference (black tint).
    /// Where microData overlaps with refference data, it replaces the value with green tint.
    /// </summary>
    /// <param name="t">Value between 0-1</param>
    /// <param name="axis">Number of axis [0 = x, 1 = y, 2 = z]</param>
    /// <param name="transformation">Transformation that aligns microData onto macroData</param>
    /// <returns>Returns array of columns of a cut</returns>
    public Color[][] Cut(double t, int axis, CutResolution resolution);
}

