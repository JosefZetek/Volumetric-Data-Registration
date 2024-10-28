using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DataView;
using UnityEngine.UIElements;

/// <summary>
/// Lets user choose directory where these data are expected:
/// MacroData.mhd, MacroData.raw
/// Micro_n.mhd, Micro_n.raw, Micro_n.txt
/// </summary>
public class TestResultVisualizerHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void RunTests()
    {
        /*
        string directory = DataFileDialog.GetDirectory();

        if (string.IsNullOrEmpty(directory))
        {
            Debug.Log("Directory not entered");
            SceneManager.LoadScene("MainView");
            return;
        }


        VolumetricData macroData = GetMacroFilePathDescriptor(directory);
        if(macroData == null)
        {
            Debug.Log("Macro files not found.");
            SceneManager.LoadScene("MainView");
            return;
        }


        List<TestDataUnit> testFiles = FindTestFiles(directory);

        if (testFiles.Count == 0)
        {
            Debug.Log("Files are not located inside the folder");
            SceneManager.LoadScene("MainView");
            return;
        }

        NaiveTransformationDistance naiveTransformationDistance;
        RegistrationLauncher registrationLauncher = new RegistrationLauncher();
        Transform3D calculatedTransformation;

        VolumetricData microData;

        for (int i = 0; i < testFiles.Count; i++)
        {
            microData = new VolumetricData(testFiles[i].GetFilePathDescriptor());

            calculatedTransformation = registrationLauncher.RunRegistration(microData, macroData);
            calculatedTransformation = registrationLauncher.RevertCenteringTransformation(calculatedTransformation);


            naiveTransformationDistance = new NaiveTransformationDistance(microData);
            Debug.Log("Distance T1: " + naiveTransformationDistance.GetSqrtTransformationDistance(calculatedTransformation, testFiles[i].GetTransformation()));
        }
        */
    }

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        rootVisualElement.style.backgroundColor = Color.white;
        var barChart = new BarChart(0);
        barChart.AddColumn(10);
        barChart.AddColumn(20);
        barChart.AddColumn(15);

        rootVisualElement.Add(barChart);
        barChart.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        barChart.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

    }


    private VolumetricData GetMacroFilePathDescriptor(string directory)
    {
        string macroDataName = "MacroData";

        if (!FilesFound(directory, macroDataName))
            return null;

        return new VolumetricData(
            new FilePathDescriptor(
                directory + macroDataName + ".mhd",
                directory + macroDataName + ".raw"
            )
        );
    }


private List<TestDataUnit> FindTestFiles(string directory)
    {
        List<TestDataUnit> fetchedFiles = new List<TestDataUnit>();
        int order = 1;

        bool fetchingFiles = true;

        string currentMicroFile;
        while(fetchingFiles)
        {
            currentMicroFile = "Micro_" + (order++);
            if (!FilesFound(directory, currentMicroFile) || !TransformationFileFound(directory, currentMicroFile))
            {
                fetchingFiles = false;
                continue;
            }

            fetchedFiles.Add(new TestDataUnit(currentMicroFile, directory));
        }

        return fetchedFiles;
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

    /// <summary>
    /// Class providing path to file as well as its transformation
    /// </summary>
    private class TestDataUnit
    {
        private FilePathDescriptor filePathDescriptor;
        private Transform3D transformation;

        public TestDataUnit(string fileName, string directory)
        {
            this.filePathDescriptor = new FilePathDescriptor(Path.Combine(directory, fileName + ".mhd"), Path.Combine(directory, fileName + ".raw"));
            this.transformation = TransformationIO.FetchTransformation(Path.Combine((directory), fileName + ".txt"));
        }

        public FilePathDescriptor GetFilePathDescriptor()
        {
            return this.filePathDescriptor;
        }

        public Transform3D GetTransformation()
        {
            return this.transformation;
        }
    }
}
