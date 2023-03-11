using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    /// <summary>
    /// Handles rotation of the camera on X and Y axes based on player inputs.
    /// </summary>
    public class CameraController : NetworkBehaviour
    {
        public GameObject PlayerCamera;    // Reference to player camera's game object

        [SerializeField] private float _mouseSensitivity = 400f;

        private float _xRotation;
        private float _yRotation;

        void Start()
        {
            // Lock and hide cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!isLocalPlayer)
                return;

            Transform mainCamera = Camera.main.gameObject.transform; // Get the scene's main camera
                                                                     // Assign the main camera to the player's camera object
            mainCamera.parent = PlayerCamera.transform;
            mainCamera.position = PlayerCamera.transform.position;
            mainCamera.rotation = PlayerCamera.transform.rotation;
        }

        void Update()
        {
            if (!isLocalPlayer)
                return;

            float mouseX = Input.GetAxisRaw("Mouse X") * _mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxisRaw("Mouse Y") * _mouseSensitivity * Time.deltaTime;

            _yRotation += mouseX;
            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

            PlayerCamera.transform.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);  // Rotate camera
            transform.rotation = Quaternion.Euler(0, _yRotation, 0); // Rotate player's body
        }
    }
}
