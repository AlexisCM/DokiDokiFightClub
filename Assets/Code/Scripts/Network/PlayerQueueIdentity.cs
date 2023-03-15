using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class PlayerQueueIdentity : NetworkBehaviour
    {
        public bool IsMatchMade { get; private set; }

        public override void OnStartClient()
        {
            base.OnStartClient();
            IsMatchMade = false;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            MatchMaker.Instance.RemovePlayerFromQueue(this);
        }
    }
}
