namespace DataView
{
    interface ITransformer
    {
        Transform3D GetTransformation(Match m, IData d1, IData d2);
    }
}
