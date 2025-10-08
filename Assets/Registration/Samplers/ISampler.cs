namespace DataView
{
    public interface ISampler
    {
       Point3D[] Sample(AData d, int count);
    }
}
