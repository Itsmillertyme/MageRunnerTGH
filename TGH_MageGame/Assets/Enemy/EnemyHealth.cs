using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Base Stats")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth;
    private readonly int minHealth = 0;

    // GETTERS
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int MinHealth => minHealth;

    public void RemoveFromHealth(int remove)
    {
        if (remove < currentHealth)
        {
            currentHealth -= remove;
        }
        else
        {
            currentHealth = minHealth;
            Destroy(gameObject);
        }
    }
}
