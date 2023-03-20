using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DokiDokiFightClub
{
    public class Player : NetworkBehaviour
    {
        public PlayerInput PlayerInput; // Handles player inputs (KMB or Gamepad)
        public Weapon ActiveWeapon;     // Player's active weapon
        public PlayerStats Stats;       // Player's stats for the current match (kills/deaths/damage/etc)
        public GameManager MatchMgrInstance; // Instance of the GameManager attached to this player's match
        public int ClientMatchIndex = -1;

        #region SyncVars
        [SyncVar]
        public int MatchId;

        [SyncVar]
        public int PlayerId;

        [SyncVar]
        public uint Score;

        [SyncVar]
        private int _currentHealth;

        #endregion


        InputMaster _playerInputActions; // Reference to Player inputs for attacking

        [SerializeField]
        private int _maxHealth;

        private DdfcNetworkManager _networkManager;

        private void Awake()
        {
            PlayerInput = GetComponent<PlayerInput>();
            _playerInputActions = new InputMaster();

            // Assign attack methods to inupt action callback contexts
            _playerInputActions.Player.QuickAttack.performed += QuickAttack;
            _playerInputActions.Player.HeavyAttack.performed += HeavyAttack;
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
            // Play attack animation
            // Check if weapon collides with an enemy
            Debug.Log("Quick attack!");

            if (isLocalPlayer)
                ActiveWeapon.QuickAttack(this);
        }

        public void HeavyAttack(InputAction.CallbackContext context)
        {
            // Play attack animation
            // Check if weapon collides with an enemy
            Debug.Log("HEAVY attack!");
        }
        #endregion

        public void TakeDamage(int damage, int attackerId)
        {
            _currentHealth -= damage;
            Stats.AddDamageTaken(damage);
            Debug.Log($"{name} took {damage} dmg! Health is now {_currentHealth}");

            if (_currentHealth <= 0)
                Die(attackerId);
        }

        private void Die(int attackerId)
        {
            Stats.AddDeath();
            Debug.Log($"MatchMgr? {MatchMgrInstance == null}");
            Player attackingPlayer = MatchMgrInstance.GetPlayer(attackerId);
            attackingPlayer.Stats.AddKill();

            // Notify GameManager that the player died, so the round can end
            MatchMgrInstance.PlayerDeath(gameObject);
        }

        public void ResetState()
        {
            _currentHealth = 0;
        }

        void OnGUI()
        {
            if (!isServerOnly && !isLocalPlayer && ClientMatchIndex < 0)
                //ClientMatchIndex = NetworkClient.connection.identity.GetComponent<Player>().MatchIndex;

            if (isLocalPlayer || MatchId == ClientMatchIndex)
            {
                // UI for displaying score
                //GUI.Box(new Rect(10f + (ScoreIndex * 110), 10f, 100f, 25f), $"P{PlayerNumber}: {Score}");
            }
        }

        #region Commands

        #endregion
    }
}
