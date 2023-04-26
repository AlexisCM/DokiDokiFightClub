using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace DokiDokiFightClub
{
    public class HealthMetre : NetworkBehaviour
    {
        // Delegate to update subscribers and handle behaviour when health reaches zero
        public delegate void HealthZeroHandler();
        public event HealthZeroHandler OnHealthZero;

        // Delegate to handle behaviour when health is removed
        public delegate void HealthRemovedHandler();
        public event HealthRemovedHandler OnHealthRemoved;

        private const int _maxHealth = 100;

        [SyncVar(hook = nameof(HandleHealthUpdated))]
        private int _health;

        [SerializeField]
        private GameObject _healthMetreUI; // The game object that contains the UI for the health bar

        [SerializeField]
        private Slider _healthSlider; // Slider that controls the health bar on screen

        [SerializeField]
        private Image _healthFill; // The actual image to change when the health value updates

        [SerializeField]
        private Gradient _damageGradient; // Changes colour of the slider based on its current value

        public override void OnStartServer()
        {
            base.OnStartServer();
            _health = _maxHealth;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            InitializeUI();
        }

        private void InitializeUI()
        {
            _healthMetreUI.SetActive(true);

            _healthSlider.minValue = 0;
            _healthSlider.maxValue = _maxHealth;
            _healthSlider.wholeNumbers = true;

            _healthSlider.value = _maxHealth;

            // Set color of gradient to the one assigned for 100%
            _healthFill.color = _damageGradient.Evaluate(1f);
        }

        /// <summary>Callback to handle UI when the health value is changed.</summary>
        public void HandleHealthUpdated(int oldValue, int newValue)
        {
            if (!isLocalPlayer)
                return;
            _healthSlider.value = newValue;
            _healthFill.color = _damageGradient.Evaluate(_healthSlider.normalizedValue);
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
            // Notify subscribers of removed health
            OnHealthRemoved?.Invoke();

            // Notify subscribers of "death"
            if (_health == 0)
                OnHealthZero?.Invoke();
        }

        [Server]
        public void ResetValue()
        {
            _health = _maxHealth;
            // Don't need to change UI here because of SyncVar hook
        }
    }

}