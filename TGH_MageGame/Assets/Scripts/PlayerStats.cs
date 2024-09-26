using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO.Enumeration;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]

public class PlayerStats : ScriptableObject
{
    [System.NonSerialized]
    public UnityEvent<int> currentHealthChangeEvent;
    [System.NonSerializedAttribute]
    public UnityEvent<int> currentManaChangeEvent;
    [System.NonSerializedAttribute]
    public UnityEvent<int> experienceChangeEvent;
    [System.NonSerializedAttribute]
    public UnityEvent<int> levelChangeEvent;
    [System.NonSerializedAttribute]
    public UnityEvent manaSpentEvent;

    // The base stats the player initially will have at the start of a run.
    public int baseHealth = 100;
    public int baseAttackDamage = 5;
    public int baseAttackSpeed = 5;
    public int baseDefence = 5;
    public int baseMovementSpeed = 5;
    public int baseMana = 100;

    // Stores the amount stats upgrade on level up.
    public int healthUpgrade = 5;
    public int attackDamageUpgrade = 1;
    public int attackSpeedUpgrade = 1;
    public int defenceUpgrade = 1;
    public int movementSpeedUpgrade = 0; // It may be better to have movement speed upgrades be intentional only instead of every level up increasing movement speed.
    public int manaUpgrade = 5;

    // Keeps track of current level, experience, and skill points.
    int level = 1;
    int experience = 0;
    int experienceForNextLevel = 100;
    int skillPoints = 0;

    // Stores the bonus amount of each stat.
    int bonusHealth = 0;
    int bonusAttackDamage = 0;
    int bonusAttackSpeed = 0;
    int bonusDefence = 0;
    int bonusMovementSpeed = 0;
    int bonusMana = 0;

    // These variables will store the maximum possible of stats at a given moment.
    int maxHealth;
    int maxAttackDamage;
    int maxAttackSpeed;
    int maxDefence;
    int maxMovementSpeed;
    int maxMana;

    // Stores the current health and mana
    [Header("Current Attributes")]
    [SerializeField]
    int currentHealth;
    [SerializeField]
    int currentMana;

    private void OnEnable()
    {
        // Initialzes the max value of each stat at the beginning of the game.
        updateMaxHealth();
        updateMaxAttackDamage();
        updateMaxAttackSpeed();
        updateMaxDefence();
        updateMaxMovementSpeed();
        updateMaxMana();
        currentHealth = maxHealth;
        currentMana = maxMana;
        if (currentHealthChangeEvent == null)
        {
            currentHealthChangeEvent = new UnityEvent<int>();
        }
        if (currentManaChangeEvent == null)
        {
            currentManaChangeEvent = new UnityEvent<int>();
        }
        if(manaSpentEvent == null)
        {
            manaSpentEvent = new UnityEvent();
        }
        if(experienceChangeEvent == null)
        {
            experienceChangeEvent = new UnityEvent<int>();
        }
        if(levelChangeEvent == null)
        {
            levelChangeEvent = new UnityEvent<int>();
        }
    }

    // Methods that will be called whenever the maximum value of a stat should change
    void updateMaxHealth()
    {
        int oldMax = maxHealth;
        maxHealth = baseHealth + (healthUpgrade * (level - 1)) + bonusHealth;

        if (currentHealth >= oldMax)
        {
            currentHealth = maxHealth;
        }
    }
    void updateMaxAttackDamage()
    {
        maxAttackDamage = baseAttackDamage + (attackDamageUpgrade * (level - 1)) + bonusAttackDamage;
    }
    void updateMaxAttackSpeed()
    {
        maxAttackSpeed = baseAttackSpeed + (attackSpeedUpgrade * (level - 1)) + bonusAttackSpeed;
    }
    void updateMaxDefence()
    {
        maxDefence = baseDefence + (defenceUpgrade * (level - 1)) + bonusDefence;


        // Right now the idea for defence is that it will reduce a percent of damage so 100 will be the max since that will reduce 100% of damage
        if (maxDefence > 100)
        {
            maxDefence = 100;
        }
    }
    void updateMaxMovementSpeed()
    {
        maxMovementSpeed = baseMovementSpeed + (movementSpeedUpgrade * (level - 1)) + bonusMovementSpeed;
    }
    void updateMaxMana()
    {
        int oldMax = maxMana;
        maxMana = baseMana + (manaUpgrade * (level - 1)) + bonusMana;

        if (currentMana >= oldMax)
        {
            currentMana = maxMana;
        }

    }

