using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DataView;

class CutViewerHandler : MonoBehaviour
{
    private Slider slider;
    private DropdownField dropdown;
    private VisualElement cutPreview;

    private VolumetricData volumetricData;

    private Image image;

    /// <summary>
    /// Creates a texture where 1 value in passed array is 1 pixel
    /// </summary>
    /// <param name="values">
    /// 2D array of values between 0 and 1 representing color for each pixel.
    /// Data organized as follows [row][column], where [0][0] is bottom left corner.
    /// </param>
    /// <returns>Texture with values applied.</returns>
    public Texture2D GetTextureSequentially(double[][] values)
    {
        int height = values.Length;
        if (height == 0)
            return null;

        int width = values[0].Length;


        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].Length != width)
                return null;

            for (int j = 0; j < values[i].Length; j++)
            {
                texture.SetPixel(j, i, new Color((float)values[i][j], (float)values[i][j], (float)values[i][j]));
            }
        }

        texture.Apply();
        return texture;
    }

    public double[][] GetCutData(double sliderValue, int axis, CutResolution resolution)
    {
        double[][] values = volumetricData.Cut(sliderValue * (volumetricData.Measures[axis] - 1), axis, resolution);
        PrintStatistics(values);
        return values;
    }

    private void PrintStatistics(double[][] values)
    {
        float minValue = float.MaxValue, maxValue = float.MinValue;

        for(int i = 0; i< values.Length; i++)
        {
            for(int j = 0; j < values[i].Length; j++)
            {
                minValue = Mathf.Min((float)values[i][j], minValue);
                maxValue = Mathf.Max((float)values[i][j], maxValue);
            }
        }

        Debug.Log("Min value = " + minValue + ", Max value = " + maxValue);
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
        this.image.scaleMode = ScaleMode.StretchToFill;

        image.style.width = new StyleLength(Length.Percent(100));
        image.style.height = new StyleLength(Length.Percent(100));

        cutPreview.Add(image);
    }

    private void Start()
    {
        this.volumetricData = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/rotatedEllipsoidMicro.mhd", "/Users/pepazetek/Desktop/rotatedEllipsoidMicro.raw"));
        UpdateImage();
    }

    void UpdateImage()
    {
        if (volumetricData == null)
            return;

        double[][] values = GetCutData(slider.value, dropdown.index, new CutResolution(100, 100));

        image.image = GetTextureSequentially(values);
    }
}
