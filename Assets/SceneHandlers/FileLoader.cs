using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataView;

public class FileLoader : MonoBehaviour
{
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

    private bool ShowDialogBox(string title, string content)
    {
        return EditorUtility.DisplayDialog(title, content, "Select different file", "Cancel");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        FilePathDescriptor filePathDescriptor = GetFilePath();
        if(filePathDescriptor == null)
            return;

        VolumetricData loadedData = new VolumetricData(filePathDescriptor);

        loadedData.get
        EditorUtility.DisplayDialog("Selected file", filePathDescriptor.MHDFilePath + " | " + filePathDescriptor.DataFilePath, "OK");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
