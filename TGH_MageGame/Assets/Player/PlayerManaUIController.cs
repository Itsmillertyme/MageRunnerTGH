using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PlayerManaUIController : MonoBehaviour {
    [SerializeField]
    public TextMeshProUGUI currentManaText;
    [SerializeField]
    private PlayerStats playerStats;

    private void Start() {
        currentManaText.text = "Mana:\n" + playerStats.getCurrentMana();
        GetComponent<Image>().fillAmount = playerStats.getCurrentMana() / (float)playerStats.getMaxMana();
    }

    private void OnEnable() {
        playerStats.currentManaChangeEvent.AddListener(updateCurrentManaText);
    }
    private void OnDisable() {
        playerStats.currentManaChangeEvent.RemoveListener(updateCurrentManaText);
    }

    private void updateCurrentManaText(int mana) {
        currentManaText.text = "Mana:\n" + mana;
        GetComponent<Image>().fillAmount = mana / (float)playerStats.getMaxMana();
    }

    private void Update() {
        if (Input.GetKey(KeyCode.N)) {
            playerStats.updateCurrentMana(-5);
        }
        if (Input.GetKey(KeyCode.M)) {
            playerStats.updateCurrentMana(5);
        }
    }
}
