using UnityEngine;

/// <summary>
/// Handles rotation of the camera on X and Y axes based on player inputs.
/// </summary>
public class CameraController : MonoBehaviour
{
    public Transform PlayerBody;    // Reference to player model's transform

    [SerializeField] private float _mouseSensitivity = 400f;

    private float _xRotation;
    private float _yRotation;

    void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * Time.deltaTime;

        _yRotation += mouseX;
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);  // Rotate camera
        PlayerBody.rotation = Quaternion.Euler(0, _yRotation, 0); // Rotate player's body
    }
}
