using DataView;

public interface IRegistrationLauncher
{
    public Transform3D RunRegistration(FilePathDescriptor microDataPath, FilePathDescriptor macroDataPath);
    public Transform3D RunRegistration(AData microData, AData macroData);
}

