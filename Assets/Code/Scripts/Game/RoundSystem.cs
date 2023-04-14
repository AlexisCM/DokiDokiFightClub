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
        public void StartRound()
        {
            RpcStartRound();
        }

        [ClientRpc]
        private void RpcStartRound()
        {
            _animator.enabled = true;
        }
    }
}
