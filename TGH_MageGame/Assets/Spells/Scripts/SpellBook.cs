using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class SpellBook : MonoBehaviour {
    [Header("Spell List")]
    [SerializeField] private Spell[] spellBook;

    [Header("Spell Spawn Points")]
    [Tooltip("Must be in the same order as the spells themselves")]
    [SerializeField] private Transform[] spellSpawnPoints;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent ActiveSpellSwitched;

    [Header("Player Stats")]
    [SerializeField] PlayerStats playerStats;

    [Header("Game Manager")]
    [SerializeField] GameManager gameManager;

    #region GETTERS
    public int ActiveSpell => currentSpellIndex;
    public bool IsReadyToCast => isReadyToCast;
    #endregion

    #region DRIVEN
    private Transform currentSpawnPoint;
    private float scrollValue;
    private Coroutine castCooldown; // later implenent coroutine stopping for interrupted delay when cycling away (if desired)
    private int currentSpellIndex = 0;
    private bool isReadyToCast = true;
    private bool castInterrupt = false;
    private int lastActiveSpell;
    #endregion

    private void Awake() {
        foreach (Spell spell in spellBook) {
            spell.SetDefaultValues();
        }
        currentSpawnPoint = GetSpellSpawnPoint();
    }

    private void Update() {
        scrollValue = Input.mouseScrollDelta.y; // REFACTOR TO NEW INPUT WHEN MERGED

        // TEMP WORKAROUND UNTIL INPUT SYSTEM METHOD IS PRESENT
        if (scrollValue != 0) {
            SetSpell();
        }
    }

    public void Cast() {
        if (isReadyToCast && playerStats.getCurrentMana() >= spellBook[currentSpellIndex].ManaCost) {
            spellBook[currentSpellIndex].Cast(currentSpawnPoint.position, gameManager.CrosshairPositionIn3DSpace);
            playerStats.updateCurrentMana(-spellBook[currentSpellIndex].ManaCost);
            castCooldown = StartCoroutine(CastCooldown());
        }
    }

    // HANDLES DELAY IN ABILITY TO CAST AGAIN
    public IEnumerator CastCooldown() {
        isReadyToCast = false;

        float currentTime = Time.time;
        float endDelayTime = currentTime + spellBook[ActiveSpell].CastCooldownTime;

        while (Time.time < endDelayTime && !castInterrupt) {
            yield return new WaitForEndOfFrameUnit();
        }
        castInterrupt = false;
        isReadyToCast = true;
    }

    // SPELL INVENTORY CYCLING
    private void SetSpell() {
        // IF SCROLLING THE MOUSE WHEEL
        if (scrollValue < 0f) {
            do {
                lastActiveSpell = currentSpellIndex;
                currentSpellIndex++;

                if (currentSpellIndex >= spellBook.Length) {
                    currentSpellIndex = 0;
                }
            }
            while (!spellBook[currentSpellIndex].IsUnlocked);
        }

        else if (scrollValue > 0f) {
            do {
                lastActiveSpell = currentSpellIndex;
                currentSpellIndex--;

                if (currentSpellIndex < 0) {
                    currentSpellIndex = spellBook.Length - 1;
                }
            }
            while (!spellBook[currentSpellIndex].IsUnlocked);
        }

        // SET SPELL SPAWN POINT
        currentSpawnPoint = GetSpellSpawnPoint();

        // RAISE AN EVENT THAT THE SPELL SELECTION HAS CHANGED
        ActiveSpellSwitched.Invoke();
    }
    // SET SPELL DIRECTLY
    public void SetSpellByIndex(int newSpellIndex) {
        //Validate input
        if (newSpellIndex < 0 || newSpellIndex > spellBook.Length) {
            return;
        }

        lastActiveSpell = currentSpellIndex;
        currentSpellIndex = newSpellIndex;

        // SET SPELL SPAWN POINT
        currentSpawnPoint = GetSpellSpawnPoint();

        // RAISE AN EVENT THAT THE SPELL SELECTION HAS CHANGED
        ActiveSpellSwitched.Invoke();
    }
    public void HotSwitchSpell() {
        SetSpellByIndex(lastActiveSpell);
    }

    public Transform GetSpellSpawnPoint() => spellSpawnPoints[currentSpellIndex];
    public string GetSpellUIData() => spellBook[currentSpellIndex].Name; // GETS ACTIVE SPELL TO USE IN UI TEXT
    public Sprite GetSpellIconData() => spellBook[currentSpellIndex].SpellIcon; // GETS ACTIVE SPELL ICON TO USE IN UI
    public Sprite GetSpellReticleData() => spellBook[currentSpellIndex].Reticle; // GETS ACTIVE SPELL RETICLE TO USE IN UI
    public AnimationClip GetSpellAnimation() => spellBook[currentSpellIndex].CastAnimation; // GETTER FOR ACTIVE SPELL ANIMATION
    public float GetSpellCastDelayTime() => spellBook[currentSpellIndex].CastDelayTime; // GETTER FOR ACTIVE SPELL ANIMATION
    public AudioClip GetSpellSpawnSound() => spellBook[currentSpellIndex].SpawnSFX; // GETTER FOR ACTIVE SPELL SPAWN SOUND


    //private void CastSpawnProjectileAtPoint()
    //{
    //    GameObject projectile = Instantiate(spellBook[activeSpell].Projectile, spawnPosition.position, spawnPosition.rotation);
    //    projectile.GetComponent<ProjectileMover>().SetAttributes(spellBook[activeSpell].Damage, spellBook[activeSpell].LifeSpan, spellBook[activeSpell].MoveSpeed, spellBook[activeSpell].ProjectileSize, mousePositionTracker.CurrentPosition);
    //}
}