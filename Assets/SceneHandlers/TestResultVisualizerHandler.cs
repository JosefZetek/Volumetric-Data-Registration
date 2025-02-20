using System.IO;
using UnityEngine;
using DataView;
using UnityEngine.UIElements;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;

/// <summary>
/// Lets user choose directory where these data are expected:
/// MacroData.mhd, MacroData.raw
/// Micro_n.mhd, Micro_n.raw, Micro_n.txt
/// </summary>
public class TestResultVisualizerHandler : MonoBehaviour
{
    BarChart barChart;

    // Start is called before the first frame update
    void RunTests()
    {
        IRegistrationLauncher registrationLauncher = new RegistrationLauncher();

        int order = 1;
        string directory = "/Users/pepazetek/Desktop/Tests/";
        string currentDirectory;

        while(Directory.Exists(directory + "TEST" + order))
        {
            currentDirectory = directory + "TEST" + order + "/";


            if (!FilesFound(currentDirectory, "macroData"))
            {
                order++;
                continue;
            }

            RunTestsWithinDirectory(
                currentDirectory,
                new VolumetricData(GetFilePathDescriptor("macroData", currentDirectory)),
                registrationLauncher
            );

            order++;
        }
    }
    private void RunTestsWithinDirectory(string directory, AData macroData, IRegistrationLauncher registrationLauncher)
    {
        Transform3D calculatedTransformation;
        Transform3D expectedTransformation;
        AData microData;

        int order = 1;
        string currentMicroFile;
        double currentTransformationDistance;


        while (true)
        {
            currentMicroFile = "microData" + (order++);

            if (!FilesFound(directory, currentMicroFile) || !TransformationFileFound(directory, currentMicroFile))
                break;

            microData = new VolumetricData(GetFilePathDescriptor(currentMicroFile, directory));
            expectedTransformation = TransformationIO.FetchTransformation(Path.Combine(directory, currentMicroFile + ".txt"));

            RegistrationLauncher.expectedTransformation = expectedTransformation;

            calculatedTransformation = registrationLauncher.RunRegistration(microData, macroData);

            currentTransformationDistance = GetTransformationDistance(
                microData,
                calculatedTransformation,
                expectedTransformation
            );

            this.barChart.AddColumn(currentTransformationDistance);
        }
    }

    private double GetTransformationDistanceNaive(AData microData, Transform3D calculatedTransformation, Transform3D expectedTransformation)
    {
        NaiveTransformationDistance naiveTransformationDistance = new NaiveTransformationDistance(microData);
        return naiveTransformationDistance.GetTransformationsDistance(calculatedTransformation, expectedTransformation);
    }

    private double GetTransformationDistance(AData microData, Transform3D calculatedTransformation, Transform3D expectedTransformation)
    {
        //Calculate predifined values
        ITransformationDistance transformationDistance = new TransformationDistanceSeven(microData);
        Transform3D.SetTransformationDistance(transformationDistance);

        return calculatedTransformation.DistanceTo(expectedTransformation);
    }

    private void Start()
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        RunTests();
        stopwatch.Stop();
        UnityEngine.Debug.Log($"Execution Time: {stopwatch.Elapsed.TotalSeconds} s");
    }

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;
        rootVisualElement.style.backgroundColor = Color.white;

        this.barChart = new BarChart(0);
        rootVisualElement.Add(barChart);

        barChart.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        barChart.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
    }

    private bool FilesFound(string directory, string fileName)
    {
        string mhdFile = Path.Combine(directory, fileName + ".mhd");
        string rawFile = Path.Combine(directory, fileName + ".raw");

        return File.Exists(mhdFile) && File.Exists(rawFile);
    }

    private bool TransformationFileFound(string directory, string fileName)
    {
        string transformationFile = Path.Combine(directory, fileName + ".txt");

        return File.Exists(transformationFile);
    }

    private FilePathDescriptor GetFilePathDescriptor(string fileName, string directory)
    {
        return new FilePathDescriptor(Path.Combine(directory, fileName + ".mhd"), Path.Combine(directory, fileName + ".raw"));
    }
}
