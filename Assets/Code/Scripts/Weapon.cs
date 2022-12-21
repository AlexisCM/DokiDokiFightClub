using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int BaseDamage;
    public float BaseAttackSpeed;

    private void Start()
    {
        BaseDamage = 50;
        BaseAttackSpeed = 1f;
    }
}
