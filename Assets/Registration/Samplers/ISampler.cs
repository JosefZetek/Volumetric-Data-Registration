namespace DataView
{
    interface ISampler
    {
       Point3D[] Sample(IData d, int count);
    }
}
