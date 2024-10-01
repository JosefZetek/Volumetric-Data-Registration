using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    interface IFeatureComputer
    {
        /// <summary>
        /// Calculates a set of features, that are as specific to a given point as possible.
        /// These are later used for matching point pairs.
        /// </summary>
        /// <param name="d">Data class for a given point</param>
        /// <param name="p">Point</param>
        /// <returns>Returns FeatureVector consisting of characteristic values as well as the point used for their calculation.</returns>
        FeatureVector ComputeFeatureVector(IData d, Point3D p);
    }
}
