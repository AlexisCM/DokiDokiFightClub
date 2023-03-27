using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DokiDokiFightClub
{
    public class Player : NetworkBehaviour
    {
        public PlayerInput PlayerInput; // Handles player inputs (KMB or Gamepad)
        public HealthMetre HealthMetre; // Handles player's Max and Current Health
        public Weapon ActiveWeapon;     // Player's active weapon
        public PlayerStats Stats;       // Player's stats for the current match (kills/deaths/damage/etc)

        #region SyncVars
        [SyncVar]
        public int MatchId;

        [SyncVar]
        public int PlayerId;

        [SyncVar]
        public uint Score;

        #endregion


        InputMaster _playerInputActions; // Reference to Player inputs for attacking

        //[SerializeField]
        //List<Behaviour> ComponentsToDisable; // Components to disable for non-local players

        private DdfcNetworkManager _networkManager;

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
            _networkManager = FindObjectOfType<DdfcNetworkManager>();
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
            // Play attack animation
            // Check if weapon collides with an enemy
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.TryGetComponent(out Player enemy) && ActiveWeapon.CanAttack)
            {
                Debug.Log($"Quick ATK!");
                CmdPlayerDamaged(enemy.PlayerId, ActiveWeapon.QuickAttack(), PlayerId);
            }
            else
            {
                Debug.Log("whiff");
            }
        }

        [Client]
        public void HeavyAttack(InputAction.CallbackContext context)
        {
            // Play attack animation
            // Check if weapon collides with an enemy
            Debug.Log("HEAVY attack!");
        }
        #endregion

        public void ResetState(Transform spawnPoint)
        {
            ToggleComponents(false);
            HealthMetre.Reset();
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            ToggleComponents(true);
        }

        private void Die(int sourceId)
        {
            Stats.AddDeath();
            _networkManager.MatchManagers[MatchId].Players[sourceId].Stats.AddKill();

            CmdOnPlayerDeath(PlayerId, sourceId);
        }

        void OnGUI()
        {
            if (isLocalPlayer)
            {
                // UI for displaying score
                GUI.Box(new Rect(10f, 10f, 100f, 25f), $"P{PlayerId}");
            }
        }

        /// <summary>
        /// Activates/Deactivates the Components, depending on whether isActive is true/false.
        /// </summary>
        /// <param name="isActive"></param>
        private void ToggleComponents(bool isActive)
        {
            //for (int i = 0; i < ComponentsToDisable.Count; ++i)
            //    ComponentsToDisable[i].enabled = isLocalPlayer && isActive;

            GetComponent<PlayerController>().enabled = isActive;
            GetComponent<CharacterController>().enabled = isActive;
        }

        #region Commands

        [ClientRpc]
        public void RpcTakeDamage(int damage, int sourceId)
        {
            HealthMetre.CurrentHealth -= damage;
            if (HealthMetre.CurrentHealth <= 0)
                Die(sourceId);
        }

        [Command]
        private void CmdPlayerDamaged(int targetId, int damage, int sourceId)
        {
            var targetPlayer = _networkManager.MatchManagers[MatchId].Players[targetId];
            Debug.Log($"targetPlayer isNull? {targetPlayer==null}");
            Debug.Log($"Player#{targetId} took {damage} DMG!");
            targetPlayer.RpcTakeDamage(damage, sourceId);
        }

        [Command]
        private void CmdOnPlayerDeath(int targetId, int sourceId)
        {
            // Notify GameManager that the player died, so the round can end
            _networkManager.MatchManagers[MatchId].PlayerDeath(targetId, sourceId);
        }
        #endregion
    }
}
