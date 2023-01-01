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

    public void QuickAttack()
    {
        // TODO: Add attack speed delay. Coroutine?

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.transform.tag == "Enemy")
        {
                Entity enemy = hit.transform.gameObject.GetComponent<Entity>();
                enemy.TakeDamage(BaseDamage);
        }
        else
        {
            Debug.Log("whiff");
        }
    }

    public void HeavyAttack()
    {

    }

    void OnDrawGizmosSelected()
    {
        // Draws a 5 unit long red line in front of the object
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)));
    }
}
