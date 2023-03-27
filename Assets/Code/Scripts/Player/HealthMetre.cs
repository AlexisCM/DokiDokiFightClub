using Mirror;
using UnityEngine;

public class HealthMetre : NetworkBehaviour
{
    [SyncVar]
    public int CurrentHealth;
    public int MaxHealth;

    private void Start()
    {
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
    }

    public void Reset()
    {
        CurrentHealth = MaxHealth;
        // TODO: Update Player Health UI
    }
}
