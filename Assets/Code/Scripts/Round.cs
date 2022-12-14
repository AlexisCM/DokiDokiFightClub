using System.Collections;
using UnityEngine;

public class Round : MonoBehaviour
{
    public float CurrentTime { get; private set; }   // Current time left in the round
    public bool IsOngoing { get; private set; }     // Flag for if the round is still in progress

    private const float _maxRoundDuration = 5f;     // Maximum duration per round in seconds

    void Start()
    {
        // Ensure initial round variables are set properly
        ResetRound();
    }

    public void StartRound()
    {
        IsOngoing = true;
        StartCoroutine("StartTimer");
    }

    public void ResetRound()
    {
        StopCoroutine("StartTimer");
        CurrentTime = _maxRoundDuration;
        IsOngoing = false;
    }

    IEnumerator StartTimer()
    {
        while (CurrentTime > 0)
        {
            yield return new WaitForSeconds(1f);
            --CurrentTime;
        }
    }
}
