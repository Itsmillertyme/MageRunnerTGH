using UnityEngine;

public class EnemyHealth : MonoBehaviour {

    //**PROPERTIES**
    [Header("Health Base Stats")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;
    private readonly int minHealth = 0;

    //**FIELDS**
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int MinHealth => minHealth;

    //**UTILITY METHODS**
    public void RemoveFromHealth(int amountToRemove) {

        //remove health
        currentHealth -= amountToRemove;

        //test for death
        if (currentHealth <= minHealth) {
            currentHealth = minHealth;
            Destroy(gameObject);
        }
    }
}
