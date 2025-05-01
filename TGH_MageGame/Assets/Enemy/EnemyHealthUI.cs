using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour {
    [SerializeField] Image healthBar;
    float currentHealth;
    float maxHealth;
    bool isBoss;

    private void Awake() {
        if (GetComponent<EnemyHealth>() == null) {
            isBoss = true;
            maxHealth = GetComponent<BossHealth>().MaxHealth;
        }
        else {
            isBoss = false;
            maxHealth = GetComponent<EnemyHealth>().MaxHealth;
        }

    }

    void Update() {
        if (isBoss) {
            currentHealth = GetComponent<BossHealth>().CurrentHealth;
        }
        else {
            currentHealth = GetComponent<EnemyHealth>().CurrentHealth;
        }
        healthBar.fillAmount = currentHealth / maxHealth;
    }
}
