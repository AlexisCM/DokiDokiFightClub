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

        /// <summary>
        /// Randomly generate an index between 0 (inclusive) and the number of players per match (exclusive).
        /// </summary>
        /// <returns>Player's spawn index</returns>
        int GenerateSpawnIndex()
        {
            return Random.Range(0, PlayersPerMatch);
        }

        /// <summary>
        /// Returns the alternate index when the opposing player's spawn has already been assigned.
        /// </summary>
        /// <param name="opposingPlayerIndex">Previously assigned spawn index of opposing player</param>
        /// <returns>This player's spawn index</returns>
        int GenerateSpawnIndex(int opposingPlayerIndex)
        {
            return (opposingPlayerIndex == 0) ? 1 : 0;
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
                InitiateMatch();
            }
        }

        void InitiateMatch()
        {
            List<PlayerQueueIdentity> matchPlayers = new();
            // Generate random spawn index for first player
            var spawnIndex = GenerateSpawnIndex();

            for (int i = 0; i < PlayersPerMatch; ++i)
            {
                var player = PlayerQueue[i];
                player.SpawnIndex = spawnIndex;
                matchPlayers.Add(player);

                // Generate alternate spawn index for next player
                spawnIndex = GenerateSpawnIndex(player.SpawnIndex);
            }

            // Remove players from queue before starting match
            foreach (PlayerQueueIdentity player in matchPlayers)
            {
                RemovePlayerFromQueue(player);
            }

            // Create the match
            Match match = new(GetAvailableMatchId(), matchPlayers);
            Matches.Add(match);
            // Add players to match over the network
            _networkManager.AddPlayersToMatchScene(match);
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
}
