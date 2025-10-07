using System.Collections;
using UnityEngine;

public class SpellBook : MonoBehaviour
{
    [Header("Spell List")]
    [SerializeField] private Spell[] spellBook;

    [Header("Spell Spawn Points")]
    [Tooltip("Must be in the same order as the spells themselves")]
    [SerializeField] private Transform[] spellSpawnPoints;

    private SpellUI spellUI;
    private SpellLevels spellLevels;
    private GameManager gameManager;
    private LightingController lightingController;

    #region // GETTERS
    public int CurrentSpellIndex => currentSpellIndex;
    public bool IsReadyToCast => isReadyToCast;
    public string GetSpellUIData() => $"{spellBook[currentSpellIndex].CurrentMana} / {spellBook[currentSpellIndex].MaxMana}"; // GETTER FOR ACTIVE SPELL TO USE IN UI TEXT
    public Sprite GetSpellIconData() => spellBook[currentSpellIndex].SpellIcon; // GETTER FOR ACTIVE SPELL ICON TO USE IN UI
    public Sprite GetSpellReticleData() => spellBook[currentSpellIndex].Reticle; // GETTER FOR ACTIVE SPELL RETICLE TO USE IN UI
    public AnimationClip GetSpellAnimation() => spellBook[currentSpellIndex].CastAnimation; // GETTER FOR ACTIVE SPELL ANIMATION
    public float GetSpellCastDelayTime() => spellBook[currentSpellIndex].CastDelayTime; // GETTER FOR CAST DELAY TIME
    public float GetSpellManaCost() => spellBook[currentSpellIndex].CurrentMana; // GETTER FOR ACTIVE SPELL MANA COST
    public AudioClip GetSpellSpawnSound() => spellBook[currentSpellIndex].SpawnSFX; // GETTER FOR ACTIVE SPELL SPAWN SOUND
    public float GetSpellSpawnVolume() => spellBook[currentSpellIndex].SpawnSFXVolume; // GETTER FOR ACTIVE SPELL SFX VOLUME
    public float GetSpellSpawnPitch() => spellBook[currentSpellIndex].SpawnSFXPitch + UtilityTools.RandomVarianceFloat(); // GETTER FOR ACTIVE SPELL SFX PITCH
    public float GetXPBarProgress() => (float)spellBook[currentSpellIndex].CurrentXP / (float)spellBook[currentSpellIndex].XPToLevelUp;
    public int GetSpellCurrentLevel() => spellBook[currentSpellIndex].CurrentLevel;
    #endregion

    #region // DRIVEN
    private Transform currentSpawnPoint;
    private float scrollValue;
    private Coroutine castCooldown;
    private int currentSpellIndex = 0;
    private bool isReadyToCast = true;
    private int previousSpellIndex;
    #endregion

    private void Awake()
    {
        spellUI = FindFirstObjectByType<SpellUI>();
        spellLevels = FindFirstObjectByType<SpellLevels>();
        gameManager = FindFirstObjectByType<GameManager>();
        lightingController = gameManager.GetComponent<LightingController>();

        foreach (Spell spell in spellBook)
        {
            spell.Initialize();
        }

        UpdateUI();
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
        // POSITIONS: 0 IS LH, 1 IS RH, 2 IS CHEST, 3 IS GROUND, 4 IS SKY
        switch (spellBook[currentSpellIndex])
        {
            case AbyssalFang:
                currentSpawnPoint = spellSpawnPoints[0];
                break;
            //case HeavensLament:
            //    currentSpawnPoint = spellSpawnPoints[2];
            //    break;
            //case InfernalEmbrace:
            //    currentSpawnPoint = spellSpawnPoints[0];
            //    break;
            case ShatterstoneBarrage:
                currentSpawnPoint = spellSpawnPoints[3];
                break;
            case ThunderlordsCascade:
                break;
            //case WintersWrath:
            //    currentSpawnPoint = spellSpawnPoints[3];
            //    break;
        }

        return currentSpawnPoint;
    }

