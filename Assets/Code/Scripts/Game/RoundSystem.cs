using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    [RequireComponent(typeof(Animator))]
    public class RoundSystem : NetworkBehaviour
    {
        [SerializeField]
        Animator _animator;

        public void CountdownEnded()
        {
            _animator.enabled = false;
        }

        [ServerCallback]
        public void StartCountdown()
        {
            RpcStartCountdown();
        }

        [ClientRpc]
        private void RpcStartCountdown()
        {
            _animator.enabled = true;
        }
    }
}
