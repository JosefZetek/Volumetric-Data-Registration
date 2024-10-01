using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    class PointWithFeatures : Point3D //TODO - Matěj - zeptat se
    {
        public double[] featureVector;

        public PointWithFeatures(double x, double y , double z, double[] featureVector) : base(x, y, z)
        {
            this.featureVector = featureVector;
        }

        public PointWithFeatures(Point3D point, double[] featureVector) : base(point.X, point.Y, point.Z)
        {
            this.featureVector = featureVector;
        }
    }
}
