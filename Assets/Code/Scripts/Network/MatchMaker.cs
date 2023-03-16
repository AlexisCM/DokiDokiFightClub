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
        public readonly SyncList<Match> Matches = new(); // List of matches
        public readonly int PlayersPerMatch = 2; // Exact number of players needed to start a match

        private DdfcNetworkManager _networkManager;
        private int _lastUsedMatch; // Number of matches created thus far; used to generate match ids

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
            _lastUsedMatch = 0;
        }

        int GetAvailableMatchId()
        {
            // Assumes there is room for at least 1 new match
            return _lastUsedMatch++ % _networkManager.MatchInstances;
        }

        bool IsAvailableMatch()
        {
            return Matches.Count < _networkManager.MatchInstances;
        }

        public void AddPlayerToQueue(NetworkConnectionToClient conn)
        {
            PlayerQueue.Add(conn.identity.gameObject.GetComponent<PlayerQueueIdentity>());
            Debug.Log($"# of players in Queue: {PlayerQueue.Count}");

            if (PlayerQueue.Count >= PlayersPerMatch && IsAvailableMatch())
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
            }
            Match match = new(GetAvailableMatchId(), matchPlayers);
            Matches.Add(match);
            _networkManager.AddPlayersToMatch(match);
        }

        public void RemovePlayerFromQueue(PlayerQueueIdentity player)
        {
            PlayerQueue.Remove(player);
            Debug.Log($"MatchMaker.RemovePlayerFromQueue. # players left: {PlayerQueue.Count}");
        }

        public void RemoveMatch(int matchId)
        {
            for (int i = 0; i < Matches.Count; ++i)
            {
                if (Matches[i].MatchId == matchId)
                {
                    Matches.RemoveAt(i);
                    break;
                }
            }
        }
    }

    [System.Serializable]
    public class Match
    {
        public int MatchId;
        public List<PlayerQueueIdentity> Players;

        public Match(int matchId, List<PlayerQueueIdentity> players)
        {
            MatchId = matchId;
            Players = players;
        }

        // Default constructor
        public Match() { }
    }
}
