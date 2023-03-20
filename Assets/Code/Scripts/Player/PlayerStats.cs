using UnityEngine;

namespace DokiDokiFightClub
{
    public class PlayerStats : MonoBehaviour
    {
        public int Kills { get; private set; }
        public int Deaths { get; private set; }
        public int DamageDealt { get; private set; }

        public int DamageTaken { get; private set; }

        private void Awake()
        {
            Kills = 0;
            Deaths = 0;
            DamageDealt = 0;
        }

        public void AddKill()
        {
            ++Kills;
        }

        public void AddDeath()
        {
            ++Deaths;
        }

        public void AddDamageDealt(int damage)
        {
            DamageDealt += damage;
        }

        public void AddDamageTaken(int damage)
        {
            DamageTaken += damage;
        }
    }
}
