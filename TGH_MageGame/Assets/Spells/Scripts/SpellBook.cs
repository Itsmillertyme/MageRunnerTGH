using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
            // THUNDERLORDS CASCADE DOESN'T NEED A SPAWN POINT
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

            castCooldown = StartCoroutine(CastCooldown(spellBook[ActiveSpell].CastCooldownTime));
            SpellCasted.Invoke();
            playerStats.updateCurrentMana(-spellBook[currentSpellIndex].ManaCost);
        }
    }

    private void HandleSpellLogic()
    {
        switch (spellBook[currentSpellIndex])
        {
            case AbyssalFang af:
                CastAbyssalFang(af, currentSpawnPoint.position, gameManager.CrosshairPositionIn3DSpace);
                StartCoroutine(CooldownThenCastAltHandAbyssalFang(af, af.CastAltHandCooldownTime));
                break;
            //case HeavensLament hl:
            //    //
            //    break;
            //case InfernalEmbrace ie:
            //    //
            //    break;
            case ShatterstoneBarrage sb:
                StartCoroutine(ShatterstoneSpawnProjectiles(sb));
                break;
            case ThunderlordsCascade tc:
                StartCoroutine(CastThunderlordsCascade(tc, gameManager.CrosshairPositionIn3DSpace));
                break;
                //case WintersWrath ww:
                //    //
                //    break;
        }
    }

    #region ABYSSAL FANG
    public void CastAbyssalFang(AbyssalFang af, Vector3 position, Vector3 direction)
    {
        GameObject newProjectile = Instantiate(spellBook[currentSpellIndex].Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<AbyssalFangProjectileMovement>().SetAttributes(spellBook[currentSpellIndex].MoveSpeed, spellBook[currentSpellIndex].ProjectileSize, direction);
        newProjectile.GetComponent<EnemyDamager>().SetAttributes(af.Damage, af.LifeSpan);
    }

    public IEnumerator CooldownThenCastAltHandAbyssalFang(AbyssalFang af, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        CastAbyssalFang(af, spellSpawnPoints[1].position, gameManager.CrosshairPositionIn3DSpace);
    }
    #endregion

    #region HEAVEN'S LAMENT
    #endregion

    #region INFERNAL EMBRACE
    #endregion

    #region SHATTERSTONE BARRAGE
    private IEnumerator ShatterstoneSpawnProjectiles(ShatterstoneBarrage sb)
    {
        for (int i = 0; i < sb.ProjectileCount; i++)
        {
            // CREATE RANDOM OFFSET FROM BASE SPAWN POSITION
            Vector3 spawnOffset = Random.insideUnitSphere * sb.SpawnRadius; // RANDOM OFFSET POSITION AROUND THE SPAWN CENTER
            //spawnOffset.y = 0; // CLAMP TO 0 TO REMOVE RANDOMNESS OF VERTICAL SPAWN POSITION
            Vector3 spawnPosition = currentSpawnPoint.position + spawnOffset; // SPAWN POSITION PLUS RANDOMNESS ON X

            // CREATE RANDOM TYPE OF PROJECTILE WITH RANDOM ROTATIONS
            GameObject projectile = Instantiate(sb.Prefabs[Random.Range(0, sb.Prefabs.Length)], spawnPosition, Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))); // RANDOM ROTATIONS
            
            // INVOKE MOVEMENT LOGIC
            StartCoroutine(projectile.GetComponent<ShatterstoneBarrageProjectileMovement>().ShatterstoneMoveProjectile(sb, spawnOffset));
            
            // WAIT BETWEEN EACH PROJECTILE SPAWN
            yield return new WaitForSeconds(sb.DelayBetweenSpawns); 
        }
    }
    #endregion

    #region THUNDERLORD'S CASCADE
    private IEnumerator CastThunderlordsCascade(ThunderlordsCascade tc, Vector3 direction) // NO SPAWN POSITION. ALL FROM DIRECTION. 
    {
        float boltSpacing = tc.BoltSpread / (tc.BoltCount - 1);

        for (int i = 0; i < tc.VolleyCount; i++)
        {
            Vector3 spawnPosition = new(direction.x - (tc.BoltSpread / 2), 0, direction.z); // CLAMP Y TO 0

            for (int j = 0; j < tc.BoltCount; j++)
            {
                GameObject newProjectile = Instantiate(spellBook[currentSpellIndex].Projectile, spawnPosition, Quaternion.identity);
                spawnPosition = new(spawnPosition.x + boltSpacing, 0, spawnPosition.z);
                SetThunderlordsCascadeProjectile(tc, newProjectile);
            }

            yield return new WaitForSeconds(tc.VolleyCooldown);
        }
    }

    private void SetThunderlordsCascadeProjectile(ThunderlordsCascade tc, GameObject gameObject)
    {
        // ENEMY DAMAGER
        gameObject.GetComponent<EnemyDamager>().SetAttributes(tc.Damage, tc.LifeSpan);

        // PARTICLE SYSTEM REFERENCE
        ParticleSystem particleSystem = gameObject.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule particle = particleSystem.main;

        // START DELAY
        particle.startDelay = Random.Range(0f, tc.BoltSpawnDelay);

        // START ROTATION
        float rotation = Random.Range(-tc.BoltAngularSpread, tc.BoltAngularSpread) * Mathf.Deg2Rad; // CONVERT UNITS
        particle.startRotation = rotation;
    } 
    #endregion

    #region WINTER'S WRATH
    #endregion

    // TEST TO SEE HOW THIS WORKS WITH SWITCHING SPELLS. HARD TO TEST WITH TOUCHPAD.
    private IEnumerator CastCooldown(float waitTime)
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