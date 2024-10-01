using UnityEngine;
using UnityEngine.UIElements;
using MathNet.Numerics.LinearAlgebra;
using DataView;

class CutViewerHandler : MonoBehaviour
{
    private Slider slider;
    private DropdownField dropdown;
    private VisualElement cutPreview;

    private DataSlicer dataSlicer;

    private CustomData customData;

    private Image image;

    /// <summary>
    /// Creates a texture where 1 value in passed array is 1 pixel
    /// </summary>
    /// <param name="values">
    /// 2D array of values between 0 and 1 representing color for each pixel.
    /// Data organized as follows [row][column], where [0][0] is bottom left corner.
    /// </param>
    /// <returns>Texture with values applied.</returns>
    public Texture2D GetTextureSequentially(Color[][] values)
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
                texture.SetPixel(j, i, values[i][j]);
            }
        }

        texture.Apply();
        return texture;
    }

    public Color[][] GetCutData(double sliderValue, int axis, CutResolution resolution)
    {
        Color[][] values = dataSlicer.TransformationCut(sliderValue, axis, customData, new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.DenseOfArray(new double[] { -1, 0, 0 })), resolution);
        //Color[][] values = dataSlicer.Cut(sliderValue, axis, resolution);
        return values;
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
        //VolumetricData d = new VolumetricData(new FilePathDescriptor("/Users/pepazetek/Desktop/rotatedEllipsoidMicro.mhd", "/Users/pepazetek/Desktop/rotatedEllipsoidMicro.raw"));

        customData = new CustomData();
        this.dataSlicer = new DataSlicer(customData);

        UpdateImage();
    }

    void UpdateImage()
    {
        if (dataSlicer == null)
            return;

        Color[][] values = GetCutData(slider.value, dropdown.index, new CutResolution(3, 3));

        image.image = GetTextureSequentially(values);
    }
}
