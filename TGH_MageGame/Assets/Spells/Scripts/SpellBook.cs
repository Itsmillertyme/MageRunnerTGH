using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class SpellBook : MonoBehaviour
{
    [Header("Spell List")]
    [SerializeField] private Spell[] spellBook;

    [Header("Spell Spawn Points")]
    [Tooltip("Must be in the same order as the spells themselves")]
    [SerializeField] private Transform[] spellSpawnPoints;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent ActiveSpellSwitched;
    [SerializeField] private UnityEvent SpellCasted;

    [Header("Player Stats")]
    [SerializeField] PlayerStats playerStats;

    private GameManager gameManager;

    #region // GETTERS
    // VARIABLES
    public int ActiveSpell => currentSpellIndex;
    public bool IsReadyToCast => isReadyToCast;

    // METHODS
    //public Transform GetSpellSpawnPoint() => spellSpawnPoints[currentSpellIndex]; // GETTER FOR ACTIVE SPELL SPAWN POINT FOR CAST
    public string GetSpellUIData() => spellBook[currentSpellIndex].Name; // GETTER FOR ACTIVE SPELL TO USE IN UI TEXT
    public Sprite GetSpellIconData() => spellBook[currentSpellIndex].SpellIcon; // GETTER FOR ACTIVE SPELL ICON TO USE IN UI
    public Sprite GetSpellReticleData() => spellBook[currentSpellIndex].Reticle; // GETTER FOR ACTIVE SPELL RETICLE TO USE IN UI
    public AnimationClip GetSpellAnimation() => spellBook[currentSpellIndex].CastAnimation; // GETTER FOR ACTIVE SPELL ANIMATION
    public float GetSpellCastDelayTime() => spellBook[currentSpellIndex].CastDelayTime; // GETTER FOR ACTIVE SPELL ANIMATION
    public AudioClip GetSpellSpawnSound() => spellBook[currentSpellIndex].SpawnSFX; // GETTER FOR ACTIVE SPELL SPAWN SOUND
    #endregion

    #region // DRIVEN
    private Transform currentSpawnPoint;
    private float scrollValue;
    private Coroutine castCooldown; // later implenent coroutine stopping for interrupted delay when cycling away (if desired)
    private int currentSpellIndex = 0;
    private bool isReadyToCast = true;
    private int lastActiveSpell;
    #endregion

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();

        foreach (Spell spell in spellBook)
        {
            spell.Initialize();
        }

        currentSpawnPoint = GetSpellSpawnPosition();
    }

    private void Update() // DID WE GET AN INPUT SYSTEM INPUT FOR THIS?
    {
         scrollValue = Input.mouseScrollDelta.y; // REFACTOR TO NEW INPUT WHEN MERGED

        // TEMP WORKAROUND UNTIL INPUT SYSTEM METHOD IS PRESENT
        if (scrollValue != 0)
        {
            SetSpell();
        }
    }

    public Transform GetSpellSpawnPosition()
    {
        // POSITIONS: 0 IS RH, 1 IS LH, 2 IS CHEST, 3 IS GROUND, 4 IS SKY
        switch (spellBook[currentSpellIndex])
        {
            case AbyssalFang:
                currentSpawnPoint = spellSpawnPoints[0];
                break;
            case HeavensLament:
                currentSpawnPoint = spellSpawnPoints[2];
                break;
            case InfernalEmbrace:
                currentSpawnPoint = spellSpawnPoints[0];
                break;
            case ShatterstoneBarrage:
                currentSpawnPoint = spellSpawnPoints[3];
                break;
            case ThunderlordsCascade:
                currentSpawnPoint = spellSpawnPoints[4];
                break;
            case WintersWrath:
                currentSpawnPoint = spellSpawnPoints[3];
                break;
        }

        return currentSpawnPoint;
    }

    public void Cast()
    {
        if (isReadyToCast && playerStats.getCurrentMana() >= spellBook[currentSpellIndex].ManaCost)
        {
            HandleSpellLogic();

            //spellBook[currentSpellIndex].Cast(currentSpawnPoint.position, gameManager.CrosshairPositionIn3DSpace);

            //// ADDITIONAL CASTING LOGIC FOR ABYSSAL FANG SPELL
            //if (spellBook[currentSpellIndex] is AbyssalFang spell)
            //{
            //    StartCoroutine(CastAltHandAfterCooldown(spell.CastAltHandCooldownTime));
            //}

            castCooldown = StartCoroutine(CastCooldown(spellBook[ActiveSpell].CastCooldownTime));
            SpellCasted.Invoke();
            playerStats.updateCurrentMana(-spellBook[currentSpellIndex].ManaCost);
        }
    }

    private void HandleSpellLogic()
    {
        switch (spellBook[currentSpellIndex])
        {
            case AbyssalFang abf:
                //
                CastAbyssalFang(currentSpawnPoint.position, gameManager.CrosshairPositionIn3DSpace);
                StartCoroutine(CooldownThenCastAltHandAbyssalFang(abf.CastAltHandCooldownTime));
                break;
            case HeavensLament hl:
                //
                break;
            case InfernalEmbrace ie:
                //
                break;
            case ShatterstoneBarrage sb:
                //
                break;
            case ThunderlordsCascade tc:
                //
                break;
            case WintersWrath ww:
                //
                break;
        }
    }

    #region ABYSSAL FANG
    public void CastAbyssalFang(Vector3 position, Vector3 direction)
    {
        GameObject newProjectile = Instantiate(spellBook[currentSpellIndex].Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<ProjectileMover>().SetAttributes(spellBook[currentSpellIndex].Damage, spellBook[currentSpellIndex].LifeSpan, spellBook[currentSpellIndex].MoveSpeed, spellBook[currentSpellIndex].ProjectileSize, direction);
    }

    public IEnumerator CooldownThenCastAltHandAbyssalFang(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        CastAltHand();
    }

    public void CastAltHand()
    {
        CastAbyssalFang(spellSpawnPoints[1].position, gameManager.CrosshairPositionIn3DSpace);
    }
    #endregion

    #region HEAVEN'S LAMENT
    #endregion

    #region INFERNAL EMBRACE
    #endregion

    #region SHATTERSTONE BARRAGE
    #endregion

    #region THUNDERLORD'S CASCADE
    #endregion

    #region WINTER'S WRATH
    #endregion

    // TEST TO SEE HOW THIS WORKS WITH SWITCHING SPELLS. HARD TO TEST WITH TOUCHPAD.
    public IEnumerator CastCooldown(float waitTime)
    {
        isReadyToCast = false;
        yield return new WaitForSeconds(waitTime);
        isReadyToCast = true;
    }

    // SEE IF JACOB NEEDS THIS. 
    //public IEnumerator CastDelay(float waitTime)
    //{
    //    yield return new WaitForSeconds(waitTime);
    //}


    // REVIEW THIS
    // SPELL INVENTORY CYCLING
    private void SetSpell()
    {
        // IF SCROLLING THE MOUSE WHEEL
        if (scrollValue < 0f)
        {
            do
            {
                lastActiveSpell = currentSpellIndex;
                currentSpellIndex++;

                if (currentSpellIndex >= spellBook.Length)
                {
                    currentSpellIndex = 0;
                }
            }
            while (!spellBook[currentSpellIndex].IsUnlocked);
        }

        else if (scrollValue > 0f)
        {
            do
            {
                lastActiveSpell = currentSpellIndex;
                currentSpellIndex--;

                if (currentSpellIndex < 0)
                {
                    currentSpellIndex = spellBook.Length - 1;
                }
            }
            while (!spellBook[currentSpellIndex].IsUnlocked);
        }

        // SET SPELL SPAWN POINT
        currentSpawnPoint = GetSpellSpawnPosition();

        // RAISE AN EVENT THAT THE SPELL SELECTION HAS CHANGED
        ActiveSpellSwitched.Invoke();
    }
    
    // REVIEW THIS
    // SET SPELL DIRECTLY
    public void SetSpellByIndex(int newSpellIndex)
    {
        //Validate input
        if (newSpellIndex < 0 || newSpellIndex > spellBook.Length)
        {
            return;
        }

        lastActiveSpell = currentSpellIndex;
        currentSpellIndex = newSpellIndex;

        // SET SPELL SPAWN POINT
        currentSpawnPoint = GetSpellSpawnPosition();

        // RAISE AN EVENT THAT THE SPELL SELECTION HAS CHANGED
        ActiveSpellSwitched.Invoke();
    }

    // REVIEW THIS.
    public void HotSwitchSpell()
    {
        SetSpellByIndex(lastActiveSpell);
    }
}