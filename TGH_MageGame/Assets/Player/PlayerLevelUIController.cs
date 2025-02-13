using TMPro;
using UnityEngine;

public class PlayerLevelUIController : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI currentLevelText;
    [SerializeField]
    private PlayerStats playerStats;

    private void Start()
    {
        currentLevelText.text = "Player Level:\n" + playerStats.getLevel();
    }

    private void OnEnable()
    {
        playerStats.levelChangeEvent.AddListener(updateCurrentLevelText);
    }
    private void OnDisable()
    {
        playerStats.levelChangeEvent.RemoveListener(updateCurrentLevelText);
    }

    private void updateCurrentLevelText(int level)
    {
        currentLevelText.text = "Player Level:\n" + playerStats.getLevel();
    }
}
