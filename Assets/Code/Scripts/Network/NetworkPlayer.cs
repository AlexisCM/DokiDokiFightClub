using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{

    public class NetworkPlayer : NetworkRoomPlayer
    {
        public static NetworkPlayer LocalPlayer;

        [SyncVar] public string MatchId;

        private NetworkMatch _networkMatch;

        public new void Start()
        {
            base.Start();
            _networkMatch = GetComponent<NetworkMatch>();
            
            if (isLocalPlayer)
            {
                LocalPlayer = this;
            }

        }

        //public void QueueForMatch()
        //{
        //    MatchMaker.Instance.AddPlayerToQueue(this);
        //}

        //public void CancelQueue()
        //{
        //    MatchMaker.Instance.RemovePlayerFromQueue(this);
        //}

        #region Host Match
        public void HostMatch()
        {
            string matchId = MatchMaker.Instance.GenerateMatchId();
            CmdHostMatch(matchId);
        }

        [Command]
        void CmdHostMatch(string matchId)
        {
            MatchId = matchId;

            bool isSuccessful = false;
            if (MatchMaker.Instance.HostMatch(matchId, gameObject))
            {
                Debug.Log("<color=green>Match host successful</color>");
                _networkMatch.matchId = matchId.ToGuid();
                isSuccessful = true;
            }
            else
            {
                Debug.Log("<color=red>Match host failed</color>");
            }

            TargetHostMatch(isSuccessful, matchId);
        }

        /// <summary>
        /// Inform client on status of host. Allows client to perform necessary UI methods.
        /// </summary>
        /// <param name="isSuccessful"></param>
        /// <param name="matchId"></param>
        [TargetRpc]
        void TargetHostMatch(bool isSuccessful, string matchId)
        {
            MatchId = matchId;
        }
        #endregion

        #region Join Match

        public void JoinMatch()
        {
            string matchId = MatchMaker.Instance.FindMatch();
            if (matchId.Equals(string.Empty))
            {
                Debug.LogError("Failed to find a match to join.");
                return;
            }
            CmdJoinMatch(matchId);
        }

        [Command]
        void CmdJoinMatch(string matchId)
        {
            bool isSuccessful = false;
            if (MatchMaker.Instance.JoinMatch(matchId, gameObject))
            {
                Debug.Log("<color=green>Match host successful</color>");
                _networkMatch.matchId = matchId.ToGuid();
                isSuccessful = true;
            }
            else
            {
                Debug.Log("<color=red>Match host failed</color>");
            }

            TargetJoinMatch(isSuccessful, matchId);
        }

        [TargetRpc]
        void TargetJoinMatch(bool isSuccessful, string matchId)
        {
            MatchId = matchId;
        }

        #endregion

        #region Start Match
        public void StartMatch()
        {
            CmdStartMatch();
        }

        [Command]
        void CmdStartMatch()
        {
            MatchMaker.Instance.StartMatch();
            Debug.Log("<color=green>Starting Match</color>");
        }

        [ClientRpc]
        void RpcStartMatch()
        {
            // Additively load game scene
            // Or send matched players to new rooms
        }
        #endregion
    }
}
