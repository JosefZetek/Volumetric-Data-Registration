using UnityEngine;
using DataView;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Profiling;

public class ModelCreatorHandler : MonoBehaviour
{
    private VisualElement rootVisualElement;
    private DropdownField dropdown;

    private Transform3D transformation;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        rootVisualElement = uiDocument.rootVisualElement;
        

        rootVisualElement.Q<Button>("backButton").clicked += () => SceneManager.LoadScene("MainView");
        rootVisualElement.Q<Button>("backButton").focusable = false;

        rootVisualElement.Q<Button>("generatePairButton").clicked += () => GeneratePair();

        this.dropdown = rootVisualElement.Q<DropdownField>("modelSelection");
    }

    private void ShowSlicerView()
    {
        VolumetricData microData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/mockMicro.mhd", "/Users/pepazetek/Desktop/mockMicro.raw"));
        VolumetricData macroData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/mockMacro.mhd", "/Users/pepazetek/Desktop/mockMacro.raw"));

        CutViewerHandler.SetDataSlicer(microData, macroData, transformation);
        SceneManager.LoadScene("CutViewer");
    }

    private AMockObject InitMacro()
    {
        int sizeX = int.Parse(rootVisualElement.Q<TextField>("macroSizeX").text);
        int sizeY = int.Parse(rootVisualElement.Q<TextField>("macroSizeY").text);
        int sizeZ = int.Parse(rootVisualElement.Q<TextField>("macroSizeZ").text);

        double spacingX = double.Parse(rootVisualElement.Q<TextField>("macroSpacingX").text);
        double spacingY = double.Parse(rootVisualElement.Q<TextField>("macroSpacingY").text);
        double spacingZ = double.Parse(rootVisualElement.Q<TextField>("macroSpacingZ").text);

        Debug.Log("Size: " + sizeX + ";" + sizeY + ";" + sizeZ);
        Debug.Log("Spacing: " + spacingX + ";" + spacingY + ";" + spacingZ);

        if (dropdown.index == 0)
            return new EllipsoidMockData((int)((sizeX-1)*spacingX), (int)((sizeY-1)*spacingY), (int)((sizeZ-1)*spacingZ), new int[] { sizeX, sizeY, sizeZ }, new double[] { spacingX, spacingY, spacingZ });
        else if (dropdown.index == 1)
            return new PointDistanceMock(new int[] { sizeX, sizeY, sizeZ }, new double[] { spacingX, spacingY, spacingZ });

        throw new ArgumentException("Dropdown index not implemented");
    }


    private void GeneratePair()
    {
        AMockObject macroObject = null;
        AMockObject microObject = null;
        try
        {
            //macroObject = InitMacro();
            Profiler.BeginSample("Load data");
            //macroObject = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Documents/CT/Zkouska/P01_c_DICOM-8bit-lowres_130926_liver-29-8-12-C_1x.mhd", "/Users/pepazetek/Documents/CT/Zkouska/P01_c_DICOM-8bit-lowres_130926_liver-29-8-12-C_1x.raw"));
            macroObject = new EllipsoidMockData(180, 150, 120, new int[] { 200, 200, 200 }, new double[] { 1, 1, 1 });
            Profiler.EndSample();
        }
        catch
        {
            Debug.Log("Wrong format of parameters for macro data.");
            return;
        }

        try
        {
            microObject = InitMicro(macroObject);
        }
        catch
        {
            Debug.Log("Wrong format of parameters for micro data.");
            return;
        }
        
        //SmallMockObject smallMockObject = new SmallMockObject();

        Profiler.BeginSample("Saving data");
        
        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "mockMacro", macroObject);
        fileSaver.MakeFiles();

        fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "mockMicro", microObject);
        fileSaver.MakeFiles();

        Profiler.EndSample();

        TransformationIO.ExportTransformation("/users/pepazetek/Desktop/mockTransformation.txt", transformation);

        Debug.Log("Saved");

        ShowSlicerView();
    }

    private AMockObject InitMicro(AMockObject macroData)
    {
        Vector<double> translationVector = Vector<double>.Build.Dense(3);
        Matrix<double> rotationMatrix = Matrix<double>.Build.DenseIdentity(3);
        int[] Measures = new int[3];
        double [] Spacings = new double[3];

        try
        {
            int sizeX = macroData.Measures[0], sizeY = macroData.Measures[1], sizeZ = macroData.Measures[2];
            double spacingX = macroData.XSpacing, spacingY = macroData.YSpacing, spacingZ = macroData.ZSpacing;
            

            /*
            int sizeX = int.Parse(rootVisualElement.Q<TextField>("microSizeX").text);
            int sizeY = int.Parse(rootVisualElement.Q<TextField>("microSizeY").text);
            int sizeZ = int.Parse(rootVisualElement.Q<TextField>("microSizeZ").text);

            double spacingX = double.Parse(rootVisualElement.Q<TextField>("microSpacingX").text);
            double spacingY = double.Parse(rootVisualElement.Q<TextField>("microSpacingY").text);
            double spacingZ = double.Parse(rootVisualElement.Q<TextField>("microSpacingZ").text);
            */

            double translationX = double.Parse(rootVisualElement.Q<TextField>("microTranslationX").text);
            double translationY = double.Parse(rootVisualElement.Q<TextField>("microTranslationY").text);
            double translationZ = double.Parse(rootVisualElement.Q<TextField>("microTranslationZ").text);

            double rotationX = double.Parse(rootVisualElement.Q<TextField>("microRotationX").text);
            double rotationY = double.Parse(rootVisualElement.Q<TextField>("microRotationY").text);
            double rotationZ = double.Parse(rootVisualElement.Q<TextField>("microRotationZ").text);

            translationVector = Vector<double>.Build.DenseOfArray(new double[] { translationX, translationY, translationZ });
            rotationMatrix = GenerateRotationMatrix(rotationX, rotationY, rotationZ);

            Measures = new int[] { sizeX/2, sizeY/2, sizeZ/2 };
            Spacings = new double[] { spacingX, spacingY, spacingZ };
        }
        catch
        {
            Debug.Log("Wrong format of parameters for micro data.");
        }

        this.transformation = new Transform3D(rotationMatrix, translationVector);

        MockDataSegment microData = new MockDataSegment(macroData, Measures, Spacings);
        microData.TransformObject(transformation);

        return microData;
    }

    /// <summary>
    /// Generates random rotation matrix
    /// </summary>
    /// <param name="rotationX">Rotation angle X</param>
    /// <param name="rotationY">Rotation angle Y</param>
    /// <param name="rotationZ">Rotation angle Z</param>
    /// <returns>Returns rotation matrix</returns>
    public Matrix<double> GenerateRotationMatrix(double rotationX, double rotationY, double rotationZ)
    {
        Matrix<double> rotationMatrixX = Matrix<double>.Build.DenseOfArray(new double[,]
        {
                { 1, 0, 0 },
                { 0, Math.Cos(rotationX), -Math.Sin(rotationX) },
                { 0, Math.Sin(rotationX), Math.Cos(rotationX) }
        });

        Matrix<double> rotationMatrixY = Matrix<double>.Build.DenseOfArray(new double[,]
        {
                {Math.Cos(rotationY), 0, Math.Sin(rotationY) },
                { 0, 1, 0 },
                { -Math.Sin(rotationY), 0, Math.Cos(rotationY) }
        });

        Matrix<double> rotationMatrixZ = Matrix<double>.Build.DenseOfArray(new double[,]
        {
                { Math.Cos(rotationZ), -Math.Sin(rotationZ), 0 },
                { Math.Sin(rotationZ), Math.Cos(rotationZ), 0 },
                { 0, 0, 1 }
        });

        return rotationMatrixX * rotationMatrixY * rotationMatrixZ;
    }
}

