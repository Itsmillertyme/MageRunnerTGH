using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpellBook : MonoBehaviour {
    [Header("Spell List")]
    [SerializeField] private List<Spell> spellBook;

    [Header("Spawn Position")]
    [SerializeField] private Transform spawnPosition;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent ActiveSpellSwitched;

    [Header("Player Stats")]
    [SerializeField] PlayerStats playerStats;

    [Header("Mouse Position Tracker")]
    [SerializeField] MousePositionTracking mousePositionTracker;

    // GETTERS
    public int ActiveSpell => activeSpell;

    // DRIVEN
    #region DRIVEN
    [Header("Debugging")]
    private float scrollValue;
    private Coroutine castDelay;
    private bool isReadyToCast = true;
    private int activeSpell;
    #endregion

    void Update() {
        scrollValue = Input.mouseScrollDelta.y;

        // TEMP WORKAROUND UNTIL INPUT SYSTEM METHOD IS PRESENT
        if (scrollValue != 0) {
            SetSpell();
        }
    }

    public void Cast() {
        if (isReadyToCast && playerStats.getCurrentMana() >= spellBook[activeSpell].ManaCost) {
            GameObject projectile;
            int spellLevel = spellBook[activeSpell].CurrentLevel;

            switch (spellLevel) {
                case 1:
                    projectile = Instantiate(spellBook[activeSpell].SpawnObjectLvl1, spawnPosition.transform.position, spawnPosition.transform.rotation);
                    projectile.GetComponent<ProjectileMover>().SetAttributes(spellBook[activeSpell].Damage, spellBook[activeSpell].LifeSpan, spellBook[activeSpell].MoveSpeed, mousePositionTracker.CurrentPosition);
                    break;
                case 2:
                    projectile = Instantiate(spellBook[activeSpell].SpawnObjectLvl2, spawnPosition.transform.position, spawnPosition.transform.rotation);
                    projectile.GetComponent<ProjectileMover>().SetAttributes(spellBook[activeSpell].Damage, spellBook[activeSpell].LifeSpan, spellBook[activeSpell].MoveSpeed, mousePositionTracker.CurrentPosition);
                    break;
                case 3:
                    projectile = Instantiate(spellBook[activeSpell].SpawnObjectLvl3, spawnPosition.transform.position, spawnPosition.transform.rotation);
                    projectile.GetComponent<ProjectileMover>().SetAttributes(spellBook[activeSpell].Damage, spellBook[activeSpell].LifeSpan, spellBook[activeSpell].MoveSpeed, mousePositionTracker.CurrentPosition);
                    break;
            }
            if (activeSpell == 1) {
                StartCoroutine(FireSecondRay());
            }

            playerStats.updateCurrentMana(-spellBook[activeSpell].ManaCost);
            castDelay = StartCoroutine(CastDelay());
        }
    }

    // HANDLES DELAY IN ABILITY TO CAST AGAIN
    public IEnumerator CastDelay() {
        isReadyToCast = false;
        yield return new WaitForSeconds(spellBook[ActiveSpell].CastDelayTime);
        isReadyToCast = true;
    }

    // SPELL INVENTORY CYCLING
    private void SetSpell() {
        // if 
        if (scrollValue < 0f) {
            do {
                activeSpell++;

                if (activeSpell >= spellBook.Count) {
                    activeSpell = 0;
                }
            }
            while (!spellBook[activeSpell].IsUnlocked);
        }

        else if (scrollValue > 0f) {
            do {
                activeSpell--;

                if (activeSpell < 0) {
                    activeSpell = spellBook.Count - 1;
                }
            }
            while (!spellBook[activeSpell].IsUnlocked);
        }

        // RAISE AN EVENT THAT THE SPELL SELECTION HAS CHANGED
        ActiveSpellSwitched.Invoke();
    }

    // GETS ACTIVE SPELL TO USE IN UI TEXT
    public string GetSpellUIData() {
        return $"{spellBook[activeSpell].Name}\nLevel {spellBook[activeSpell].CurrentLevel}";
    }

    // GETS ACTIVE SPELL ICON TO USE IN UI
    public Sprite GetSpellIconData() {
        return spellBook[activeSpell].SpellIcon;
    }

    // GETTER FOR ACTIVE SPELL ANIMATION
    public AnimationClip GetSpellAnimation() {
        return spellBook[activeSpell].CastAnimation;
    }

    // GETTER FOR ACTIVE SPELL SPAWN SOUND
    public AudioClip GetSpellSpawnSound() {
        return spellBook[activeSpell].SpawnSFX;
    }
    public void LevelUpActiveSpell() {
        spellBook[activeSpell].LevelUp();
    }
    public void LevelDownActiveSpell() {
        spellBook[activeSpell].LevelDown();
    }


    //Coroutine for 'Rays' Spell
    IEnumerator FireSecondRay() {
        GameObject ray2;

        yield return new WaitForSeconds(6f / 30f);

        ray2 = Instantiate(spellBook[activeSpell].SpawnObjectLvl2, spawnPosition.transform.position, spawnPosition.transform.rotation);
        ray2.GetComponent<ProjectileMover>().SetAttributes(spellBook[activeSpell].Damage, spellBook[activeSpell].LifeSpan, spellBook[activeSpell].MoveSpeed, mousePositionTracker.CurrentPosition);

    }
}