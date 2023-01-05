using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } // Singleton instance

    public GameObject[] Players; // List of players

    internal Round Round { get; private set; } // Keeps track of current round's state

    private const int _maxRounds = 3;   // Maximum number of rounds played per match
    private int _roundsPlayed = 0;      // Current number of rounds played/completed

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        Round = GetComponent<Round>();
    }

    void Start()
    {
        Debug.Log("Game started");

        // Get all player objects
        Players = GameObject.FindGameObjectsWithTag("Player");

        Round.StartRound();
    }

    void Update()
    {
        // TESTING ONLY! APPLIES DMG TO PLAYER. REMOVE LATER:
        if (Input.GetKeyDown(KeyCode.V))
            Players[0].GetComponent<Player>().TakeDamage(50);

        // Check if round ended
        if (Round.CurrentTime <= 0)
        {
            RoundEnded();
        }

        if (!Round.IsOngoing && _roundsPlayed < _maxRounds)
        {
            Debug.Log($"Rounds Played: {_roundsPlayed}");
            Round.StartRound();
        }
        else if (!Round.IsOngoing && _roundsPlayed == _maxRounds)
        {
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
        SceneManager.LoadScene("MenuScene");
    }

    public void PlayerDeath(GameObject deadPlayer)
    {
        Debug.Log($"{deadPlayer.name} DIED and lost the round!");
        // TODO: Update player scores
        RoundEnded();
    }
}
