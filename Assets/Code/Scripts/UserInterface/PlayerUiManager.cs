using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _healthMetreObject;

    [Header("Score UI")]
    [SerializeField]
    private GameObject _scoreUiObject;

    [SerializeField]
    private TMP_Text _localPlayerScoreObject;

    [SerializeField]
    private TMP_Text _remotePlayerScoreObject;

    #region Round Over Members
    [Header("Round Over Settings")]

    [SerializeField]
    private GameObject _roundOverObject;

    [SerializeField]
    private Image _roundOverBackground;

    [SerializeField]
    private TMP_Text _roundOverText;

    private Color32 _victoryBgColour = new(73, 188, 117, 80);
    private Color32 _defeatBgColour = new(226, 57, 57, 80);
    private Color32 _drawBgColour = new(70, 70, 70, 80);

    private readonly string _roundVictoryText = "Round Victory";
    private readonly string _roundDefeatText = "Round Defeat";
    private readonly string _roundDrawText = "Round Draw";

    #endregion

    public void ToggleHealthMetre(bool isActive)
    {
        _healthMetreObject.SetActive(isActive);
    }

    public void ToggleScoreUi(bool isActive)
    {
        _scoreUiObject.SetActive(isActive);
    }

    public void UpdateScoreUiValues(int localScore, int remoteScore)
    {
        _localPlayerScoreObject.text = localScore.ToString();
        _localPlayerScoreObject.text = remoteScore.ToString();
    }

    public void ToggleRoundOver(bool isActive)
    {
        _roundOverObject.SetActive(isActive);
    }

    /// <param name="isActive">Activate/Deactivate flag for the game object.</param>
    /// <param name="isWinner">Nullable bool. If null, the round ended in a Draw.</param>
    public void ToggleRoundOver(bool isActive, bool? isWinner)
    {
        _roundOverObject.SetActive(isActive);

        if (isWinner.HasValue)
        {
            _roundOverBackground.color = isWinner.Value ? _victoryBgColour : _defeatBgColour;
            _roundOverText.text = isWinner.Value ? _roundVictoryText : _roundDefeatText;
        }
        else
        {
            _roundOverBackground.color = _drawBgColour;
            _roundOverText.text = _roundDrawText;
        }
    }
}
