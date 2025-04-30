using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour {
    [SerializeField] Image healthBar;
    EnemyHealth health;

    private void Awake() {
        health = GetComponent<EnemyHealth>();
    }

    void Update() {
        healthBar.fillAmount = (float) health.CurrentHealth / health.MaxHealth;
    }
}
