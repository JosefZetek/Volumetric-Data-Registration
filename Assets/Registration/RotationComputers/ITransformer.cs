namespace DataView
{
    public interface ITransformer
    {
        Transform3D GetTransformation(Match m, AData d1, AData d2);
    }
}
