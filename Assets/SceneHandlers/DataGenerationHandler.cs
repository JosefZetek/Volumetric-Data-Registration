using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using UnityEditor;
using DataView;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Threading.Tasks;
using System.Diagnostics;

public class DataGenerationHandler : MonoBehaviour
{
    private const int DEFAULT_PAIRS_NUMBER = 1;

    private System.Random random;
    private int pairsNumber;

    //private PieChart pieChart;


    public DataGenerationHandler()
    {
        this.random = new System.Random();
        this.pairsNumber = DEFAULT_PAIRS_NUMBER;
    }

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        TextField pairsTextField = rootVisualElement.Q<TextField>("pairsTextField");

        pairsTextField.RegisterValueChangedCallback(evt =>
        {
            var forbiddenPattern = "[^\\d]";

            if (Regex.IsMatch(evt.newValue, forbiddenPattern))
                pairsTextField.value = evt.previousValue;

            try { pairsNumber = int.Parse(pairsTextField.value); }
            catch { pairsNumber = DEFAULT_PAIRS_NUMBER; }
        });

        pairsTextField.RegisterCallback<FocusOutEvent>(evt =>
        {
            if (pairsTextField.value.Length > 0)
                return;

            pairsTextField.value = DEFAULT_PAIRS_NUMBER.ToString();
            pairsNumber = DEFAULT_PAIRS_NUMBER;
        });

        rootVisualElement.Q<Button>("generatePairsButton").clicked += () => GeneratePairs();

        /*
        pieChart = new PieChart();
        pieChart.style.width = new StyleLength(Length.Percent(100)); // Set width to 100%
        pieChart.style.height = new StyleLength(Length.Percent(100)); // Set width to 100%
        rootVisualElement.Q<VisualElement>("loadingGraph").Add(pieChart);
        */
    }

    private string GetDirectory()
    {
        string directory = DataFileDialog.GetDirectory();
        if (directory == null)
            return string.Empty;

        if (!EditorUtility.DisplayDialog("File Replacement Warning",
            string.Format("Files having name in format\n<fileMacro/fileMicro>_<number>.<mhd/raw/txt>\nwhere <number> = 1 - {0}\nwill get replaced.", 1 + (pairsNumber - 1)),
            "Export anyway", "Cancel"))
            return string.Empty;

        return directory;
    }

    private void GeneratePairs()
    {
        string directory = GetDirectory();

        if (string.IsNullOrEmpty(directory))
            return;
        
        int[] measures = new int[] { 200, 200, 100 };
        double[] spacings = new double[] { 1, 1, 1 };

        EllipsoidMockData ellipsoid = new EllipsoidMockData(150, 180, 5, measures, spacings);

        FileSaver fileSaver = new FileSaver(directory, "MacroData", ellipsoid);
        fileSaver.MakeFiles();

            for (int i = 0; i < pairsNumber; i++)
                GenerateMicroData(ellipsoid, directory, i);
    }

    private void GenerateMicroData(AMockObject sourceObject, string directory, int order)
    {
        MockDataSegment mockObject = new MockDataSegment(
            sourceObject,
            new int[] { 100, 100, 100 },
            new double[] { sourceObject.XSpacing, sourceObject.YSpacing, sourceObject.ZSpacing }
        );

        Transform3D expectedTransformation = GetRandomTransformation(mockObject);
        FileSaver fileSaver = new FileSaver(directory, "Micro_" + (order + 1), mockObject);
        mockObject.TransformObject(expectedTransformation);

        fileSaver.MakeFiles();

        TransformationIO.ExportTransformation(directory + "Micro_" + (order + 1), expectedTransformation);
    }

    private Transform3D GetRandomTransformation(AMockObject mockObject)
    {
        Vector<double> translationVector = Vector<double>.Build.DenseOfArray(new double[] {
            this.random.NextDouble() * mockObject.MaxValueX,
            this.random.NextDouble() * mockObject.MaxValueY,
            this.random.NextDouble() * mockObject.MaxValueZ
        });

        Matrix<double> rotationMatrix = GetRotationMatrix(
            this.random.NextDouble() * 2 * Math.PI,
            this.random.NextDouble() * 2 * Math.PI,
            this.random.NextDouble() * 2 * Math.PI
        );

        return new Transform3D(rotationMatrix, translationVector);
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
