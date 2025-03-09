using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using DataView;
using System.Collections.Generic;

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


    private void RegistrationTest()
    {
        //AData macroData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/Tests/TEST2/macroData.mhd", "/Users/pepazetek/Desktop/Tests/TEST2/macroData.raw"));
        //AData microData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/Tests/TEST2/microData5.mhd", "/Users/pepazetek/Desktop/Tests/TEST2/microData5.raw"));

        AData data = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/sphere.mhd", "/Users/pepazetek/sphere.raw"));
        IFeatureComputer fc = new FeatureComputerISOSurfaceCurvature();
        Point3D point = new Point3D(0, 0, 2.4);
        Point3D centerPoint = new Point3D(2.5, 2.5, 2.5);

        Debug.Log($"Distance to center: {centerPoint.Distance(point)}");

        Debug.Log($"Value: {data.GetValue(point)}");
        Debug.Log($"FC: {fc.ComputeFeatureVector(data, point)}");

        //:4.34 y: 4.54 z: 2.4 and current value: 1


        CutViewerHandler.SetFCSlicer(data);

        //Transform3D transformation = TransformationIO.FetchTransformation("/Users/pepazetek/Desktop/Tests/TEST2/microData5.txt");
        //Transform3D transformation = new Transform3D();

        //Debug.Log($"Loaded transformation: {transformation}");

        //RegistrationLauncher.expectedTransformation = transformation;
        //RegistrationLauncher registrationLauncher = new RegistrationLauncher();
        //Debug.Log($"Value: {registrationLauncher.GetVariation(microData, macroData)}");

        //registrationLauncher.Krivost(data);
        //registrationLauncher.ZK(data, data);


        //Transform3D resultTransformation = registrationLauncher.RunRegistration(microData, macroData);

        //Debug.Log($"Result transformation {resultTransformation}");
        ////Debug.Log($"Distance: {transformation.RelativeDistanceTo(resultTransformation)}");

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
            new int[] { referenceObject.Measures[0]/2, referenceObject.Measures[1]/2, referenceObject.Measures[2]/2 },
            new double[] { referenceObject.XSpacing, referenceObject.YSpacing, referenceObject.ZSpacing });

        mockDataSegment.TransformObject(transformation);

        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/Tests/TEST3/", "micro", mockDataSegment);
        fileSaver.MakeFiles();

        TransformationIO.ExportTransformation("/Users/pepazetek/Desktop/Tests/TEST3/micro_transformation", transformation);
    }

}
