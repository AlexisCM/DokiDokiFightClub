using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public PlayerInput PlayerInput;         // Handles player inputs (KMB or Gamepad)
    public CharacterController Controller;  // Handles moving the player's body

    InputMaster PlayerInputActions;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;     // Player's movement speed
    [SerializeField] private float _jumpHeight = 1f;    // How high the player can jump
    [SerializeField] private float _gravity = -9.81f;   // Force of gravity applied to movement

    private Vector3 _moveDirection; // Direction player will move
    private Vector3 _velocity;      // Movement velocity

    void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        Controller = GetComponent<CharacterController>();

        PlayerInputActions = new InputMaster();
        // Attach callbacks to player input actions
        PlayerInputActions.Player.Jump.performed += Jump;
        PlayerInputActions.Player.Crouch.performed += Crouch;
    }

    private void OnEnable()
    {
        PlayerInputActions.Player.Movement.Enable();
        PlayerInputActions.Player.Jump.Enable();
        PlayerInputActions.Player.Crouch.Enable();
    }

    private void OnDisable()
    {
        PlayerInputActions.Player.Movement.Disable();
        PlayerInputActions.Player.Jump.Disable();
        PlayerInputActions.Player.Crouch.Disable();
    }

    private void Update()
    {
        Move();
    }

    /// <summary>
    /// Called on update to check for player movement inputs. Move player object accordingly.
    /// </summary>
    public void Move()
    {
        // TODO: Account for "CanMove" and LeftShift to toggle walk/sprint speeds

        // Reset velocity if player is not jumping
        if (Controller.isGrounded && _velocity.y < 0)
            _velocity.y = -2f;

        // Get input for movedirection every update (actual method to move player is in fixed update for physics)
        Vector2 inputVec = PlayerInputActions.Player.Movement.ReadValue<Vector2>();
        _moveDirection = transform.right * inputVec.x + transform.forward * inputVec.y;

        Controller.Move(_moveDirection * _moveSpeed * Time.deltaTime);
        _velocity.y += _gravity * Time.deltaTime;
        Controller.Move(_velocity * Time.deltaTime);
    }

    /// <summary>
    /// Allow player to jump on key press if they are currently on the ground.
    /// </summary>
    /// <param name="context">input action callback</param>
    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && Controller.isGrounded)
        {
            Debug.Log("jumpman jumpman jumpman jumpman--");
            _velocity.y = Mathf.Sqrt(_jumpHeight * -3f * _gravity);
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // TODO
            Debug.Log("dip dip potatah chip");
        }
    }
}
