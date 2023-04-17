using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

namespace DokiDokiFightClub
{
    /// <summary>
    /// Handles Player movement based on input.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        public PlayerInput PlayerInput;         // Handles player inputs (KMB or Gamepad)
        public CharacterController Controller;  // Handles moving the player's body
        public Transform CameraTransform;       // The transform to control the player camera's rotation

        InputMaster PlayerInputActions;

        [Header("Movement Settings")]
        [SerializeField] private float _walkSpeed = 2f;     // Player's walk speed
        [SerializeField] private float _sprintSpeed = 4f;   // Player's sprint speed
        [SerializeField] private float _jumpHeight = 1f;    // How high the player can jump
        [SerializeField] private float _gravity = -9.81f;   // Force of gravity applied to movement

        [Header("Player Movement Preferences")]
        [SerializeField] private bool _isDefaultWalking;    // Determine if player's default speed is walking or sprinting

        private float _moveSpeed;       // Player's current movement speed
        private Vector3 _moveDirection; // Direction player will move
        private Vector3 _velocity;      // Movement velocity
        private bool _isChangingSpeed;  // Check if player is holding key to change from walk to run or vice versa
        private bool _isCrouching;      // Check if player is crouching or nah

        private Vector3 _playerCentre;  // Original centre of PlayerController
        private float _playerHeight;    // Original height of PlayerController

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            PlayerInputActions = new InputMaster();

            // Attach callbacks to player input actions
            PlayerInputActions.Player.Jump.performed += Jump;
            PlayerInputActions.Player.Crouch.performed += Crouch;
            PlayerInputActions.Player.ChangeSpeed.performed += ChangeSpeed;
        }

        private void OnValidate()
        {
            if (Controller == null)
                Controller = GetComponent<CharacterController>();

            Controller.enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<NetworkTransform>().syncDirection = SyncDirection.ClientToServer;
        }

        public override void OnStartLocalPlayer()
        {
            Controller.enabled = true;

            _playerHeight = Controller.height;
            _playerCentre = Controller.center;
            _isCrouching = false;
            _isChangingSpeed = false;

            // TODO: Handle stored player preferences for default speed (walking or sprinting)
            _isDefaultWalking = false;
            _moveSpeed = GetDefaultMoveSpeed();
        }

        private void OnEnable()
        {
            PlayerInputActions.Player.Movement.Enable();
            PlayerInputActions.Player.Jump.Enable();
            PlayerInputActions.Player.Crouch.Enable();
            PlayerInputActions.Player.ChangeSpeed.Enable();
        }

        private void OnDisable()
        {
            PlayerInputActions.Player.Movement.Disable();
            PlayerInputActions.Player.Jump.Disable();
            PlayerInputActions.Player.Crouch.Disable();
            PlayerInputActions.Player.ChangeSpeed.Disable();
        }

        private void Update()
        {
            if (isLocalPlayer)
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

            // Get input for movedirection every update
            Vector2 inputVec = PlayerInputActions.Player.Movement.ReadValue<Vector2>();
            _moveDirection = transform.right * inputVec.x + transform.forward * inputVec.y;

            Controller.Move(_moveSpeed * Time.deltaTime * _moveDirection);
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
                _velocity.y = Mathf.Sqrt(_jumpHeight * -3f * _gravity);
            }
        }

        public void Crouch(InputAction.CallbackContext context)
        {
            if (context.performed && !_isCrouching)
            {
                Debug.Log("dip dip potatah chip");
                Controller.center = new Vector3(0f, Controller.center.y * 0.5f, 0f);
                Controller.height = _playerHeight * 0.5f;
                _moveSpeed = _walkSpeed;
                _isCrouching = true;
            }
            else if (context.performed && _isCrouching)
            {
                Debug.Log("undip");
                // TODO: Handle if player tries to un-crouch when below an object shorter than their standing height
                Controller.center = _playerCentre;
                Controller.height = _playerHeight;
                _moveSpeed = GetDefaultMoveSpeed();
                _isCrouching = false;
            }
        }

        /// <summary>
        /// Change MoveSpeed to walking or sprinting when player presses Left Control.
        /// </summary>
        /// <param name="context"></param>
        public void ChangeSpeed(InputAction.CallbackContext context)
        {
            if (context.performed && !_isChangingSpeed)
            {
                _moveSpeed = _isDefaultWalking ? _sprintSpeed : _walkSpeed;
            }
            else if (context.performed && _isChangingSpeed)
            {
                _moveSpeed = GetDefaultMoveSpeed();
            }
            _isChangingSpeed = !_isChangingSpeed;
        }

        private float GetDefaultMoveSpeed()
        {
            return _isDefaultWalking ? _walkSpeed : _sprintSpeed;
        }
    }
}
