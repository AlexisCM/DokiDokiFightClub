using Mirror;
using System.Collections;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class Round : NetworkBehaviour
    {
        [SyncVar]
        public float CurrentTime; // Current time left in the round

        [SyncVar]
        public bool IsOngoing; // Flag for if the round is still in progress

        private const float _maxRoundDuration = 60f;     // Maximum duration per round in seconds

        void Start()
        {
            // Ensure initial round variables are set properly
            ResetRound();
        }

        public void StartRound()
        {
            IsOngoing = true;
            StartCoroutine(nameof(StartTimer));
        }

        public void ResetRound()
        {
            StopCoroutine(nameof(StartTimer));
            CurrentTime = _maxRoundDuration;
            IsOngoing = false;
        }

        IEnumerator StartTimer()
        {
            while (CurrentTime > 0f)
            {
                yield return new WaitForSeconds(1f);
                --CurrentTime;
            }
        }
    }
}
