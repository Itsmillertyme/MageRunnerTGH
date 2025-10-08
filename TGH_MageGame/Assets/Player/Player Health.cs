using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour {
    [Header("Health Base Stats")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;

    [Header("Health Regen")]
    [SerializeField] private int healthRegenAmount;
    [SerializeField] private float healthRegenFrequency;

    [Header("Health Events")]
    [SerializeField] private UnityEvent healthChanged;

    #region DRIVEN
    private Coroutine healthRegen;
    private readonly int minHealth = 0;
    #endregion

    // GETTERS
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int MinHealth => minHealth;

    private void Start() {
        //healthChanged.Invoke();                     

        if (healthRegen == null && currentHealth < maxHealth) {
            healthRegen = StartCoroutine(HealOverTime());
        }
    }

    public void AddToHealth(int add) {
        if (add <= maxHealth - currentHealth) {
            currentHealth += add;
        }
        else {
            currentHealth += maxHealth - currentHealth;
        }

        healthChanged.Invoke();
    }

    public void RemoveFromHealth(int remove) {
        if (remove < currentHealth) {
            currentHealth -= remove;

            if (healthRegen != null) {
                StopCoroutine(healthRegen);
            }

            healthRegen = StartCoroutine(HealOverTime());
        }
        else {
            currentHealth = minHealth;
            Destroy(this);
            Time.timeScale = 0f;
            Debug.Log("You died");
        }

        healthChanged.Invoke();
    }

    public IEnumerator HealOverTime() {
        while (currentHealth < maxHealth) {
            yield return new WaitForSeconds(healthRegenFrequency);
            AddToHealth(healthRegenAmount);
        }
    }

    public void IncreaseMaxHealth(int amount) {
        maxHealth += amount;
        currentHealth += amount;
        healthChanged.Invoke();
    }

}
