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

    }

    private void Start()
    {

        int[] measures = new int[] { 200, 200, 100 };
        double[] spacings = new double[] { 1, 1, 1 };

        Vector<double> translationVector = GetTranslationVector(50, 50, 0);
        Matrix<double> rotationMatrix = GetRotationMatrix(0, 0,Math.PI/4);
        Transform3D expectedRotation = new Transform3D(rotationMatrix, translationVector);


        EllipsoidMockData ellipsoid = new EllipsoidMockData(150, 180, 5, measures, spacings);
        MockDataSegment mockDataSegment = new MockDataSegment(
            ellipsoid,
            new int[] {100, 100, 100},
            new double[] { ellipsoid.XSpacing, ellipsoid.YSpacing, ellipsoid.ZSpacing }
        );

        mockDataSegment.TransformObject(expectedRotation);

        /*
        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "45ZMacro", ellipsoid);
        fileSaver.MakeFiles();

        fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "45ZMicro", mockDataSegment);
        fileSaver.MakeFiles();
        */

        /*
        CheckTransformation(
            new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMacro.mhd", "/Users/pepazetek/Desktop/45ZMacro.raw")),
            new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/rotated45ZMicro.mhd", "/Users/pepazetek/Desktop/rotated45ZMicro.raw")),
            transformation
        );

        */

        CutViewerHandler.macroData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMacro.mhd", "/Users/pepazetek/Desktop/45ZMacro.raw"));
        CutViewerHandler.microData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/45ZMicro.mhd", "/Users/pepazetek/Desktop/45ZMicro.raw"));

        CutViewerHandler.transformation = expectedRotation;
        SceneManager.LoadScene("CutViewer");
        
    }

    private bool CheckTransformation(AData macroDataVolumetric, AData microDataVolumetric, Transform3D transformation)
    {

        Point3D microPoint;
        Point3D macroPoint;
        double microValue = 0, macroValue = 0;
        for(double x = 0; x<= microDataVolumetric.MaxValueX; x += microDataVolumetric.XSpacing)
        {
            for (double y = 0; y <= microDataVolumetric.MaxValueY; y += microDataVolumetric.YSpacing)
            {
                for (double z = 0; z <= microDataVolumetric.MaxValueZ; z += microDataVolumetric.ZSpacing)
                {
                    microPoint = new Point3D(x, y, z);
                    macroPoint = microPoint.ApplyRotationTranslation(transformation);

                    microValue = microDataVolumetric.GetValue(microPoint);

                    if(!macroDataVolumetric.PointWithinBounds(macroPoint))
                    {
                        if(microValue != 10000)
                        {
                            Debug.Log("Nevychazi mimo hranice: " + microValue);
                            return false;
                        }
                        continue;
                    }

                    macroValue = macroDataVolumetric.GetValue(macroPoint);

                    if (microValue != macroValue)
                    {
                        Debug.Log("Nevychazi protoze tohle: " + microValue + "x" + macroValue);
                        Debug.Log("Souradnice micro: " + microPoint);
                        Debug.Log("Souradnice macro: " + macroPoint);

                        Debug.Log("Value: " + macroDataVolumetric.GetValue(1, 0, 0));
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void GetAllValues(AMockObject mockDataSegment)
    {
        for (double x = 0; x <= mockDataSegment.MaxValueX; x += mockDataSegment.XSpacing)
        {
            for (double y = 0; y <= mockDataSegment.MaxValueY; y += mockDataSegment.YSpacing)
            {
                for (double z = 0; z <= mockDataSegment.MaxValueZ; z += mockDataSegment.ZSpacing)
                {
                    mockDataSegment.GetValue(x, y, z);
                }
            }
        }
    }

    private bool CheckMockAgainstLoadedFile(AMockObject mockObject, AData volumetricMockObject)
    {
        for (double x = 0; x<mockObject.MaxValueX; x += mockObject.XSpacing)
        {
            for(double y = 0; y<mockObject.MaxValueY; y += mockObject.YSpacing)
            {
                for(double z = 0; z<mockObject.MaxValueZ; z += mockObject.ZSpacing)
                {
                    if (Math.Abs(mockObject.GetValue(x, y, z) - volumetricMockObject.GetValue(x, y, z)) >= 1)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private Vector<double> GetTranslationVector(double x, double y, double z)
    {
        return Vector<double>.Build.DenseOfArray(new double[]
        {
            x, y, z
        });
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
