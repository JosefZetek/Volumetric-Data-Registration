using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FileLoader : MonoBehaviour
{
    private string LoadFile()
    {

        string path = EditorUtility.OpenFilePanel("Select mhd file with file description.", "", "py");

        return path;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Loading file");
        string path = LoadFile();
        Debug.Log("Loaded file: " + path);
        EditorUtility.DisplayDialog("Selected file", path, "OK");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