    public void Cast()
    {
        if (isReadyToCast && spellBook[currentSpellIndex].CurrentMana > 0)
        {
            HandleSpellLogic();

            castCooldown = StartCoroutine(CastCooldown(spellBook[CurrentSpellIndex].CastCooldownTime));
            spellBook[CurrentSpellIndex].ManaExpended();
            UpdateUI();
        }
    }

    private void HandleSpellLogic()
    {
        switch (spellBook[currentSpellIndex])
        {
            case AbyssalFang af:
                CastAbyssalFang(af, currentSpawnPoint.position, gameManager.CrosshairPositionIn3DSpace);
                isReadyToCast = false;
                StartCoroutine(CooldownThenCastAltHandAbyssalFang(af, af.CastAltHandCooldownTime));
                break;
            case ShatterstoneBarrage sb:
                StartCoroutine(ShatterstoneSpawnProjectiles(sb));
                break;
            case ThunderlordsCascade tc:
                StartCoroutine(CastThunderlordsCascade(tc));
                break;
        }
    }

    #region ABYSSAL FANG
    public void CastAbyssalFang(AbyssalFang af, Vector3 position, Vector3 direction)
    {
        GameObject newProjectile = Instantiate(af.Projectile, position, Quaternion.identity);
        newProjectile.GetComponent<AbyssalFangProjectileMovement>().SetAttributes(af.MoveSpeed, af.ProjectileSize, direction);
        newProjectile.GetComponent<EnemyDamager>().SetAttributes(af);
    }

    public IEnumerator CooldownThenCastAltHandAbyssalFang(AbyssalFang af, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        CastAbyssalFang(af, spellSpawnPoints[1].position, gameManager.CrosshairPositionIn3DSpace);
    }
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
            projectile.GetComponent<ShatterstoneBarrageProjectileMovement>().SetProjectileSize(sb.ProjectileSizeScalar);

            // INVOKE MOVEMENT LOGIC
            StartCoroutine(projectile.GetComponent<ShatterstoneBarrageProjectileMovement>().ShatterstoneMoveProjectile(sb, spawnOffset));

