using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;
using DataView;

public class TestCase
{

    private static IRegistrationLauncher registrationLauncher = new RegistrationLauncher();
    private FilePathDescriptor microData;
    private FilePathDescriptor macroData;

    private Transform3D expectedTransformation;

    public TestCase(FilePathDescriptor microData, FilePathDescriptor macroData, Transform3D expectedTransformation)
    {
        this.microData = microData;
        this.macroData = macroData;
        this.expectedTransformation = expectedTransformation;
    }
    
    public double RunTest()
    {
        AData micro = new VolumetricData(microData);
        AData macro = new VolumetricData(macroData);

        Transform3D.SetTransformationDistance(new TransformationDistanceSeven(micro));
        RegistrationLauncher.expectedTransformation = expectedTransformation;

        Transform3D output = registrationLauncher.RunRegistration(micro, macro);

        return expectedTransformation.DistanceTo(output);
    }
}

