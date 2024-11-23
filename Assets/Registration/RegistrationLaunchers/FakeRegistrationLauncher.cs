using DataView;

/// <summary>
/// 
/// </summary>
public class FakeRegistrationLauncher : IRegistrationLauncher
{
    public Transform3D RunRegistration(FilePathDescriptor microDataPath, FilePathDescriptor macroDataPath)
    {
        return new Transform3D();
    }

    public Transform3D RunRegistration(AData microData, AData macroData)
    {
        return new Transform3D();
    }
}

