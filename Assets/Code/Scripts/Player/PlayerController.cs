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
        public Animator PlayerAnimator;         // Handles player movement animations

        InputMaster PlayerInputActions;

        [Header("Movement Settings")]
        [SerializeField] private float _walkSpeed = 2f;     // Player's walk speed
        [SerializeField] private float _sprintSpeed = 4f;   // Player's sprint speed
        [SerializeField] private float _jumpHeight = 2f;    // How high the player can jump
        [SerializeField] private float _gravity = -9.81f;   // Force of gravity applied to movement
        private readonly float _groundedGravity = -0.05f;   // Ensure CharacterController detects when player is grounded

        [Header("Player Movement Preferences")]
        [SerializeField] private bool _isDefaultWalking; // Determine if player's default speed is walking or sprinting

        private float _moveSpeed;       // Player's current movement speed
        private Vector3 _moveDirection; // Direction player will move
        private Vector3 _velocity;      // Movement velocity
        private bool _isChangingSpeed;  // Check if player is holding key to change from walk to run or vice versa
        private bool _isCrouching;      // Check if player is crouching or nah
        private bool _isMovementPressed; // Check for player movement input

        private Vector3 _playerCentre;  // Original centre of PlayerController
        private float _playerHeight;    // Original height of PlayerController

        // animator hashes
        readonly int _isWalkingHash = Animator.StringToHash("isWalking");
        readonly int _isRunningHash = Animator.StringToHash("isRunning");

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            PlayerInputActions = new InputMaster();

            // Attach callbacks to player input actions
            PlayerInputActions.Player.Movement.started += OnMovementInput;
            PlayerInputActions.Player.Movement.canceled += OnMovementInput;
            PlayerInputActions.Player.Movement.performed += OnMovementInput;
            PlayerInputActions.Player.Jump.performed += Jump;
            PlayerInputActions.Player.Crouch.performed += Crouch;
            PlayerInputActions.Player.ChangeSpeed.started += ChangeSpeed;
            PlayerInputActions.Player.ChangeSpeed.canceled += ChangeSpeed;
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

            CmdGroundPlayer();

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
            if (!isLocalPlayer)
                return;

            Animate();
            Move();
        }

        void OnMovementInput(InputAction.CallbackContext context)
        {
            // Store movement input
            var inputVec = context.ReadValue<Vector2>();
            _moveDirection = transform.right * inputVec.x + transform.forward * inputVec.y;
            _isMovementPressed = inputVec.x != 0 || inputVec.y != 0;
        }

        /// <summary> Called on update to check for player movement inputs. Move player object accordingly.</summary>
        void Move()
        {
            // Reset velocity if player is not jumping
            if (Controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            Controller.Move(_moveSpeed * Time.deltaTime * _moveDirection);
            ApplyGravity();
            Controller.Move(_velocity * Time.deltaTime);
        }

        void Animate()
        {
            bool isRunningAnim = PlayerAnimator.GetBool(_isRunningHash);
            bool isWalkingAnim = PlayerAnimator.GetBool(_isWalkingHash);

            // Handle running animation
            if (_isMovementPressed && !isRunningAnim)
            {
                PlayerAnimator.SetBool(_isRunningHash, true);
            }
            else if (!_isMovementPressed && isRunningAnim)
            {
                PlayerAnimator.SetBool(_isRunningHash, false);
            }
        }

        void ApplyGravity()
        {
            // CharacterController deems itself "floating" if downward movement is 0
            // apply proper gravity depending on whether or not character is grounded
            if (Controller.isGrounded)
            {
                _velocity.y = _groundedGravity;
            }
            else
            {
                _velocity.y += _gravity * Time.deltaTime;
            }
        }

        /// <summary> Allow player to jump on key press if they are currently on the ground. </summary>
        /// <param name="context">input action callback</param>
        public void Jump(InputAction.CallbackContext context)
        {
            if (Controller.isGrounded)
            {
                _velocity.y = Mathf.Sqrt(_jumpHeight * -3f * _gravity);
                //PlayerAnimator.SetTrigger("Jump");
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
            else if (context.canceled && _isCrouching)
            {
                Debug.Log("undip");
                // TODO: Handle if player tries to un-crouch when below an object shorter than their standing height
                Controller.center = _playerCentre;
                Controller.height = _playerHeight;
                _moveSpeed = GetDefaultMoveSpeed();
                _isCrouching = false;
            }
        }

        /// <summary> Change MoveSpeed to walking or sprinting when player presses Left Control. </summary>
        /// <param name="context"></param>
        public void ChangeSpeed(InputAction.CallbackContext context)
        {
            _isChangingSpeed = !context.ReadValueAsButton();

            if (_isChangingSpeed)
            {
                _moveSpeed = GetDefaultMoveSpeed();
                
            }
            else
            {
                _moveSpeed = _isDefaultWalking ? _sprintSpeed : _walkSpeed;
            }
        }

        private float GetDefaultMoveSpeed()
        {
            return _isDefaultWalking ? _walkSpeed : _sprintSpeed;
        }

        [Command]
        private void CmdGroundPlayer()
        {
            var pos = transform.position;
            transform.position = new Vector3(pos.x, 0f, pos.z);
        }
    }
}
