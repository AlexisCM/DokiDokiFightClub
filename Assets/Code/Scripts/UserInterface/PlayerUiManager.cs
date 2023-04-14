using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _healthMetreObject;

    #region Round Over Members
    [Header("Round Over Settings")]

    [SerializeField]
    private GameObject _roundOverObject;

    [SerializeField]
    private Image _roundOverBackground;

    [SerializeField]
    private TMP_Text _roundOverText;

    private Color _victoryColour = new (73, 188, 117);
    private Color _defeatColour = new(226, 57, 57);

    private Color _victoryBgColour = new(73, 188, 117, 80);
    private Color _defeatBgColour = new(226, 57, 57, 80);

    private readonly string _roundVictoryText = "Round Victory";
    private readonly string _roundDefeatText = "Round Defeat";

    #endregion

    public void ToggleHealthMetre(bool isActive)
    {
        _healthMetreObject.SetActive(isActive);
    }

    public void ToggleRoundOver(bool isActive)
    {
        _roundOverObject.SetActive(isActive);
    }

    public void ToggleRoundOver(bool isActive, bool isWinner)
    {
        _roundOverObject.SetActive(isActive);
        _roundOverBackground.color = isWinner ? _victoryBgColour : _defeatBgColour;
        _roundOverText.text = isWinner ? _roundVictoryText : _roundDefeatText;
    }
}
