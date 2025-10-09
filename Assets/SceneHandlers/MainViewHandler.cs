using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using DataView;
using System.Collections.Generic;
using System;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.WSA;
using MathNet.Numerics;

public class MainViewHandler : MonoBehaviour
{
    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        Button runRegistrationButton = rootVisualElement.Q<Button>("runRegistration");
        Button objectViewerButton = rootVisualElement.Q<Button>("objectViewer");
        Button slicerButton = rootVisualElement.Q<Button>("slicer");
        Button transformedSlicerButton = rootVisualElement.Q<Button>("transformedSlicer");

        runRegistrationButton.clicked += () => {
            SceneManager.LoadScene("RegistrationRunner");
        };

        objectViewerButton.clicked += () =>
        {
            SceneManager.LoadScene("ObjectViewer");
        };

        slicerButton.clicked += () =>
        {
            SceneManager.LoadScene("CutViewer");
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


    private void RegistrationTest()
    {
        List<string[]> distances = new List<string[]>();

        string[] folders = new string[] {
            "/Users/pepazetek/Desktop/Tests/Elipsoid/",
            "/Users/pepazetek/Desktop/Tests/Jatra/",
            "/Users/pepazetek/Desktop/Tests/Trup/"
            //"/Users/pepazetek/Desktop/Tests/MRI/"
        };

        foreach (string folder in folders)
        {
            FilePathDescriptor macroData = new FilePathDescriptor($"{folder}macroData.mhd", $"{folder}macroData.raw");
            for (int i = 1; i <= 5; i++)
            {
                FilePathDescriptor microData = new FilePathDescriptor($"{folder}microData{i}.mhd", $"{folder}microData{i}.raw");

                Transform3D expectedTransformation = TransformationIO.FetchTransformation($"{folder}transformation{i}.txt");

                TestCase testCase = new TestCase(microData, macroData, expectedTransformation);
                double distance = testCase.RunTest();
                distances.Add(new string[] { folder, i.ToString(), distance.ToString() });
                Debug.Log($"Folder: {folder}, Test case: {i}, Distance: {distance}");
            }
        }

        CSVWriter.WriteResult("/Users/pepazetek/Desktop/distances.csv", distances.ToArray());
    }

    private List<Transform3D> InitTransformations(int count)
    {
        List<Transform3D> transformations = new List<Transform3D>();

        for(int i = 0; i<count; i++)
        {
            transformations.Add(
                new Transform3D(
                    Generator.GetRotationMatrix(i / 10.0, i / 10.0, i / 10.0),
                    Generator.GetTranslationVector(0, 0, 0)
                )
            );
        }

        return transformations;
    }

    private List<Transform3D> InitRandomTransformations(int count)
    {
        List<Transform3D> transformations = new List<Transform3D>();
        for (int i = 0; i < count; i++)
            transformations.Add(Generator.GetRandomTransformation());

        return transformations;
    }

    private void Start()
    {
        RegistrationTest();
        //SphereMockData sphereMockData = new SphereMockData();

        //FileSaver fileSaver = new FileSaver("/Users/pepazetek/", "sphere", sphereMockData);
        //fileSaver.MakeFiles();
    }

    private void CreatePair(AMockObject referenceObject, Transform3D transformation)
    {
        MockDataSegment mockDataSegment = new MockDataSegment(
            referenceObject,
            new int[] { referenceObject.Measures[0], referenceObject.Measures[1], referenceObject.Measures[2] },
            new double[] { referenceObject.XSpacing, referenceObject.YSpacing, referenceObject.ZSpacing });

        mockDataSegment.TransformObject(transformation);

        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/Tests/TEST3/", "micro", mockDataSegment);
        fileSaver.MakeFiles();

        TransformationIO.ExportTransformation("/Users/pepazetek/Desktop/Tests/TEST3/micro_transformation", transformation);
    }

}
