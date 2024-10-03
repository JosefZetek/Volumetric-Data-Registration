using UnityEngine;

public class MovementHandler : MonoBehaviour
{
    private float rotationSpeed = 5.0f;  // Speed of camera rotation
    private bool isRotating = false;

    private float translationSpeed = 20.0f;
    private float keyboardRotationSpeed = 50.0f;


    void Update()
    {
        RotationCameraHandler();
        TranslationCameraHandler();
    }

    private void RotationCameraHandler()
    {
        isRotating = Input.GetMouseButtonUp(0) ? false : isRotating;
        isRotating = Input.GetMouseButtonDown(0) ? true : isRotating;

        if (!isRotating)
            return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        float rotationY = mouseX * rotationSpeed;
        float rotationX = mouseY * rotationSpeed;

        transform.eulerAngles += new Vector3(-rotationX, rotationY, 0);
    }

    private void TranslationCameraHandler()
    {
        Vector3 translationDirection = new Vector3();

        if (Input.GetKey(KeyCode.W))
            translationDirection += transform.forward * translationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.S))
            translationDirection -= transform.forward * translationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.D))
            translationDirection += transform.right * translationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
            translationDirection -= transform.right * translationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q))
            translationDirection -= transform.up * translationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.E))
            translationDirection += transform.up * translationSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow))
            transform.eulerAngles += new Vector3(-keyboardRotationSpeed * Time.deltaTime, 0, 0);

        if (Input.GetKey(KeyCode.DownArrow))
            transform.eulerAngles += new Vector3(keyboardRotationSpeed * Time.deltaTime, 0, 0);

        if (Input.GetKey(KeyCode.LeftArrow))
            transform.eulerAngles += new Vector3(0, -keyboardRotationSpeed * Time.deltaTime, 0);

        if (Input.GetKey(KeyCode.RightArrow))
            transform.eulerAngles += new Vector3(0, keyboardRotationSpeed * Time.deltaTime, 0);




        transform.Translate(translationDirection, Space.World);

    }
}
