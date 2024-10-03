using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainViewHandler : MonoBehaviour
{

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var rootVisualElement = uiDocument.rootVisualElement;

        Button runRegistrationButton = rootVisualElement.Q<Button>("runRegistration");
        Button createObjectButton = rootVisualElement.Q<Button>("createObject");
        Button objectViewerButton = rootVisualElement.Q<Button>("objectViewer");
        Button slicerButton = rootVisualElement.Q<Button>("slicer");

        runRegistrationButton.clicked += () => {
            SceneManager.LoadScene("RegistrationRunner");
        };

        createObjectButton.clicked += () =>
        {
            SceneManager.LoadScene("ModelCreator");
        };

        objectViewerButton.clicked += () =>
        {
            SceneManager.LoadScene("ObjectViewer");
        };

        slicerButton.clicked += () =>
        {
            SceneManager.LoadScene("CutViewer");
        };

    }
}
