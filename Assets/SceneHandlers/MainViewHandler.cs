using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using DataView;
using MathNet.Numerics.LinearAlgebra;
using System;

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

    }

    private void Start()
    {
        /*

        TransformationIO.ExportTransformation("/Users/pepazetek/Desktop/transformation", new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.Dense(3)));

        /*
        CheckTransformation(
            new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMacro.mhd", "/Users/pepazetek/Desktop/45ZMacro.raw")),
            new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/rotated45ZMicro.mhd", "/Users/pepazetek/Desktop/rotated45ZMicro.raw")),
            transformation
        );
        */

        TransformationIO.FetchTransformation("/Users/pepazetek/Desktop/transformation.txt");

        /*
        CutViewerHandler.macroData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMacro.mhd", "/Users/pepazetek/Desktop/45ZMacro.raw"));
        CutViewerHandler.microData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMicro.mhd", "/Users/pepazetek/Desktop/45ZMicro.raw"));

        CutViewerHandler.transformation = expectedRotation;
        SceneManager.LoadScene("CutViewer");
        */

    }


}
