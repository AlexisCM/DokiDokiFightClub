using Mirror;
using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class MatchMaker : NetworkBehaviour
    {
        public static MatchMaker Instance { get; private set; }

        private const int _serverMatchCapacity = 5; // Maximum number of simultaneous matches
        private const int _playersPerMatch = 2; // Exact number of players allowed per match

        private readonly SyncList<Match> _matches = new(); // List of ongoing matches
        private readonly SyncList<string> _matchIds = new(); // List of IDs for ongoing matches

        private readonly SyncList<NetworkPlayer> player = new(); // List of players looking for a match

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = this;
        }

        void Start()
        {
            DontDestroyOnLoad(this);
        }

        #region Custom Methods (non-tutorial)
        //public void AddPlayerToQueue(NetworkPlayer player)
        //{
        //    CmdAddPlayerToQueue(player);
        //}

        //public void RemovePlayerFromQueue(NetworkPlayer player)
        //{
        //    this.player.Remove(player);
        //}


        //private void CreateMatch(List<NetworkPlayer> players)
        //{
        //    Match match = new(GenerateMatchId(), players);
        //    _matches.Add(match);

        //    foreach (NetworkPlayer player in players)
        //    {
        //        player.JoinMatchLobby();
        //    }
        //}

        #endregion

        public string GenerateMatchId()
        {
            string id = string.Empty;

            for (int i = 0; i < 5; ++i)
            {
                int random = UnityEngine.Random.Range(0, 36);
                if (random < 26)
                    id += (char)(random + 65); // Uppercase Letter
                else
                    id += (random - 26).ToString(); // Number
            }
            Debug.Log($"Generated Match ID: {id}");
            return id;
        }

        public bool HostMatch(string matchId, GameObject player)
        {
            if (_matchIds.Contains(matchId))
            {
                Debug.Log("Match ID already exists. Failed to Host Match.");
                return false;
            }

            _matchIds.Add(matchId);
            _matches.Add(new Match(matchId, player));
            Debug.Log("Match generated");
            return true;
        }

        public bool JoinMatch(string matchId, GameObject player)
        {
            if (!_matchIds.Contains(matchId))
            {
                Debug.Log("Match ID does not exist. Failed to Join Match.");
                return false;
            }

            for (int i = 0; i < _matches.Count; ++i)
            {
                if (_matches[i].MatchId == matchId)
                {
                    _matches[i].Players.Add(player);
                    break;
                }
            }
            Debug.Log("Joined Match");
            return true;
        }

        /// <summary>
        /// Finds an available match and returns its ID.
        /// </summary>
        /// <returns></returns>
        public string FindMatch()
        {
            for (int i = 0; i < _matches.Count; ++i)
            {
                if (_matches[i].IsJoinable)
                    return _matches[i].MatchId;
            }
            return string.Empty;
        }

        public void StartMatch()
        {
        }

        #region Server Commands
        //[Command]
        //public void CmdAddPlayerToQueue(NetworkPlayer player)
        //{
        //    // Check for server load
        //    if (_matches.Count >= _serverMatchCapacity)
        //    {
        //        // Server at match capacity
        //        // TODO: Display UI to inform player of status
        //        return;
        //    }

        //    this.player.Add(player);
        //    Debug.Log($"Added player {player} to the queue");

        //    // Not enough players to create a match
        //    if (this.player.Count < _playersPerMatch)
        //        return;

        //    // Add players to a new match, then remove them from queue
        //    List<NetworkPlayer> matchPlayers = new();
        //    for (int i = 0; i < _playersPerMatch; i++)
        //    {
        //        matchPlayers.Add(this.player[i]);
        //        this.player.RemoveAt(i);
        //    }

        //    CreateMatch(matchPlayers);
        //}
        #endregion
    }

    public static class MatchIdExtension
    {
        public static Guid ToGuid(this string id)
        {
            MD5CryptoServiceProvider provider = new();
            byte[] inputBytes = Encoding.Default.GetBytes(id);
            byte[] hashBytes = provider.ComputeHash(inputBytes);
            return new Guid(hashBytes);
        }
    }
}
