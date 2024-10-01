using System;

namespace DataView
{
	interface ITransformationDistance
	{
        double GetTransformationsDistance(Transform3D transformation1, Transform3D transformation2);
        double GetSqrtTransformationDistance(Transform3D transformation1, Transform3D transformation2);
    }
}

