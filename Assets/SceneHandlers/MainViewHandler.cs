using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using DataView;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Diagnostics;

public class MainViewHandler : MonoBehaviour
{
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        Button runRegistrationButton = rootVisualElement.Q<Button>("runRegistration");
        Button createObjectButton = rootVisualElement.Q<Button>("createObject");
        Button objectViewerButton = rootVisualElement.Q<Button>("objectViewer");
        Button slicerButton = rootVisualElement.Q<Button>("slicer");
        Button testDataGeneratorButton = rootVisualElement.Q<Button>("testDataGenerator");
        Button testButton = rootVisualElement.Q<Button>("test");
        Button transformedSlicerButton = rootVisualElement.Q<Button>("transformedSlicer");

        runRegistrationButton.clicked += () => {
            SceneManager.LoadScene("RegistrationRunner");
        };

        createObjectButton.clicked += () =>
        {
            SceneManager.LoadScene("ModelCreator");
        };

        objectViewerButton.clicked += () =>
        {
            SceneManager.LoadScene("ObjectViewer");
        };

        slicerButton.clicked += () =>
        {
            SceneManager.LoadScene("CutViewer");
        };

        testDataGeneratorButton.clicked += () =>
        {
            SceneManager.LoadScene("DataGeneration");
        };

        testButton.clicked += () =>
        {
            SceneManager.LoadScene("TestResultVisualizer");
        };

        transformedSlicerButton.clicked += () =>
        {
            var macroDataPath = DataFileDialog.GetFilePath("macro");
            var microDataPath = DataFileDialog.GetFilePath("micro");
            Transform3D transformation = TransformationIO.FetchTransformation(DataFileDialog.GetFile("txt"));
            

            VolumetricData macroData = new VolumetricData(macroDataPath);
            VolumetricData microData = new VolumetricData(microDataPath);

            CutViewerHandler.SetDataSlicer(microData, macroData, transformation);
            SceneManager.LoadScene("CutViewer");
        };
    }

    private void Start()
    {
        AMockObject macroObject = new EllipsoidMockData(180, 150, 120, new int[] { 200, 200, 200 }, new double[] { 1, 1, 1 });

        int[] measures = new int[]
        {
            macroObject.Measures[0] / 2,
            macroObject.Measures[1] / 2,
            macroObject.Measures[2] / 2,
        };

        double[] spacing = new double[]
        {
            1,1,1
        };
        MockDataSegment microObject = new MockDataSegment(macroObject, measures, spacing);
        microObject.TransformObject(new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.Dense(3)));

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "mockMacro", macroObject);
        fileSaver.MakeFiles();
        stopwatch.Stop();
        UnityEngine.Debug.Log("Macro saving: " + stopwatch.ElapsedMilliseconds);

        stopwatch = new Stopwatch();
        stopwatch.Start();

        fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "mockMicro", microObject);
        fileSaver.MakeFiles();
        stopwatch.Stop();
        UnityEngine.Debug.Log("Micro saving: " + stopwatch.ElapsedMilliseconds);


        //TransformationIO.ExportTransformation("/Users/pepazetek/Desktop/transformation", new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.Dense(3)));
        //Debug.Log(TransformationIO.FetchTransformation("/Users/pepazetek/Desktop/TestData/Micro_1.txt"));
        /*


        /*
        CheckTransformation(
            new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMacro.mhd", "/Users/pepazetek/Desktop/45ZMacro.raw")),
            new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/rotated45ZMicro.mhd", "/Users/pepazetek/Desktop/rotated45ZMicro.raw")),
            transformation
        );
        */


        /*
        CutViewerHandler.macroData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMacro.mhd", "/Users/pepazetek/Desktop/45ZMacro.raw"));
        CutViewerHandler.microData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMicro.mhd", "/Users/pepazetek/Desktop/45ZMicro.raw"));

        CutViewerHandler.transformation = expectedRotation;
        SceneManager.LoadScene("CutViewer");
        */

    }


}
