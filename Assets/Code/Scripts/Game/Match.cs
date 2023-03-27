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
        private readonly List<Player> _matchPlayers; // Represents players AFTER GameObjects are spawned in game scene

        public Match(int matchId, List<PlayerQueueIdentity> players)
        {
            MatchId = matchId;
            CanStart = false;
            QueuedPlayers = players;
            _matchPlayers = new();
        }

        // Default constructor
        public Match() { }

        public void AddMatchPlayer(Player player)
        {
            _matchPlayers.Add(player);
            if (_matchPlayers.Count == 2)
                CanStart = true;
        }

        public List<Player> GetPlayerObjects()
        {
            return _matchPlayers;
        }
    }
}
