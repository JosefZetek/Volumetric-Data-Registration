using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataView;
using UnityEngine.UIElements;

public class Loader : MonoBehaviour
{
    /// <summary>
    /// Mesh for voxel representation
    /// </summary>
    public Mesh mesh;

    /// <summary>
    /// Material linked with shader file
    /// </summary>
    public Material material;

    /// <summary>
    /// Batches to render
    /// </summary>
    private Matrix4x4[][] batches = new Matrix4x4[0][];

    /// <summary>
    /// List of arguments representing 
    /// </summary>
    private float[][] properties = new float[0][];

    /// <summary>
    /// Size of a batch thats being rendered simultaneously
    /// </summary>
    private const int BATCH_SIZE = 1023; // Maximum batch size for GPU instancing

    /// <summary>
    /// Calculates number of vertices
    /// </summary>
    /// <param name="volumetricData">Instance of Volumetric Data</param>
    /// <returns>Returns number of vertices in VolumetricData instance</returns>
    private int GetNumberOfVertices(AData volumetricData)
    {
        int NUMBER_OF_VERTICES_X = (int)(volumetricData.Measures[0] / volumetricData.XSpacing);
        int NUMBER_OF_VERTICES_Y = (int)(volumetricData.Measures[1] / volumetricData.YSpacing);
        int NUMBER_OF_VERTICES_Z = (int)(volumetricData.Measures[2] / volumetricData.ZSpacing);

        int NUMBER_OF_VERTICES = NUMBER_OF_VERTICES_X * NUMBER_OF_VERTICES_Y * NUMBER_OF_VERTICES_Z;

        //Mathf.CeilToInt(NUMBER_OF_VERTICES / BATCH_SIZE);
        return NUMBER_OF_VERTICES;
    }

    private void InitBatches(int NUMBER_OF_VERTICES)
    {
        int NUMBER_OF_BATCHES = Mathf.CeilToInt((float)((double)NUMBER_OF_VERTICES / (double)BATCH_SIZE));

        this.batches = new Matrix4x4[NUMBER_OF_BATCHES][];
        this.properties = new float[NUMBER_OF_BATCHES][];
    }

    /// <summary>
    /// Adds space for another batch
    /// </summary>
    /// <param name="NUMBER_OF_VERTICES">Total number of vertices</param>
    /// <param name="batchNumber">Number of a current batch being the index of the current batch</param>
    private void AddAnotherBatch(int NUMBER_OF_VERTICES, int batchNumber)
    {
        //Number of vertices to be placed in this batch (smaller than batch size if remainder)
        int VERTICES_IN_BATCH = Mathf.Min(NUMBER_OF_VERTICES - batchNumber * BATCH_SIZE, BATCH_SIZE);

        batches[batchNumber] = new Matrix4x4[VERTICES_IN_BATCH];
        properties[batchNumber] = new float[VERTICES_IN_BATCH];
    }

    private void LoadData(FilePathDescriptor filePathDescriptor)
    {
        AData loadedData = new VolumetricData(filePathDescriptor);

        int NUMBER_OF_VERTICES = GetNumberOfVertices(loadedData);
        InitBatches(NUMBER_OF_VERTICES);

        int batchNumber = 0;
        int orderNumber = 0;


        for (float i = 0; i < loadedData.Measures[0]; i += (float)loadedData.XSpacing)
        {
            for (float j = 0; j < loadedData.Measures[1]; j += (float)loadedData.YSpacing)
            {
                for (float k = 0; k < loadedData.Measures[2]; k += (float)loadedData.ZSpacing)
                {
                    if (orderNumber == 0)
                        AddAnotherBatch(NUMBER_OF_VERTICES, batchNumber);

                    double currentValue = loadedData.GetValue(i, j, k);
                    float normalizedValue = (float)((currentValue - loadedData.MinValue)/ (loadedData.MaxValue - loadedData.MinValue));

                    batches[batchNumber][orderNumber] = Matrix4x4.TRS(new Vector3(i, j, k), Quaternion.identity, Vector3.one);
                    properties[batchNumber][orderNumber] = normalizedValue;

                    batchNumber += (orderNumber >= (BATCH_SIZE-1)) ? 1 : 0;
                    orderNumber = (orderNumber + 1) % BATCH_SIZE;
                }
            }
        }
    }

    private void RenderBatches()
    {
        for (int b = 0; b < batches.Length; b++)
        {
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetFloatArray("_Color", properties[b]);

            Graphics.DrawMeshInstanced(mesh, 0, material, batches[b], batches[b].Length, props);
        }
    }

    private void AddActionsToUI()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        rootVisualElement.Q<Button>("loadObjectButton").clicked += () =>
        {
            FilePathDescriptor filePathDescriptor = FileDialog.GetFilePath();
            if (filePathDescriptor == null)
                return;
            LoadData(filePathDescriptor);
        };
    }

    void OnEnable()
    {
        AddActionsToUI();
    }

    void Update()
    {
        RenderBatches();
    }
}