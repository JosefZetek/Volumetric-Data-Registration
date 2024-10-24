using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using DataView;

public class CutViewerHandler : MonoBehaviour
{
    private Slider slider;
    private DropdownField dropdown;
    private VisualElement cutPreview;

    private DataSlicer dataSlicer;

    public static Transform3D transformation;
    public static AData microData;
    public static AData macroData;

    //private CustomData customData;

    private Image image;



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

    public Color[][] GetCutData(double sliderValue, int axis, CutResolution resolution)
    {
        return dataSlicer.Cut(sliderValue, axis, resolution);
    }

    public Color[][] GetTransformedCut(double sliderValue, int axis, CutResolution resolution)
    {
        return dataSlicer.TransformationCut(sliderValue, axis, microData, transformation, resolution);
    }

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

        if (macroData != null)
            this.dataSlicer = new DataSlicer(macroData);

    }

    private void LoadObject()
    {
        FilePathDescriptor filePathDescriptor = DataFileDialog.GetFilePath();
        if (filePathDescriptor == null)
            return;

        ResetValues();

        AData volumetricData = new VolumetricData(filePathDescriptor);
        this.dataSlicer = new DataSlicer(volumetricData);

        UpdateImage();
    }

    void UpdateImage()
    {
        if (dataSlicer == null)
            return;

        Color[][] values;
        CutResolution resolution = new CutResolution(500, 500);


        if (microData != null)
            values = GetTransformedCut(slider.value, dropdown.index, resolution);
        else
            values = GetCutData(slider.value, dropdown.index, resolution);

        image.image = GetTexture(values);
    }

    private void ResetValues()
    {
        microData = null;
        macroData = null;
        transformation = null;
    }

    private void OnDestroy()
    {
        ResetValues();
    }
}
