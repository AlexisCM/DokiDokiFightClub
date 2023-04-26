using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DokiDokiFightClub
{
    public class Player : NetworkBehaviour
    {
        public PlayerInput PlayerInput; // Handles player inputs (KMB or Gamepad)
        public HealthMetre Health;      // Handles player's Max and Current Health
        public Weapon ActiveWeapon;     // Player's active weapon
        public PlayerStats Stats;       // Player's stats for the current match (kills/deaths/damage/etc)
        public PlayerHeartbeat Heartbeat; // Handles heartbeat effect variables
        public PlayerUiManager PlayerUi;  // References to player UI objects that must be toggled/updated
        public NetworkAnimator PlayerAnimator;  // Handles player animations on attack

        #region SyncVars
        [SyncVar]
        public int MatchId;

        [SyncVar]
        public int PlayerId;
        #endregion

        private InputMaster _playerInputActions;    // Reference to Player inputs for attacking
        private DdfcNetworkManager _networkManager; // Reference to NetworkManager's singleton instance
        
        // Hashes for animations names
        private readonly int _quickAtkAnimHash = Animator.StringToHash("ATK_Quick");
        private readonly int _heavyAtkAnimHash = Animator.StringToHash("ATK_Heavy");
        private readonly int _deathAnimHash = Animator.StringToHash("Death");
        private readonly int _takeDamageAnimHash = Animator.StringToHash("TakeDamage");

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            _playerInputActions = new InputMaster();

            // Assign attack methods to inupt action callback contexts
            _playerInputActions.Player.QuickAttack.performed += QuickAttack;
            _playerInputActions.Player.HeavyAttack.performed += HeavyAttack;
        }

        private void Start()
        {
            //_networkManager = FindObjectOfType<DdfcNetworkManager>();
            _networkManager = NetworkManager.singleton as DdfcNetworkManager;

            // Subscribe to the OnHealthZero event to handle player death
            Health.OnHealthZero += Die;
            // Subscribe to the OnHealthRemoved event to handle when player takes dmg
            Health.OnHealthRemoved += TakeDamage;

            // Name player objects in the hierarchy for ease of debugging
            if (isLocalPlayer)
            {
                name = $"Player [id = {PlayerId}]";
                PlayerUi.ToggleScoreUi(true);
            }
            else
            {
                var id = PlayerId == 0 ? 1 : 0;
                name = $"Player [id = {id}]";
            }
        }

        private void OnEnable()
        {
            _playerInputActions.Player.QuickAttack.Enable();
            _playerInputActions.Player.HeavyAttack.Enable();
        }

        private void OnDisable()
        {
            _playerInputActions.Player.QuickAttack.Disable();
            _playerInputActions.Player.HeavyAttack.Disable();
        }

        #region Input Action Callbacks

        public void QuickAttack(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer || !ActiveWeapon.CanAttack)
                return;

            // Play attack animation
            PlayerAnimator.SetTrigger(_quickAtkAnimHash);

            // Check if weapon collides with an enemy
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.TryGetComponent(out Player enemy))
            {
                CmdOnPerformAttack(enemy, ActiveWeapon.QuickAttack());
            }
        }

        public void HeavyAttack(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer || !ActiveWeapon.CanAttack)
                return;

            // Play attack animation
            PlayerAnimator.SetTrigger(_heavyAtkAnimHash);

            // Check if weapon collides with an enemy
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.TryGetComponent(out Player enemy))
            {
                CmdOnPerformAttack(enemy, ActiveWeapon.HeavyAttack());
            }
        }
        #endregion

        [Client]
        public void DisplayRoundOverUi(bool? isWinner)
        {
            // Prevent player input from interfering
            ToggleComponents(false);

            if (isLocalPlayer)
                PlayerUi.ToggleRoundOver(true, isWinner);
        }

        [Client]
        public void DisplayGameOverUi(bool? isWinner)
        {
            ToggleComponents(false);
            if (isLocalPlayer)
                PlayerUi.DisplayGameOver(isWinner);
        }

        [Client]
        public void UpdateScoreUi(uint localScore, uint remoteScore)
        {
            if (isLocalPlayer)
                PlayerUi.UpdateScoreUiValues(localScore, remoteScore);
        }

        /// <summary>Reset the player's health and set transform to new spawn point.</summary>
        /// <param name="spawnPoint"></param>
        public void ResetState(Transform spawnPoint)
        {
            // Prevent player input from interfering
            ToggleComponents(false);

            // Reset player health value and respawn location
            CmdOnResetState();
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            // Update current heart rate value
            Heartbeat.UpdateHeartRate();

            // Turn off round-over UI for local player
            if (isLocalPlayer)
                PlayerUi.ToggleRoundOver(false);

            // Re-enable player input/controls
            ToggleComponents(true);
        }

        /// <summary> Trigger coroutine that stops the client and returns player to the OfflineScene. </summary>
        public void LeaveMatch(bool? isWinner)
        {
            if (!isLocalPlayer)
                return;

            StartCoroutine(OnMatchEnded(isWinner));
        }

        private IEnumerator OnMatchEnded(bool? isWinner)
        {
            ToggleComponents(false);
            DisplayGameOverUi(isWinner);
            yield return new WaitForSeconds(5f);
            _networkManager.StopClient();
        }

        [ClientCallback]
        public void MatchInterrupted()
        {
            _networkManager.StopClient();
        }

        [ClientCallback]
        private void TakeDamage()
        {
            PlayerAnimator.SetTrigger(_takeDamageAnimHash);
        }

        private void Die()
        {
            // Disable player input and movement
            ToggleComponents(false);

            // TODO: trigger death animation
            PlayerAnimator.SetTrigger(_deathAnimHash);
            _networkManager.MatchManagers[MatchId].PlayerDeath(PlayerId);
        }

        /// <summary> Activates/Deactivates the Components, depending on whether isActive is true/false. </summary>
        /// <param name="isActive"></param>
        public void ToggleComponents(bool isActive)
        {
            if (isActive)
            {
                _playerInputActions.Enable();
            }
            else
            {
                _playerInputActions.Disable();
            }
            GetComponent<PlayerController>().enabled = isActive;
            GetComponent<CharacterController>().enabled = isActive;
        }

        void OnGUI()
        {
            if (isLocalPlayer)
            {
                // UI for displaying id
                GUI.Box(new Rect(10f, 10f, 100f, 25f), $"P{PlayerId}");
            }
        }

        public override string ToString()
        {
            var player = $"Player#{PlayerId} in Match#{MatchId}";
            return player;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        #region Commands

        [Command]
        private void CmdOnPerformAttack(Player target, int damageDealt)
        {
            // Send command to server to damage enemy player
            if (target.TryGetComponent(out HealthMetre health))
            {
                health.Remove(damageDealt);
            }
        }

        [Command]
        private void CmdOnResetState()
        {
            Health.ResetValue();
        }
        #endregion
    }
}
