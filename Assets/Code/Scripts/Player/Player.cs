using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public PlayerInput PlayerInput; // Handles player inputs (KMB or Gamepad)

    public HealthMetre HealthMetre; // Reference to player's max and current health
    public Weapon ActiveWeapon;     // Player's active weapon

    InputMaster _playerInputActions; // Reference to Player inputs for attacking

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

    public void QuickAttack(InputAction.CallbackContext context)
    {
        // Play attack animation
        // Check if weapon collides with an enemy
        Debug.Log("Quick attack!");
    }

    public void HeavyAttack(InputAction.CallbackContext context)
    {
        // Play attack animation
        // Check if weapon collides with an enemy
        Debug.Log("HEAVY attack!");
    }

    public void TakeDamage(int damage)
    {
        HealthMetre.CurrentHealth -= damage;
        Debug.Log($"Took {damage} dmg! Health is now {HealthMetre.CurrentHealth}");
        if (HealthMetre.CurrentHealth <= 0)
            Die();
    }

    public void Die()
    {
        // Notify GameManager that the player died, so the round can end
        GameManager.Instance.PlayerDeath(gameObject);
    }

    public void ResetState()
    {
        HealthMetre.Reset();
        // TODO: Reset location
    }
}
