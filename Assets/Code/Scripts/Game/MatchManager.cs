using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

namespace DokiDokiFightClub
{
    [RequireComponent(typeof(Round))]
    [RequireComponent(typeof(ScoreKeeper))]
    public class MatchManager : NetworkBehaviour
    {
        public int MatchInstanceId { get; private set; }
        public bool WaitingForPlayers = true;

        public List<Player> Players;

        internal Round Round { get; private set; } // Keeps track of current round's state
        internal ScoreKeeper ScoreKeeper { get; private set; }

        private DdfcNetworkManager _networkManager;
        private const int _maxRounds = 3;   // Maximum number of rounds played per match
        private const float _timeBetweenRounds = 5f;

        [SerializeField]
        private TMP_Text _timerTextObj; // The UI representing the timer

        [SerializeField]
        private GameObject _matchForfeitObj; // The UI to display when a match is forfeit (disconnection)

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
            ScoreKeeper = GetComponent<ScoreKeeper>();
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

            // Check for disconnected players
            if (Players.Count <= 1)
            {
                StartCoroutine(PlayerDisconnected());
            }

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
            RpcLogMessage($"<color=red>Player#{deadPlayerId} was KILLED!</color>");
            // Update player scores
            ScoreKeeper.AddScore(GetRemotePlayerId(deadPlayerId));
            StartCoroutine(RoundEnded(deadPlayerId));
        }

        private IEnumerator RoundEnded(int? deadPlayerId)
        {
            _isDoingWork = true;
            Round.IsOngoing = false;
            ++_roundsPlayed;

            foreach (var score in _playerScoreKeeper)
            {
                Debug.Log($"Player: {score.Key}, Score: {score.Value}");
            }

            // Display Round Over UI
            foreach (var player in Players)
            {
                if (deadPlayerId != null && deadPlayerId != player.PlayerId)
                {
                    // Victorious player
                    TargetDisplayRoundOver(player.connectionToClient, true);
                }
                else if (deadPlayerId != null && deadPlayerId == player.PlayerId)
                {
                    // Defeated player
                    TargetDisplayRoundOver(player.connectionToClient, false);
                }
                else
                {
                    // Draw
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

        private IEnumerator PlayerDisconnected()
        {
            Round.PauseRound();
            // Disable player controls
            foreach (var player in Players)
                TargetDisablePlayerControls(player.connectionToClient);
            // Display Match Forfeit UI
            RpcOnPlayerDisonnect();
            // Delay to allow UI time to appear
            yield return new WaitForSeconds(_timeBetweenRounds);

            StartCoroutine(MatchEnded());
        }

        public void RemovePlayerFromMatch(Player playerToRemove)
        {
            for (int i = 0; i < Players.Count; ++i)
            {
                if (playerToRemove.Equals(Players[i]))
                {
                    Players.RemoveAt(i);
                    break;
                }
            }
            Debug.Log("Removed Player: " + playerToRemove);
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

        [ClientRpc]
        private void RpcOnPlayerDisonnect()
        {
            _matchForfeitObj.SetActive(true);
        }

        [TargetRpc]
        private void TargetDisablePlayerControls(NetworkConnection conn)
        {
            conn.identity.GetComponent<Player>().ToggleComponents(false);
        }

        [TargetRpc]
        private void TargetResetPlayerState(NetworkConnection conn)
        {
            Debug.Log($"Round ended! {_roundsPlayed} rounds played.");
            var player = conn.identity.GetComponent<Player>();
            var spawnIndex = GetPlayerSpawnIndex(player.PlayerId);
            player.ResetState(NetworkManager.startPositions[spawnIndex]);
        }

        [TargetRpc]
        private void TargetDisplayRoundOver(NetworkConnection conn, bool? isWinner)
        {
            var player = conn.identity.GetComponent<Player>();
            var localScore = ScoreKeeper.GetScore(player.PlayerId);
            var remoteScore = ScoreKeeper.GetScore(GetRemotePlayerId(player.PlayerId));

            player.UpdateScoreUi(localScore, remoteScore);
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
