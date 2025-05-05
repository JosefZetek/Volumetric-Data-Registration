using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using DataView;
using System.Collections.Generic;
using System;
using MathNet.Numerics.LinearAlgebra;

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
        //AData microData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/Tests/TEST3/microData1.mhd", "/Users/pepazetek/Desktop/Tests/TEST3/microData1.raw"));

        //Transform3D transformation = TransformationIO.FetchTransformation("/Users/pepazetek/Desktop/Tests/TEST3/microData5.txt");
        Transform3D transformation2 = TransformationIO.FetchTransformation("/Users/pepazetek/Downloads/elipsoid_5.txt");
        //TransformationIO.ExportTransformation("/Users/pepazetek/Downloads/export.txt", transformation2);

        //Transform3D.SetTransformationDistance(new TransformationDistanceSeven(microData));
        //RegistrationLauncher.expectedTransformation = transformation;
        //RegistrationLauncher registrationLauncher = new RegistrationLauncher();
        //Transform3D resultTransformation = registrationLauncher.RunRegistration(microData, macroData);

        //Debug.Log($"Distance: {transformation.SqrtDistanceTo(transformation2)}");
        //CutViewerHandler.SetDataSlicer(microData, macroData, resultTransformation);
        //SceneManager.LoadScene("CutViewer");

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
