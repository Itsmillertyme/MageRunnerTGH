using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExpUIController : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI currentExpText;
    [SerializeField]
    private PlayerStats playerStats;

    private void Start()
    {
        currentExpText.text = "Exp:\n" + playerStats.getExperience();
        GetComponent<Image>().fillAmount = playerStats.getExperience() / (float)playerStats.getExperienceForNextLevel();
    }

    private void OnEnable()
    {
        playerStats.experienceChangeEvent.AddListener(updateCurrentExpText);
    }
    private void OnDisable()
    {
        playerStats.experienceChangeEvent.RemoveListener(updateCurrentExpText);
    }

    private void updateCurrentExpText(int exp)
    {
        currentExpText.text = "Exp:\n" + exp;
        GetComponent<Image>().fillAmount = exp / (float)playerStats.getExperienceForNextLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            playerStats.updateExperience((int)(playerStats.getExperienceForNextLevel()*0.05f));
        }
    }
}
