using System.Collections.Generic;
using UnityEngine;

namespace DokiDokiFightClub
{
    /// <summary>
    /// Represents a single match that can be stored on the network.
    /// </summary>
    [System.Serializable]
    public class Match
    {
        public string MatchId;
        public bool IsJoinable;
        public List<GameObject> Players = new(); // TODO: Change to private to limit players

        public Match(string matchId, GameObject player)
        {
            MatchId = matchId;
            Players.Add(player);
            IsJoinable = true;
        }

        public Match() { }
    }
}
