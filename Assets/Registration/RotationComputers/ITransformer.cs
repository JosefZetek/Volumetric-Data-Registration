using System.Collections.Generic;

namespace DataView
{
    public interface ITransformer
    {
        Transform3D GetTransformation(Match m, AData d1, AData d2);
        void AppendTransformation(Match m, AData dataMicro, AData dataMacro, ref List<Transform3D> transformations);
    }
}
