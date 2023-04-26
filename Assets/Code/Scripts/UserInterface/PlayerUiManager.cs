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
    [Header("Round Over UI")]

    [SerializeField]
    private GameObject _roundOverObject;

    [SerializeField]
    private Image _roundOverBackground;

    [SerializeField]
    private TMP_Text _roundOverText;

    private readonly string _roundVictoryText = "Round Victory";
    private readonly string _roundDefeatText = "Round Defeat";
    private readonly string _roundDrawText = "Round Draw";

    #endregion

    #region Game Over Members
    [Header("Game Over UI")]
    [SerializeField]
    private GameObject _gameOverObject;

    [SerializeField]
    private Image _gameOverBackground; // Spans the entire screen, mostly transparent

    [SerializeField]
    private Image _gameOverPanel;

    [SerializeField]
    private TMP_Text _gameOverText;

    private const string _gameVictoryText = "Victory";
    private const string _gameDefeatText = "Defeat";
    private const string _gameDrawText = "Draw";
    private readonly byte _gameOverBgAlpha = 20;

    #endregion

    private Color32 _victoryBgColour = new(73, 188, 117, 100);
    private Color32 _defeatBgColour = new(226, 57, 57, 100);
    private Color32 _drawBgColour = new(70, 70, 70, 100);

    public void ToggleHealthMetre(bool isActive)
    {
        _healthMetreObject.SetActive(isActive);
    }

    public void ToggleScoreUi(bool isActive)
    {
        _scoreUiObject.SetActive(isActive);
    }

    public void UpdateScoreUiValues(uint localScore, uint remoteScore)
    {
        _localPlayerScoreObject.text = localScore.ToString();
        _remotePlayerScoreObject.text = remoteScore.ToString();
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

    public void DisplayGameOver(bool? isWinner)
    {
        // ensure round over UI is disabled
        _roundOverObject.SetActive(false);
        // activate game over UI before updating values
        _gameOverObject.SetActive(true);

        Color32 bgColour;
        if (isWinner.HasValue)
        {
            // Update colour of panel that contains the text
            bgColour = isWinner.Value ? _victoryBgColour : _defeatBgColour;
            _gameOverPanel.color = bgColour;
            // Change alpha for background image
            bgColour.a = _gameOverBgAlpha;
            _gameOverBackground.color = bgColour;
            _gameOverText.text = isWinner.Value ? _gameVictoryText : _gameDefeatText;
        }
        else
        {
            // Update colour of panel that contains the text
            bgColour = _drawBgColour;
            _gameOverPanel.color = bgColour;
            // Change alpha for background image
            bgColour.a = _gameOverBgAlpha;
            _gameOverBackground.color = bgColour;
            _gameOverText.text = _gameDrawText;
        }
    }
}
