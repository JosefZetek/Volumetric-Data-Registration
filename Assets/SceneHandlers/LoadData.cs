using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataView;

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
    private List<Matrix4x4[]> batches = new List<Matrix4x4[]>();

    /// <summary>
    /// Colors for given voxels
    /// </summary>
    private List<Vector4[]> colorData = new List<Vector4[]>();

    /// <summary>
    /// Size of a batch thats being rendered simultaneously
    /// </summary>
    private const int BATCH_SIZE = 1023; // Maximum batch size for GPU instancing

    private bool ShowDialogBox(string title, string content)
    {
        return EditorUtility.DisplayDialog(title, content, "Select different file", "Cancel");
    }

    private FilePathDescriptor GetFilePath()
    {
        string metadataPath = "", dataPath = "";

        while (metadataPath.Length == 0)
        {
            metadataPath = EditorUtility.OpenFilePanel("Select metadata file descriptor for input object.", "", "mhd");

            if (metadataPath.Length == 0 && !ShowDialogBox("Meatadata file descriptor was not selected.", "Would you like to continue"))
                return null;
        }

        while (dataPath.Length == 0)
        {
            dataPath = EditorUtility.OpenFilePanel("Select data file with input object.", metadataPath, "raw");
            if (dataPath.Length == 0 && !ShowDialogBox("Data file with input object was not selected.", "Would you like to continue"))
                return null;
        }

        return new FilePathDescriptor(metadataPath, dataPath);
    }

    private void LoadData(FilePathDescriptor filePathDescriptor)
    {
        VolumetricData loadedData = new VolumetricData(filePathDescriptor);
        List<Matrix4x4> currentBatch = new List<Matrix4x4>();
        List<Vector4> currentColorData = new List<Vector4>();

        for (float i = 0; i < loadedData.Measures[0]; i += (float)loadedData.XSpacing)
        {
            for (float j = 0; j < loadedData.Measures[1]; j += (float)loadedData.YSpacing)
            {
                for (float k = 0; k < loadedData.Measures[2]; k += (float)loadedData.ZSpacing)
                {
                    if (currentBatch.Count >= BATCH_SIZE)
                    {
                        batches.Add(currentBatch.ToArray());
                        colorData.Add(currentColorData.ToArray());
                        currentBatch = new List<Matrix4x4>();
                        currentColorData = new List<Vector4>();
                    }

                    currentBatch.Add(Matrix4x4.TRS(new Vector3(i, j, k), Quaternion.identity, Vector3.one));

                    double currentValue = loadedData.GetValue(i, j, k);
                    float normalizedValue = (float)(currentValue / loadedData.MaxValue);
                    currentColorData.Add(new Vector4(0, 0, normalizedValue, 1f));
                }
            }
        }

        // Add any remaining data
        if (currentBatch.Count > 0)
        {
            batches.Add(currentBatch.ToArray());
            colorData.Add(currentColorData.ToArray());
        }
    }

    private void RenderBatches()
    {
        for (int b = 0; b < batches.Count; b++)
        {
            MaterialPropertyBlock props = new MaterialPropertyBlock();
            props.SetVectorArray("_Color", colorData[b]);

            Graphics.DrawMeshInstanced(mesh, 0, material, batches[b], batches[b].Length, props);
        }
    }

    void Start()
    {
        FilePathDescriptor filePathDescriptor = GetFilePath();
        if (filePathDescriptor == null)
            return;
        LoadData(filePathDescriptor);
    }

    void Update()
    {
        RenderBatches();
    }
}