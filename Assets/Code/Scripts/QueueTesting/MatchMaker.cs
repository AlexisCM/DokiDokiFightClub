using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class MatchMaker : NetworkBehaviour
    {

        // Singleton Instance
        public static MatchMaker Instance { get; private set; }

        public readonly SyncList<PlayerQueueIdentity> PlayerQueue = new(); // List of waiting players
        public readonly int PlayersPerMatch = 2; // Exact number of players needed to start a match

        private DdfcNetworkManager _networkManager;

        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        void Start()
        {
            DontDestroyOnLoad(Instance.gameObject);
            _networkManager = FindObjectOfType<DdfcNetworkManager>();
        }

        public void AddPlayerToQueue(NetworkConnectionToClient conn)
        {
            PlayerQueue.Add(conn.identity.gameObject.GetComponent<PlayerQueueIdentity>());
            Debug.Log($"# of players in Queue: {PlayerQueue.Count}");

            if (PlayerQueue.Count >= PlayersPerMatch)
            {
                List<PlayerQueueIdentity> matchPlayers = new();
                for (int i = 0; i < PlayersPerMatch; ++i)
                {
                    matchPlayers.Add(PlayerQueue[i]);
                }
                InitiateMatch(matchPlayers);
            }
        }

        void InitiateMatch(List<PlayerQueueIdentity> matchPlayers)
        {
            Debug.Log("MatchMaker.InitateMatch()");
            foreach (PlayerQueueIdentity player in matchPlayers)
            {
                // Remove players from queue before starting match
                RemovePlayerFromQueue(player);
                //NetworkIdentity playerIdentity = player.gameObject.GetComponent<NetworkIdentity>();
                //if (netIdentity == playerIdentity && isLocalPlayer)
                //    NetworkClient.AddPlayer();
            }
            _networkManager.AddPlayersToMatch(matchPlayers);
        }

        #region Server Commands
        //[Command]
        //public void CmdAddPlayerToQueue(NetworkConnectionToClient player)
        //{
        //    Debug.Log("MatchMaker.CmdAddPlayerToQueue");
        //    PlayerQueue.Add(player);

        //    if (PlayerQueue.Count >= PlayersPerMatch)
        //    {
        //        List<NetworkConnectionToClient> matchPlayers = new List<NetworkConnectionToClient>();
        //        for (int i = 0; i < PlayersPerMatch; ++i)
        //        {
        //            matchPlayers.Add(PlayerQueue[i]);
        //        }
        //        InitiateMatch(matchPlayers);
        //    }
        //}

        //[Command]
        public void RemovePlayerFromQueue(PlayerQueueIdentity player)
        {
            PlayerQueue.Remove(player);
            Debug.Log($"MatchMaker.RemovePlayerFromQueue. # players left: {PlayerQueue.Count}");
        }

        #endregion
    }
}
