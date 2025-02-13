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

    //[Header("Cursor Position Tracker")]
    //[SerializeField] MousePositionTracking mousePositionTracker;
    [Header("Game Manager")]
    [SerializeField] GameManager gameManager;

    // GETTERS
    public int ActiveSpell => activeSpell;
    public bool IsReadyToCast => isReadyToCast;

    // DRIVEN
    #region DRIVEN
    [Header("Debugging")]
    private float scrollValue;
    private Coroutine castDelay;
    private bool isReadyToCast = true;
    private int activeSpell;
    private int lastActiveSpell;
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
            switch (spellBook[activeSpell].SpellIdentifier) {
                case Spell.SpellName.Fireball:
                    CastSpawnProjectileAtPoint();
                    break;
                case Spell.SpellName.Rays:
                    CastSpawnProjectileAtPoint();
                    break;
                case Spell.SpellName.Flamethrower:
                    CastSpawnProjectileAtPoint(2);
                    break;
                case Spell.SpellName.BeamOfLight:
                    CastSpawnProjectileAtPoint(7);
                    break;
                case Spell.SpellName.EarthShards:
                    // TBD
                    break;
                case Spell.SpellName.LightningStrike:
                    CastSpawnProjectileAtPoint(4);
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
                lastActiveSpell = activeSpell;
                activeSpell++;

                if (activeSpell >= spellBook.Count) {
                    activeSpell = 0;
                }
            }
            while (!spellBook[activeSpell].IsUnlocked);
        }

        else if (scrollValue > 0f) {
            do {
                lastActiveSpell = activeSpell;
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

    // SET SPELL DIRECTLY
    public void SetSpellByIndex(int newSpellIndex) {
        //Validate input
        if (newSpellIndex < 0 || newSpellIndex > spellBook.Count) {
            return;
        }

        lastActiveSpell = activeSpell;
        activeSpell = newSpellIndex;

        // RAISE AN EVENT THAT THE SPELL SELECTION HAS CHANGED
        ActiveSpellSwitched.Invoke();
    }

    public void HotSwitchSpell() {
        SetSpellByIndex(lastActiveSpell);
    }

    // GETS ACTIVE SPELL TO USE IN UI TEXT
    public string GetSpellUIData() {
        return $"{spellBook[activeSpell].Name}\nLevel {spellBook[activeSpell].CurrentLevel}";
    }

    // GETS ACTIVE SPELL ICON TO USE IN UI
    public Sprite GetSpellIconData() {
        return spellBook[activeSpell].SpellIcon;
    }

    // GETS ACTIVE SPELL RETICLE TO USE IN UI
    public Sprite GetSpellReticleData() {
        return spellBook[activeSpell].Reticle;
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

        ray2 = Instantiate(spellBook[activeSpell].CurrentProjectile, spawnPosition.position, spawnPosition.rotation);
        ray2.GetComponent<ProjectileMover>().SetAttributes(spellBook[activeSpell].Damage, spellBook[activeSpell].LifeSpan, spellBook[activeSpell].MoveSpeed, spellBook[activeSpell].ProjectileSize, gameManager.CrosshairPositionIn3DSpace);
    }

    private void CastSpawnProjectileAtPoint() {
        GameObject projectile = Instantiate(spellBook[activeSpell].CurrentProjectile, spawnPosition.position, spawnPosition.rotation);
        projectile.GetComponent<ProjectileMover>().SetAttributes(spellBook[activeSpell].Damage, spellBook[activeSpell].LifeSpan, spellBook[activeSpell].MoveSpeed, spellBook[activeSpell].ProjectileSize, gameManager.CrosshairPositionIn3DSpace);
    }

    private void CastSpawnProjectileAtPoint(float offsetAlongPathValue) {
        Vector3 direction = gameManager.CrosshairPositionIn3DSpace - spawnPosition.position;
        Vector3 directionNormalized = direction.normalized;

        Vector3 offsetPosition = spawnPosition.position + directionNormalized * offsetAlongPathValue;

        GameObject projectile = Instantiate(spellBook[activeSpell].CurrentProjectile, offsetPosition, Quaternion.LookRotation(direction));
        projectile.GetComponent<ProjectileMover>().SetAttributes(spellBook[activeSpell].Damage, spellBook[activeSpell].LifeSpan, spellBook[activeSpell].MoveSpeed, spellBook[activeSpell].ProjectileSize, gameManager.CrosshairPositionIn3DSpace);
    }
}