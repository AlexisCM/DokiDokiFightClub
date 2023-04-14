using Mirror;
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
        public PlayerHeartbeat Heartbeat;
        public PlayerUiManager PlayerUi;

        #region SyncVars
        [SyncVar]
        public int MatchId;

        [SyncVar]
        public int PlayerId;

        [SyncVar]
        public uint Score;
        #endregion

        private InputMaster _playerInputActions;    // Reference to Player inputs for attacking
        private DdfcNetworkManager _networkManager; // Reference to NetworkManager's singleton instance

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

            // Name player objects in the hierarchy for ease of debugging
            if (isLocalPlayer)
            {
                name = $"Player [id = {PlayerId}]";
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

        [Client]
        public void QuickAttack(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer || !ActiveWeapon.CanAttack)
                return;

            // TODO: Play attack animation

            // Check if weapon collides with an enemy
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.TryGetComponent(out Player enemy))
            {
                Debug.Log($"Quick ATK!");
                CmdOnPerformAttack(enemy, ActiveWeapon.QuickAttack());
            }
            else
            {
                Debug.Log("whiff");
            }
        }

        [Client]
        public void HeavyAttack(InputAction.CallbackContext context)
        {
            if (!isLocalPlayer || !ActiveWeapon.CanAttack)
                return;
            // Play attack animation
            // Check if weapon collides with an enemy
            // Check if weapon collides with an enemy
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.TryGetComponent(out Player enemy))
            {
                Debug.Log($"Heavy ATK!");
                CmdOnPerformAttack(enemy, ActiveWeapon.HeavyAttack());
            }
            else
            {
                Debug.Log("whiff");
            }
        }
        #endregion

        [Client]
        public void DisplayRoundOverUi(bool isWinner)
        {
            // Prevent player input from interfering
            ToggleComponents(false);

            if (isLocalPlayer)
                PlayerUi.ToggleRoundOver(true, isWinner);
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

        private void Die()
        {
            // Disable player input and movement
            ToggleComponents(false);

            // TODO: trigger death animation
            // TODO: display round winner/loser (might be handled as rpc on MatchManager
            // TODO: update player stats
            _networkManager.MatchManagers[MatchId].PlayerDeath(PlayerId);
        }

        void OnGUI()
        {
            if (isLocalPlayer)
            {
                // UI for displaying score
                GUI.Box(new Rect(10f, 10f, 100f, 25f), $"P{PlayerId}");
            }
        }

        /// <summary> Activates/Deactivates the Components, depending on whether isActive is true/false. </summary>
        /// <param name="isActive"></param>
        private void ToggleComponents(bool isActive)
        {
            PlayerInput.enabled = isActive;
            GetComponent<PlayerController>().enabled = isActive;
            GetComponent<CharacterController>().enabled = isActive;
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
