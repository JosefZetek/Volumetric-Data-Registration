using UnityEditor;

/// <summary>
/// This class is used for invoking dialog with selection of .mhd file as well as .raw file
/// </summary>
class DataFileDialog
{
    private static bool ShowDialogBox(string title, string content)
    {
        return EditorUtility.DisplayDialog(title, content, "Select different file", "Cancel");
    }

    public static FilePathDescriptor GetFilePath()
    {
        string metadataPath = EditorUtility.OpenFilePanel("Select metadata file descriptor for input object.", string.Empty, "mhd");
        if (metadataPath.Length == 0 && !ShowDialogBox("Meatadata file descriptor was not selected.", "Would you like to continue"))
            return null;

        string dataPath = EditorUtility.OpenFilePanel("Select data file with input object.", metadataPath, "raw");
        if (dataPath.Length == 0 && !ShowDialogBox("Data file with input object was not selected.", "Would you like to continue"))
            return null;

        return new FilePathDescriptor(metadataPath, dataPath);
    }

    public static string GetDirectory()
    {
        string directoryPath = EditorUtility.OpenFolderPanel("Please, select directory", string.Empty, string.Empty);
        if (directoryPath.Length == 0 && !EditorUtility.DisplayDialog("Directory was not selected.", "Please, choose operation", "Select different folder", "Cancel"))
            return null;

        return directoryPath;
    }
}
