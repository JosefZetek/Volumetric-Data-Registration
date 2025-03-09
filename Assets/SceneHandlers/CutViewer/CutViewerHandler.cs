using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using DataView;

public class CutViewerHandler : MonoBehaviour
{
    private Slider slider;
    private DropdownField dropdown;
    private VisualElement cutPreview;

    private static IDataSlicer dataSlicer;
    private Image image;

    /// <summary>
    /// Sets slicer for data that aligns microData onto macroData
    /// </summary>
    /// <param name="microData">Micro data</param>
    /// <param name="macroData">Macro data</param>
    /// <param name="transformation">Expected transformation that aligns microData onto macroData by applying Rx+t</param>
    public static void SetDataSlicer(AData microData, AData macroData, Transform3D transformation)
    {
        dataSlicer = new TransformedDataSlicer(macroData, microData, transformation);
    }

    /// <summary>
    /// Sets slicer for data to display passed data
    /// </summary>
    /// <param name="data">Data to be displayed</param>
    public static void SetDataSlicer(AData data)
    {
        dataSlicer = new DataSlicer(data);
    }

    public static void SetFCSlicer(AData data)
    {
        dataSlicer = new DataFCSlicer(data);
    }

    /// <summary>
    /// Creates a texture where 1 value in passed array is 1 pixel
    /// </summary>
    /// <param name="values">
    /// 2D array of values between 0 and 1 representing color for each pixel.
    /// Data organized as follows [row][column], where [0][0] is bottom left corner.
    /// </param>
    /// <returns>Texture with values applied.</returns>
    public Texture2D GetTexture(Color[][] values)
    {
        int height = values.Length;
        if (height == 0)
            return null;

        int width = values[0].Length;

        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        for (int i = 0; i < values.Length; i++)
            texture.SetPixels(0, i, values[i].Length, 1, values[i]);

        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Method sets event handlers for UI components
    /// </summary>
    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        slider = rootVisualElement.Q<Slider>("axisSlider");
        slider.RegisterValueChangedCallback(evt => UpdateImage());

        dropdown = rootVisualElement.Q<DropdownField>("axisDropdown");
        dropdown.RegisterValueChangedCallback(evt => UpdateImage());

        cutPreview = rootVisualElement.Q<VisualElement>("cutPreview");

        this.image = new Image();
        //this.image.scaleMode = ScaleMode.StretchToFill;
        //this.image.scaleMode = ScaleMode.ScaleToFit;

        image.style.width = new StyleLength(Length.Percent(100));
        image.style.height = new StyleLength(Length.Percent(100));

        cutPreview.Add(image);

        rootVisualElement.Q<Button>("backButton").clicked += () => SceneManager.LoadScene("MainView");
        rootVisualElement.Q<Button>("loadButton").clicked += () => LoadObject();
    }

    private void LoadObject()
    {
        FilePathDescriptor filePathDescriptor = DataFileDialog.GetFilePath();
        if (filePathDescriptor == null)
            return;

        ResetValues();

        AData volumetricData = new VolumetricData(filePathDescriptor);
        dataSlicer = new DataSlicer(volumetricData);

        UpdateImage();
    }

    void UpdateImage()
    {
        if (dataSlicer == null)
            return;

        Color[][] values;
        CutResolution resolution = new CutResolution(500, 500);

        //values = dataSlicer.Cut(slider.value, dropdown.index, resolution);
        values = dataSlicer.Cut(0.5, 2, resolution);

        image.image = GetTexture(values);
    }

    private void ResetValues()
    {
        dataSlicer = null;
    }

    private void OnDestroy()
    {
        ResetValues();
    }
}
