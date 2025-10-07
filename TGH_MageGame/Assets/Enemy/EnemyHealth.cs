using UnityEngine;

public class EnemyHealth : MonoBehaviour {

    //**PROPERTIES**
    [Header("Health Base Stats")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;
    private readonly int minHealth = 0;
    [SerializeField] private int xpGrantedOnDeath;
    private SpellLevels levelingSystem;


    //**FIELDS**
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        levelingSystem = FindFirstObjectByType<SpellLevels>();
    }

    //**UTILITY METHODS**
    public void RemoveFromHealth(int amountToRemove) {

        if (amountToRemove < currentHealth)
        {
            currentHealth -= amountToRemove;
        }
        else
        {
            currentHealth = minHealth;
            levelingSystem.AddXP(xpGrantedOnDeath);
            Destroy(this.gameObject);
        }
    }
}