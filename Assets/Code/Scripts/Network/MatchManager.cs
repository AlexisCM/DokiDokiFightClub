using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DokiDokiFightClub
{
    [RequireComponent(typeof(Round))]
    public class MatchManager : NetworkBehaviour
    {

        public int MatchInstanceId { get; private set; }
        public bool WaitingForPlayers = true;
        public List<GameObject> Players;
        internal Round Round { get; private set; } // Keeps track of current round's state

        private DdfcNetworkManager _networkManager;
        private const int _maxRounds = 3;   // Maximum number of rounds played per match

        [SyncVar]
        private int _roundsPlayed = 0;      // Current number of rounds played/completed

        [SyncVar]
        private bool _isDoingWork = false;  // Flag for methods called in the update loop

        public void SetMatchInstanceId(int matchInstanceId)
        {
            MatchInstanceId = matchInstanceId;
        }

        private void Start()
        {
            _networkManager = NetworkManager.singleton as DdfcNetworkManager;
            Round = GetComponent<Round>();
        }

        public void StartMatch(List<GameObject> players)
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
                RoundEnded();
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
                MatchEnded();
            }
        }

        public void PlayerDeath(GameObject playerObject)
        {
            // TODO: Keep track of players' round wins/losses
            RoundEnded();
        }

        private void RoundEnded()
        {
            _isDoingWork = true;
            ++_roundsPlayed;

            foreach (var playerObject in Players)
            {
                var player = playerObject.GetComponent<Player>();
                TargetResetPlayerState(player.GetComponent<NetworkIdentity>().connectionToClient);
            }

            Round.ResetRound();
            _isDoingWork = false;
        }

        private void MatchEnded()
        {
            Debug.Log("GAME OVER!");
            // TODO: Determine the match winner
            StopAllCoroutines();
            // Transition player to match summary scene
            Cursor.lockState = CursorLockMode.None;
            WaitingForPlayers = true;
            // Disconnect player clients, which will automatically send them back to the offline screen
        }

        /// <summary>
        /// If the number of rounds played is odd, the player must swap spawn positions.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int GetPlayerSpawnIndex(Player player)
        {
            int index;
            if (_roundsPlayed % 2 == 0)
            {
                index = player.PlayerId;
            }
            else
            {
                index = (player.PlayerId == 0) ? 1 : 0;
            }
            return index;
        }

        #region Remote Prodecure Calls
        [TargetRpc]
        private void TargetResetPlayerState(NetworkConnection conn)
        {
            var player = conn.identity.gameObject.GetComponent<Player>();
            int spawnIndex = GetPlayerSpawnIndex(player);
            player.ResetState(NetworkManager.startPositions[spawnIndex]);
        }
        #endregion
    }
}