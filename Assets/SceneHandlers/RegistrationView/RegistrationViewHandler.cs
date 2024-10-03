using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using DataView;

public class RegistrationViewHandler : MonoBehaviour
{

    private FilePathDescriptor microDataPath;
    private FilePathDescriptor macroDataPath;

    private Button microDataLoadButton;
    private Button macroDataLoadButton;

    private Button runButton;
    private Label registrationStateLabel;


    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        rootVisualElement.Q<Button>("backButton").clicked += () => SceneManager.LoadScene("MainView");

        microDataLoadButton = rootVisualElement.Q<Button>("microDataLoad");
        macroDataLoadButton = rootVisualElement.Q<Button>("macroDataLoad");

        runButton = rootVisualElement.Q<Button>("runButton");

        rootVisualElement.Q<Button>("runButton").clicked += () => RunRegistration();

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

        rootVisualElement.Q<Button>("runButton").SetEnabled(false);
        registrationStateLabel = rootVisualElement.Q<Label>("registrationState");

    }

    private void RunRegistration()
    {
        registrationStateLabel.text = "STARTED";
        registrationStateLabel.style.fontSize = 100;
        RegistrationLauncher registrationLauncher = new RegistrationLauncher();
        Transform3D tr = registrationLauncher.RunRegistration(microDataPath, macroDataPath);

        Transform3D finalTransformation = registrationLauncher.RevertCenteringTransformation(tr);
        registrationStateLabel.style.fontSize = 50;
        registrationStateLabel.text = "Result transformation:\n" + finalTransformation.ToString();
    }
}
