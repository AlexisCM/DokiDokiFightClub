using UnityEngine;

public class Entity : MonoBehaviour
{

    public HealthMetre HealthMetre; // Reference to entity's max and current health

    public void TakeDamage(int damage)
    {
        HealthMetre.CurrentHealth -= damage;
        Debug.Log($"{this.name} took {damage} dmg! Health is now {HealthMetre.CurrentHealth}");
        if (HealthMetre.CurrentHealth <= 0)
            Die();
    }

    public virtual void Die()
    {
    }

    public void ResetState()
    {
        HealthMetre.Reset();
        // TODO: Reset location
    }
}
