using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class PlayerQueueIdentity : NetworkBehaviour
    {
        public NetworkConnectionToClient NetConnToClient { get; private set; }
        public bool IsMatchMade { get; private set; }

        public override void OnStartClient()
        {
            base.OnStartClient();
            NetConnToClient = connectionToClient;
            IsMatchMade = false;
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            MatchMaker.Instance.RemovePlayerFromQueue(this);
        }
    }
}
