using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public CharacterController Controller;
    public Camera PlayerCamera;

    // Movement Variables
    [Header("Movement Settings")]
    [HideInInspector] public bool CanMove = true;
    [SerializeField] private float _walkingSpeed = 7.5f;
    [SerializeField] private float _runningSpeed = 11.5f;
    [SerializeField] private float _jumpSpeed = 8.0f;
    [SerializeField] private float _gravity = 20.0f;
    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;

    // Look/Camera Variables
    [Header("Camera Settings")]
    [SerializeField] private float _lookSpeed = 2.0f;
    [SerializeField] private float _lookXLimit = 45.0f;

    void Start()
    {
        Controller.GetComponent<CharacterController>();

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // TODO:
        // Account for Player Settings, which may change Left Shift to Walk instead of Run

        // Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currSpeedX = CanMove ? (isRunning ? _runningSpeed : _walkingSpeed) * Input.GetAxis("Horizontal") : 0;
        float currSpeedY = CanMove ? (isRunning ? _runningSpeed : _walkingSpeed) * Input.GetAxis("Vertical") : 0;
        float movementDirectionY = _moveDirection.y;
        _moveDirection = (forward * currSpeedX) + (right * currSpeedY);

        if (Input.GetButton("Jump") && CanMove && Controller.isGrounded)
        {
            _moveDirection.y = _jumpSpeed;
        }
        else
        {
            _moveDirection.y = movementDirectionY;
        }

        // Apply gravity
        if (!Controller.isGrounded)
            _moveDirection.y -= _gravity * Time.deltaTime;

        Controller.Move(_moveDirection * Time.deltaTime);

        // Player and Camera rotation
        if (CanMove)
        {
            _rotationX += Input.GetAxis("Mouse Y") * _lookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -_lookXLimit, _lookXLimit);
            PlayerCamera.transform.localRotation = Quaternion.Euler(-_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * _lookSpeed, 0);
        }
    }
}
