using System.Collections;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class Weapon : MonoBehaviour
    {
        public int BaseDamage;
        public float BaseAttackSpeed;
        
        public bool CanAttack { get; private set; }

        private float _heavyAtkDmgMulitplier;

        private void Start()
        {
            CanAttack = true;
            BaseDamage = 40;
            BaseAttackSpeed = 1f;
            _heavyAtkDmgMulitplier = 1.75f;
        }
        
        /// <summary>Starts a coroutine to apply attack speed delay between consecutive attacks.</summary>
        /// <returns>Weapon's base damage value.</returns>
        public int QuickAttack()
        {
            StartCoroutine(PerformAttack(BaseAttackSpeed));
            return BaseDamage;
        }

        IEnumerator PerformAttack(float attackSpeed)
        {
            // Prevent player from performing more attacks until this one finishes
            CanAttack = false;
            yield return new WaitForSeconds(attackSpeed);
            CanAttack = true;
        }

        /// <summary>Starts a coroutine to apply attack speed delay between consecutive attacks.</summary>
        /// <returns>Weapon's base damage multiplied by the heavy attack damage modifier.</returns>
        public int HeavyAttack()
        {
            var heavyAtkDmg = BaseDamage * _heavyAtkDmgMulitplier;
            StartCoroutine(PerformAttack(BaseAttackSpeed * 2f));
            return (int) heavyAtkDmg;
        }
    }
}
