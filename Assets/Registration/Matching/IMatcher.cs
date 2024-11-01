using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    public interface IMatcher
    {
        Match[] Match(FeatureVector[] featureVectorsMicro, FeatureVector[] featureVectorsMacro, double threshold);
        Match[] Match(FeatureVector[] featureVectorsMicro, FeatureVector[] featureVectorsMacro);
    }
}
