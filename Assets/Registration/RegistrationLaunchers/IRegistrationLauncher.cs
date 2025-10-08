using DataView;

public interface IRegistrationLauncher
{
    Transform3D RunRegistration(FilePathDescriptor microDataPath, FilePathDescriptor macroDataPath);
    Transform3D RunRegistration(AData microData, AData macroData);
}

