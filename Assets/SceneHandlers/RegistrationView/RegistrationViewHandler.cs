using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using DataView;

public class RegistrationViewHandler : MonoBehaviour
{
    private VisualElement rootVisualElement;

    private FilePathDescriptor microDataPath;
    private FilePathDescriptor macroDataPath;

    private Button microDataLoadButton;
    private Button macroDataLoadButton;

    private Button runButton;
    private Label registrationStateLabel;


    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        this.rootVisualElement = uiDocument.rootVisualElement;

        this.rootVisualElement.Q<Button>("backButton").clicked += () => SceneManager.LoadScene("MainView");

        microDataLoadButton = rootVisualElement.Q<Button>("microDataLoad");
        macroDataLoadButton = rootVisualElement.Q<Button>("macroDataLoad");

        runButton = rootVisualElement.Q<Button>("runButton");

        this.rootVisualElement.Q<Button>("runButton").clicked += () => RunRegistration();

        microDataLoadButton.clicked += () =>
        {
            FilePathDescriptor file = FileDialog.GetFilePath();
            if (file == null)
                return;

            this.microDataPath = file;
            this.microDataLoadButton.text = "MICRO DATA: LOADED";

            this.runButton.SetEnabled(this.microDataPath != null && this.macroDataPath != null);
        };

        macroDataLoadButton.clicked += () =>
        {
            FilePathDescriptor file = FileDialog.GetFilePath();
            if (file == null)
                return;

            this.macroDataPath = file;
            this.macroDataLoadButton.text = "MACRO DATA: LOADED";

            this.runButton.SetEnabled(this.microDataPath != null && this.macroDataPath != null);
        };

        this.rootVisualElement.Q<Button>("runButton").SetEnabled(false);
        registrationStateLabel = this.rootVisualElement.Q<Label>("registrationState");
    }

    private void ShowRegistrationFinishedView(VolumetricData microData, VolumetricData macroData, Transform3D transformation)
    {
        //add elements to rootVisualElement
        VisualElement registrationPreview = rootVisualElement.Q<VisualElement>("registrationPreview");
        registrationPreview.Clear();

        Button buttonCutPreview = new Button();
        //Button buttonObjectPreview = new Button();
        //buttonObjectPreview.text = "Object preview";
        //buttonObjectPreview.style.height = new Length(50, LengthUnit.Percent);
        //buttonObjectPreview.style.width = new Length(20, LengthUnit.Percent);
        //registrationPreview.Add(buttonObjectPreview);

        buttonCutPreview.text = "Slicer preview";
        buttonCutPreview.style.height = new Length(50, LengthUnit.Percent);
        buttonCutPreview.style.width = new Length(20, LengthUnit.Percent);
        buttonCutPreview.clicked += () =>
        {
            CutViewerHandler.microData = microData;
            CutViewerHandler.macroData = macroData;
            CutViewerHandler.transformation = transformation;

            SceneManager.LoadScene("CutViewer");
        };

        registrationPreview.Add(buttonCutPreview);
    }

    private void RunRegistration()
    {
        registrationStateLabel.text = "STARTED";
        registrationStateLabel.style.fontSize = 100;
        RegistrationLauncher registrationLauncher = new RegistrationLauncher();

        Debug.Log("Loading micro data");
        VolumetricData microData = new VolumetricData(microDataPath);
        Debug.Log("Loading macro data");
        VolumetricData macroData = new VolumetricData(macroDataPath);

        Transform3D tr = registrationLauncher.RunRegistration(microData, macroData);
        Transform3D finalTransformation = registrationLauncher.RevertCenteringTransformation(tr);
        Debug.Log("Transformation: " + finalTransformation);

        ShowRegistrationFinishedView(microData, macroData, finalTransformation);

        //ShowRegistrationFinishedView();
    }
}