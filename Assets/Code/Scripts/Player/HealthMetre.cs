using UnityEngine;

public class HealthMetre : MonoBehaviour
{
    public int MaxHealth;
    public int CurrentHealth;

    private void Start()
    {
        MaxHealth = 100;
        CurrentHealth = MaxHealth;
    }

    public void Reset()
    {
        CurrentHealth = MaxHealth;
    }
}
