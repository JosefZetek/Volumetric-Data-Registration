using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    interface IData
    {
        double XSpacing { get; set; }
        double YSpacing { get; set; }
        double ZSpacing { get; set; }
        int[] Measures { get; set; }
        double MinValue { get; }
        double MaxValue { get; }
        double GetValue(double x, double y, double z);
        double[,] Cut(double[] point, double[] v1, double[] v2, int xRes, int yRes, double spacing);
        double GetValue(Point3D p);
        double GetValueDistribution(double value);
    }
}
