using Mirror;
using System.Collections.Generic;
using UnityEngine;

namespace DokiDokiFightClub
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; } // Singleton instance

        [Header("Game Manager Settings")]
        public bool WaitingForPlayers = true;
        public List<GameObject> Players; // List of players

        internal Round Round { get; private set; } // Keeps track of current round's state

        private const int _maxRounds = 3;   // Maximum number of rounds played per match
        private int _roundsPlayed = 0;      // Current number of rounds played/completed

        public void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            Round = GetComponent<Round>();
            WaitingForPlayers = true;

            Debug.Log($"GameManager {GetInstanceID()} awoke.");
        }

        public void InitializeMatch(List<GameObject> players)
        {
            WaitingForPlayers = false;
            Debug.Log($"GameManager: Initializing Match");
            // Get all player objects
            Debug.Log($"Players areNull = {players == null}");
            Players = players;

            for (var i = 0; i < Players.Count; ++i)
                players[i].GetComponent<Player>().MatchMgrInstance = Instance;

            Round.StartRound();
        }

        public void Update()
        {
            if (WaitingForPlayers)
                return;

            // TESTING ONLY! APPLIES DMG TO PLAYER. REMOVE LATER:
            if (Input.GetKeyDown(KeyCode.V))
                Players[0].GetComponent<Player>().TakeDamage(50, 0);

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
                GameOver();
            }
        }

        private void RoundEnded()
        {
            ++_roundsPlayed;
            // TODO: Handle players swapping sides of the map after rounds

            foreach (GameObject player in Players)
            {
                player.GetComponent<Player>().ResetState();
            }

            Round.ResetRound();
        }

        void GameOver()
        {
            Debug.Log("GAME OVER!");
            // TODO: Determine the match winner
            StopAllCoroutines();
            // Transition player to match summary scene
            Cursor.lockState = CursorLockMode.None;
            WaitingForPlayers = true;
            // Disconnect player clients, which will automatically send them back to the offline screen
        }

        public Player GetPlayer(int playerId)
        {
            return Players[playerId].GetComponent<Player>();
        }

        public void PlayerDeath(GameObject deadPlayer)
        {
            Debug.Log($"{deadPlayer.name} DIED and lost the round!");
            // TODO: Update player scores
            RoundEnded();
        }
    }
}
