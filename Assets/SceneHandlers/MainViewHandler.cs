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
        //AMockObject macroObject = new EllipsoidMockData(180, 150, 120, new int[] { 200, 200, 200 }, new double[] { 1, 1, 1 });
        VolumetricData macroObject = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/Tests/HEAD/P01_HEAD_5_0_H31S_0004.mhd", "/Users/pepazetek/Desktop/Tests/HEAD/P01_HEAD_5_0_H31S_0004.raw"));
        //VolumetricData microObject = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/Tests/HEAD/micro_HEAD.mhd", "/Users/pepazetek/Desktop/Tests/HEAD/micro_HEAD.raw"));

        Transform3D transformation = new Transform3D(GetRotationMatrix(Math.PI / 8.0, 0, 0), GetTranslationVector(macroObject.MaxValueX / 2, macroObject.MaxValueY / 2, macroObject.MaxValueZ / 2));
        //Transform3D transformation2 = TransformationIO.FetchTransformation("/Users/pepazetek/Desktop/Tests/HEAD/micro_HEAD_transformation.txt");

        //UnityEngine.Debug.Log(transformation);
        //UnityEngine.Debug.Log(transformation2);
         CreatePair(macroObject, transformation);
        /*
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
        microObject.TransformObject(transformation);
        */

        /*
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "mockMacro_HEAD", macroObject);
        fileSaver.MakeFiles();
        stopwatch.Stop();
        UnityEngine.Debug.Log("Macro saving: " + stopwatch.ElapsedMilliseconds);

        stopwatch = new Stopwatch();
        stopwatch.Start();

        fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "mockMicro_HEAD", microObject);
        fileSaver.MakeFiles();
        stopwatch.Stop();
        UnityEngine.Debug.Log("Micro saving: " + stopwatch.ElapsedMilliseconds);
        */

        // TransformationIO.ExportTransformation("/Users/pepazetek/Desktop/mockMicro_HEAD_transformation", transformation);
        //Debug.Log(TransformationIO.FetchTransformation("/Users/pepazetek/Desktop/TestData/Micro_1.txt"));


        /*
        CheckTransformation(
            new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMacro.mhd", "/Users/pepazetek/Desktop/45ZMacro.raw")),
            new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/rotated45ZMicro.mhd", "/Users/pepazetek/Desktop/rotated45ZMicro.raw")),
            transformation
        );
        */


        //CutViewerHandler.SetDataSlicer(microObject, macroObject, transformation);
        //SceneManager.LoadScene("CutViewer");

    }



    

    private void CreatePair(AMockObject referenceObject, Transform3D transformation)
    {
        MockDataSegment mockDataSegment = new MockDataSegment(
            referenceObject,
            new int[] { referenceObject.Measures[0]/2, referenceObject.Measures[1]/2, referenceObject.Measures[2]/2 },
            new double[] { referenceObject.XSpacing, referenceObject.YSpacing, referenceObject.ZSpacing });

        mockDataSegment.TransformObject(transformation);

        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/Tests/HEAD/", "micro_HEAD", mockDataSegment);
        fileSaver.MakeFiles();

        TransformationIO.ExportTransformation("/Users/pepazetek/Desktop/Tests/HEAD/micro_HEAD_transformation", transformation);
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