    // Methods for applying the bonus from the skill tree or other sources
    public void updateBonusHealth(int bonus)
    {
        bonusHealth += bonus;
        updateMaxHealth();
    }
    public void updateBonusAttackDamage(int bonus)
    {
        bonusAttackDamage += bonus;
        updateMaxAttackDamage();
    }
    public void updateBonusAttackSpeed(int bonus)
    {
        bonusAttackSpeed += bonus;
        updateMaxAttackSpeed();
    }
    public void updateBonusDefence(int bonus)
    {
        bonusDefence += bonus;
        updateMaxDefence();
    }
    public void updateBonusMovementSpeed(int bonus)
    {
        bonusMovementSpeed += bonus;
        updateMaxMovementSpeed();
    }
    public void updateBonusMana(int bonus)
    {
        bonusMana += bonus;
        updateMaxMana();
    }
    
    // Functions for spending skillpoints
    public void spendSkillpoint()
    {
        skillPoints--;
    }
    // The value of amount will be equal to the amount healed or damage dealt
    public void updateCurrentHealth(int amount)
    {
        if (amount >= 0)
        {
            currentHealth += amount;
        }
        else if (amount < 0)
        {
            if (maxDefence > 0)
            {
                currentHealth += (int)((1f - ((float)maxDefence/100)) *  amount);
            }
            else
            {
                currentHealth += amount;
            }
        }

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        currentHealthChangeEvent.Invoke(currentHealth);
    }
    public void updateCurrentMana(int amount)
    {
        currentMana += amount;

        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
        if (currentMana < 0)
        {
            currentMana = 0;
        }
        if (amount < 0)
        {
            manaSpentEvent.Invoke();
        }
        currentManaChangeEvent.Invoke(currentMana);
    }

    // The function for updating experience and levels
    public void updateExperience(int exp)
    {
        experience += exp;

        if (experience >= experienceForNextLevel)
        {
            experience = experience - experienceForNextLevel;
            experienceForNextLevel = (int)(experienceForNextLevel * 1.05);

            level++;
            skillPoints += 3;

            // Calls function again if there is enough exp for another level up

            if (experience >= experienceForNextLevel)
            {
                updateExperience(0);
            }

            updateMaxHealth();
            updateMaxAttackDamage();
            updateMaxAttackSpeed();
            updateMaxDefence();
            updateMaxMovementSpeed();
            updateMaxMana();
            levelChangeEvent.Invoke(level);
            //Only invokes these to make sure the sliders for current health and mana are correct after a level up
            currentHealthChangeEvent.Invoke(currentHealth);
            currentManaChangeEvent.Invoke(currentMana);
        }
        experienceChangeEvent.Invoke(experience);
    }

    // Resets all stats to default values
    public void resetToDefault()
    {
        bonusHealth = 0;
        bonusAttackDamage = 0;
        bonusAttackSpeed = 0;
        bonusDefence = 0;
        bonusMovementSpeed = 0;
        bonusMana = 0;
        level = 1;
        experience = 0;
        experienceForNextLevel = 100;
        skillPoints = 0;
        updateMaxHealth();
        updateMaxAttackDamage();
        updateMaxAttackSpeed();
        updateMaxDefence();
        updateMaxMovementSpeed();
        updateMaxMana();

    }

    // Functions that allow the stats to be gotten
    public int getMaxHealth()
    {
        return maxHealth;
    }
    public int getMaxAttackDamage()
    {
        return maxAttackDamage;
    }
    public int getMaxAttackSpeed()
    {
        return maxAttackSpeed;
    }
    public int getMaxDefence()
    {
        return maxDefence;
    }
    public int getMaxMovementSpeed()
    {
        return maxMovementSpeed;
    }
    public int getMaxMana()
    {
        return maxMana;
    }
    public int getCurrentHealth()
    {
        return currentHealth;
    }
    public int getCurrentMana()
    {
        return currentMana;
    }
    public int getLevel()
    {
        return level;
    }
    public int getExperience()
    {
        return experience;
    }
    public int getExperienceForNextLevel()
    {
        return experienceForNextLevel;
    }
    public int getSkillPoints()
    {
        return skillPoints;
    }
}
