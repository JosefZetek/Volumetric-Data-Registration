using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    interface IMatcher
    {
        Match[] Match(FeatureVector[] f1, FeatureVector[] f2, double threshold);
        Match[] Match(FeatureVector[] f1, FeatureVector[] f2);
    }
}
