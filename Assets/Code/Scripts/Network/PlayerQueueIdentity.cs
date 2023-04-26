using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class PlayerQueueIdentity : NetworkBehaviour
    {
        public int SpawnIndex;

        public override void OnStopClient()
        {
            base.OnStopClient();
            MatchMaker.Instance.RemovePlayerFromQueue(this);
        }
    }
}
