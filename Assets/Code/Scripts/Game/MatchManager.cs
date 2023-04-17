using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

namespace DokiDokiFightClub
{
    [RequireComponent(typeof(Round))]
    public class MatchManager : NetworkBehaviour
    {
        public int MatchInstanceId { get; private set; }
        public bool WaitingForPlayers = true;

        public List<Player> Players;

        internal Round Round { get; private set; } // Keeps track of current round's state

        private DdfcNetworkManager _networkManager;
        private const int _maxRounds = 3;   // Maximum number of rounds played per match
        private const float _timeBetweenRounds = 5f;

        [SerializeField]
        private TMP_Text _timerTextObj;

        [SyncVar]
        private int _roundsPlayed = 0;      // Current number of rounds played/completed

        [SyncVar]
        private bool _isDoingWork = false;  // Flag for methods called in the update loop

        /// <summary> Key, Value is equal to PlayerId, Score </summary>
        private readonly SyncDictionary<int, int> _playerScoreKeeper = new();

        public void SetMatchInstanceId(int matchInstanceId)
        {
            MatchInstanceId = matchInstanceId;
        }

        private void Start()
        {
            _networkManager = NetworkManager.singleton as DdfcNetworkManager;
            Round = GetComponent<Round>();
        }

        public void StartMatch(List<Player> players)
        {
            Players = players;
            WaitingForPlayers = false;
            Round.StartRound();
        }

        private void Update()
        {
            if (WaitingForPlayers || _isDoingWork)
                return;

            // Check if round ended
            if (Round.CurrentTime <= 0)
            {
                StartCoroutine(RoundEnded(null));
            }
            else
            {
                RpcUpdateTimer(Round.CurrentTime);
            }

            // Round is over
            if (!Round.IsOngoing && _roundsPlayed < _maxRounds)
            {
                // Start new round if maximum hasn't been reached
                Debug.Log($"Rounds Played: {_roundsPlayed}");
                Round.StartRound();
            }
            else if (!Round.IsOngoing && _roundsPlayed == _maxRounds)
            {
                // Maximum rounds were played; end the game
                StartCoroutine(nameof(MatchEnded));
            }
        }

        public void PlayerDeath(int deadPlayerId)
        {
            // TODO: Keep track of players' round wins/losses
            RpcLogMessage($"<color=red>Player#{deadPlayerId} was KILLED!</color>");
            StartCoroutine(RoundEnded(deadPlayerId));
        }

        private IEnumerator RoundEnded(int? deadPlayerId)
        {
            _isDoingWork = true;
            Round.IsOngoing = false;
            ++_roundsPlayed;

            // Display Round Victory/Defeat UI
            foreach (var player in Players)
            {
                if (deadPlayerId != null && deadPlayerId != player.PlayerId)
                {
                    // Update score of winning player
                    if (_playerScoreKeeper.TryGetValue(player.PlayerId, out int currentScore))
                    {
                        _playerScoreKeeper[player.PlayerId] = currentScore + 1;
                    }
                    else
                    {
                        _playerScoreKeeper.Add(player.PlayerId, 1);
                    }

                    TargetDisplayRoundOver(player.connectionToClient, true);
                }
                else if (deadPlayerId != null && deadPlayerId == player.PlayerId)
                {
                    TargetDisplayRoundOver(player.connectionToClient, false);
                }
                else
                {
                    TargetDisplayRoundOver(player.connectionToClient, null);
                }
            }

            // Delay before new round begins; allow UI time to be displayed
            yield return new WaitForSeconds(_timeBetweenRounds);

            // Reset player state and remove Round Over UI
            foreach (var player in Players)
            {
                TargetResetPlayerState(player.connectionToClient);
            }

            Round.ResetRound();
            _isDoingWork = false;
        }

        private IEnumerator MatchEnded()
        {
            // TODO: Determine the match winner
            StopAllCoroutines();
            // TODO: Transition player to match summary scene
            WaitingForPlayers = true;
            // Disconnect player clients, which will automatically send them back to the offline screen
            MatchMaker.Instance.RemoveMatch(MatchInstanceId);
            foreach (var player in Players)
            {
                TargetOnMatchEnded(player.connectionToClient);
            }

            yield return new WaitForEndOfFrame();

            Players.Clear();
        }

        /// <summary> If the number of rounds played is odd, the player must swap spawn positions. </summary>
        private int GetPlayerSpawnIndex(int playerId)
        {
            int index;
            if (_roundsPlayed % 2 == 0)
            {
                index = playerId;
            }
            else
            {
                index = (playerId == 0) ? 1 : 0;
            }
            return index;
        }

        private int GetRemotePlayerId(int localPlayerId)
        {
            return localPlayerId == 0 ? 1 : 0;
        }

        private int GetPlayerScore(int key)
        {
            return _playerScoreKeeper.TryGetValue(key, out int score) ? score : 0;
        }

        #region Remote Prodecure Calls
        [ClientRpc]
        private void RpcLogMessage(string msg)
        {
            Debug.Log(msg);
        }

        [ClientRpc]
        private void RpcUpdateTimer(float time)
        {
            _timerTextObj.text = $"{time}";
        }

        [TargetRpc]
        private void TargetResetPlayerState(NetworkConnection conn)
        {
            Debug.Log($"Round ended! {_roundsPlayed} rounds played.");
            var player = conn.identity.GetComponent<Player>();
            var spawnIndex = GetPlayerSpawnIndex(player.PlayerId);

            var localScore = GetPlayerScore(player.PlayerId);
            var remoteScore = GetPlayerScore(GetRemotePlayerId(player.PlayerId));

            player.UpdateScoreUi(localScore, remoteScore);
            player.ResetState(NetworkManager.startPositions[spawnIndex]);
        }

        [TargetRpc]
        private void TargetDisplayRoundOver(NetworkConnection conn, bool? isWinner)
        {
            var player = conn.identity.GetComponent<Player>();
            player.DisplayRoundOverUi(isWinner);
        }

        [TargetRpc]
        private void TargetOnMatchEnded(NetworkConnection conn)
        {
            conn.identity.GetComponent<Player>().LeaveMatch();
        }
        #endregion
    }
}
