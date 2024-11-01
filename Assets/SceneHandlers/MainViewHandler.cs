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
        VolumetricData macroObject = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/Tests/TEST7/macroData.mhd", "/Users/pepazetek/Desktop/Tests/TEST7/macroData.raw"));
        Transform3D expectedTransformation = new Transform3D(GetRotationMatrix(0, 0, Math.PI / 5.0), GetTranslationVector(2, 0.1, 0));

        MockDataSegment mockDataSegment = new MockDataSegment(macroObject, macroObject.Measures, new double[] { macroObject.XSpacing, macroObject.YSpacing, macroObject.ZSpacing });
        mockDataSegment.TransformObject(expectedTransformation);

        TransformedFileSaver transformedFileSaver = new TransformedFileSaver(
            "/Users/pepazetek/Desktop/Tests/",
            "microData",
            macroObject,
            expectedTransformation,
            macroObject.Measures,
            new double[] { macroObject.XSpacing, macroObject.YSpacing, macroObject.ZSpacing }
        );

        transformedFileSaver.MakeFiles();
        TransformationIO.ExportTransformation("/Users/pepazetek/Desktop/Tests/TEST7/transformation", expectedTransformation);

        
        //RegistrationLauncher registrationLauncher = new RegistrationLauncher(expectedTransformation);
        //Transform3D resultTransformation = registrationLauncher.RunRegistration(microObject, macroObject);

    }



    

    private void CreatePair(AMockObject referenceObject, Transform3D transformation)
    {
        MockDataSegment mockDataSegment = new MockDataSegment(
            referenceObject,
            new int[] { referenceObject.Measures[0]/2, referenceObject.Measures[1]/2, referenceObject.Measures[2]/2 },
            new double[] { referenceObject.XSpacing, referenceObject.YSpacing, referenceObject.ZSpacing });

        mockDataSegment.TransformObject(transformation);

        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/Tests/TEST3/", "micro", mockDataSegment);
        fileSaver.MakeFiles();

        TransformationIO.ExportTransformation("/Users/pepazetek/Desktop/Tests/TEST3/micro_transformation", transformation);
    }

    private Vector<double> GetTranslationVector(double x, double y, double z)
    {
        return Vector<double>.Build.DenseOfArray(new double[] { x, y, z });
    }

    private Matrix<double> GetRotationMatrix(double angleX, double angleY, double angleZ)
    {
        Matrix<double> rotationX = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            {1, 0,  0 },
            {0,  Math.Cos(angleX), -Math.Sin(angleX) },
            { 0, Math.Sin(angleX), Math.Cos(angleX) }
        });

        Matrix<double> rotationY = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { Math.Cos(angleY), 0, Math.Sin(angleY) },
            { 0, 1, 0 },
            { -Math.Sin(angleY), 0, Math.Cos(angleY) }
        });

        Matrix<double> rotationZ = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            {Math.Cos(angleZ), -Math.Sin(angleZ), 0 },
            {Math.Sin(angleZ), Math.Cos(angleZ), 0 },
            { 0, 0, 1 }
        });

        return rotationX * rotationY * rotationZ;
    }


}
