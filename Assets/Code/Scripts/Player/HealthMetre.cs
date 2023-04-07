using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class HealthMetre : NetworkBehaviour
    {
        private const int _maxHealth = 100;

        [SyncVar(hook = nameof(HandleHealthUpdated))]
        private int _health;

        // Delegate to update subscribers and handle behaviour when health reaches zero
        public delegate void HealthZeroHandler();
        public event HealthZeroHandler OnHealthZero;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _health = _maxHealth;
        }

        private void HandleHealthUpdated(int oldValue, int newValue)
        {
            // TODO: Handle health UI

            if (isLocalPlayer)
                Debug.Log($"New health value: {newValue}");
        }

        [Server]
        public void Add(int value)
        {
            // Ensure negative values cannot be assigned
            value = Mathf.Max(value, 0);
            // Ensure new health value cannot exceed Max Health
            _health = Mathf.Min(_health + value, _maxHealth);
        }

        [Server]
        public void Remove(int value)
        {
            // Ensure health cannot become negative
            value = Mathf.Max(value, 0);
            _health = Mathf.Max(_health - value, 0);

            if (_health == 0)
                OnHealthZero?.Invoke();
        }

        [Server]
        public void ResetValue()
        {
            _health = _maxHealth;
        }
    }

}