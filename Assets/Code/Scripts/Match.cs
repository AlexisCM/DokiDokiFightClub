using System.Collections.Generic;
using UnityEngine;

namespace DokiDokiFightClub
{
    [System.Serializable]
    public class Match
    {
        public int MatchId; // Match identifier
        public bool CanStart; // Indicates that GameManager can start the match in the subscene
        public List<PlayerQueueIdentity> QueuedPlayers; // Represents players BEFORE GameObjects are spawned in game scene
        private readonly List<GameObject> _matchPlayers; // Represents players AFTER GameObjects are spawned in game scene
        public GameManager GameManager;

        public Match(int matchId, List<PlayerQueueIdentity> players)
        {
            MatchId = matchId;
            CanStart = false;
            QueuedPlayers = players;
            _matchPlayers = new();
        }

        // Default constructor
        public Match() { }

        public void AddMatchPlayer(GameObject player)
        {
            _matchPlayers.Add(player);
            if (_matchPlayers.Count == 2)
                CanStart = true;
        }

        public List<GameObject> GetPlayerObjects()
        {
            return _matchPlayers;
        }
    }
}
