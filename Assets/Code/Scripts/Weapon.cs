using System.Collections;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class Weapon : MonoBehaviour
    {
        public int BaseDamage;
        public float BaseAttackSpeed;
        
        public bool CanAttack { get; private set; }

        private void Start()
        {
            CanAttack = true;
            BaseDamage = 50;
            BaseAttackSpeed = 1f;
        }

        public int QuickAttack()
        {
            // TODO: Add attack speed delay. Coroutine?
            StartCoroutine(PerformAttack(BaseAttackSpeed));
            return BaseDamage;

            //Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            //if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.TryGetComponent(out Player enemy))
            //{
            //    enemy.TakeDamage(thisPlayer, BaseDamage);
            //    thisPlayer.Stats.AddDamageDealt(BaseDamage);
            //}
            //else
            //{
            //    Debug.Log("whiff");
            //}
        }

        IEnumerator PerformAttack(float attackSpeed)
        {
            // Prevent player from performing more attacks until this one finishes
            CanAttack = false;
            yield return new WaitForSeconds(attackSpeed);
            CanAttack = true;
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
}