            // WAIT BETWEEN EACH PROJECTILE SPAWN
            yield return new WaitForSeconds(sb.DelayBetweenSpawns);
        }
    }
    #endregion

    #region THUNDERLORD'S CASCADE
    private IEnumerator CastThunderlordsCascade(ThunderlordsCascade tc) // NO SPAWN POSITION. ALL FROM DIRECTION. 
    {
        // DIM THE LIGHTS DURING SPELL
        lightingController.DimLights(tc.DimTimeFrame, tc.DimIntensityScale);

        // BEGIN CASTING LOGIC
        float boltSpacing = tc.BoltSpread / (tc.BoltCount - 1);

        for (int i = 0; i < tc.VolleyCount; i++)
        {
            Vector3 spawnPosition = new(gameManager.CrosshairPositionIn3DSpace.x - (tc.BoltSpread / 2), gameManager.Player.position.y, gameManager.CrosshairPositionIn3DSpace.z);

            for (int j = 0; j < tc.BoltCount; j++)
            {
                GameObject newProjectile = Instantiate(tc.Projectile, spawnPosition, Quaternion.identity);
                spawnPosition = new(spawnPosition.x + boltSpacing, spawnPosition.y, spawnPosition.z);
                SetThunderlordsCascadeProjectile(tc, newProjectile);
            }

            yield return new WaitForSeconds(tc.VolleyCooldown);
        }
    }

    private void SetThunderlordsCascadeProjectile(ThunderlordsCascade tc, GameObject gameObject)
    {
        // ENEMY DAMAGER
        gameObject.GetComponentInChildren<EnemyDamager>().SetAttributes(tc);
        Destroy(gameObject, tc.LifeSpan); // DESTROY ON DAMAGER DOESN'T WORK BECAUSE COLLISION LOGIC IS ON CHILD

        // PARTICLE SYSTEM REFERENCE SETTING FOR MAIN EFFECT
        ParticleSystem particleSystemMain = gameObject.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule particleMain = particleSystemMain.main;

        // PARTICLE SYSTEM REFERENCE SETTING FOR IMPACT EFFECTS
        ThunderlordsCascadeProjectileMovement tcpm = gameObject.GetComponent<ThunderlordsCascadeProjectileMovement>();
        ParticleSystem sparksFX = tcpm.SparksFX;
        ParticleSystem dustCloudFX = tcpm.DustCloudFX;
        ParticleSystem dustDebrisFX = tcpm.DustDebrisFX;
        ParticleSystem.MainModule particleSparks = sparksFX.main;
        ParticleSystem.MainModule particleDustCloud = dustCloudFX.main;
        ParticleSystem.MainModule particleDustDebris = dustDebrisFX.main;

        // START DELAY
        float startDelay = Random.Range(0f, tc.BoltSpawnDelay);
        particleMain.startDelay = startDelay;
        particleSparks.startDelay = startDelay;
        particleDustCloud.startDelay = startDelay;
        particleDustDebris.startDelay = startDelay;


        // 3D START ROTATION
        float zRotation = Random.Range(-tc.BoltAngularSpread, tc.BoltAngularSpread) * Mathf.Deg2Rad;
        particleMain.startRotationZ = zRotation;

        // COLLIDER
        BoxCollider boltCollider = gameObject.GetComponentInChildren<BoxCollider>();
        float xSize = particleMain.startSizeX.constantMax;
        float ySize = particleMain.startSizeY.constantMax;
        float zSize = particleMain.startSizeZ.constantMax;

        boltCollider.size = new(xSize, ySize, zSize);
        boltCollider.center = new(0, ySize / 2, 0);
        boltCollider.gameObject.transform.rotation = Quaternion.Euler(0, 0, zRotation * Mathf.Rad2Deg);
    }
    #endregion

    // TEST TO SEE HOW THIS WORKS WITH SWITCHING SPELLS. HARD TO TEST WITH TOUCHPAD.
    private IEnumerator CastCooldown(float waitTime)
    {
        isReadyToCast = false;
        yield return new WaitForSeconds(waitTime);

        // SET isReadyToCast
        isReadyToCast = SetIsReadyToCast();
    }

    // REVIEW THIS
    // SPELL INVENTORY CYCLING
    private void SetSpell()
    {
        // IF SCROLLING THE MOUSE WHEEL
        if (scrollValue < 0f)
        {
            do
            {
                previousSpellIndex = currentSpellIndex;
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
                previousSpellIndex = currentSpellIndex;
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

        // SET isReadyToCast
        isReadyToCast = SetIsReadyToCast();

        // UPDATE LEVELING SYSTEM INDEX
        spellLevels.SetIndex(currentSpellIndex);

        // UPDATE UI
        UpdateUI();
    }

    //// REVIEW THIS
    //// SET SPELL DIRECTLY
    //public void SetSpellByIndex(int newSpellIndex)
    //{
    //    //Validate input
    //    if (newSpellIndex < 0 || newSpellIndex > spellBook.Length)
    //    {
    //        return;
    //    }

    //    lastActiveSpell = currentSpellIndex;
    //    currentSpellIndex = newSpellIndex;

    //    // SET SPELL SPAWN POINT
    //    currentSpawnPoint = GetSpellSpawnPosition();

    //    // RAISE AN EVENT THAT THE SPELL SELECTION HAS CHANGED
    //    ActiveSpellSwitched.Invoke();
    //}

    //// REVIEW THIS.
    public void HotSwitchSpell()
    {
        //SetSpellByIndex(lastActiveSpell);
    }

    private bool SetIsReadyToCast()
    {
        if (spellBook[CurrentSpellIndex].CurrentMana > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void UpdateUI()
    {
        spellUI.UpdateSpellUI(GetSpellUIData(), GetSpellIconData(), GetSpellReticleData(), GetXPBarProgress(), GetSpellCurrentLevel());
    }
}