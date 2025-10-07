using System.Collections;
using UnityEngine;

public abstract class Spell : ScriptableObject
{
    [Header("Metadata")]
    [SerializeField] private new string name;
    [SerializeField] private string description; // UNUSED // NO GETTER
    [SerializeField] private string loreText; // UNUSED // NO GETTER

    [Header("Casting Default Attributes")]
    [SerializeField] private int defaultCurrentMana;
    [SerializeField] private int defaultMaxMana;
    [SerializeField] private int defaultDamage;
    [SerializeField] private float defaultLifeSpan;
    [Tooltip("Delay from click to end of animation to end cast. It's divided by 30 in playercontroller")]
    [SerializeField] private float defaultCastDelayTime;
    [Tooltip("Time between casts")]
    [SerializeField] private float defaultCastCooldownTime;
    [SerializeField] private float defaultMoveSpeed;
    [SerializeField] private Vector3 defaultProjectileSize;
    [SerializeField] private bool defaultDestroyOnEnemyImpact;
    [SerializeField] private bool defaultDestroyOnEnvironmentImpact;
    [SerializeField] private bool defaultDamageOverTime;
    [SerializeField] private bool defaultCanMoveDuringCast;
    [SerializeField] private bool defaultCanJumpDuringCast;

    [Header("Leveling Default Attributes")]
    [SerializeField] private int defaultCurrentLevel;
    [SerializeField] private int defaultMaxLevel;
    [SerializeField] private int defaultCurrentXP;
    [SerializeField] private int xpToLevelUp;
    [SerializeField] private int[] levelRequirements;


    /*
     * lvl 0 req 0 -> to lvl 1
     * lvl 1 req 1 -> to lvl 2
     * .......................
     * lvl 50 req 50 -> to lvl 51
     * 
     */


    [Header("NO EDIT - Current Attributes")] // TEMP. ONCE WE HAVE SAVES, THIS WILL GO AWAY AND DEFAULT WILL BECOME CURRENT
    private int currentMana;
    private int maxMana;
    private int damage;
    private float lifeSpan;
    [Tooltip("Time it takes to perform cast")]
    private float castDelayTime;
    [Tooltip("Time between casts")]
    private float castCooldownTime;
    private float moveSpeed;
    private Vector3 projectileSize;
    private bool destroyOnEnemyImpact;
    private bool destroyOnEnvironmentImpact;
    private bool damageOverTime;
    private bool canMoveDuringCast;
    private bool canJumpDuringCast;

    private int currentLevel = 0; // ALSO SERVES AS AN INDEX
    private int maxLevel;
    private int currentXP;

    [Header("Prefab")]
    [SerializeField] private GameObject projectile;

    [Header("SFX")]
    [SerializeField] private AudioClip spawnSFX;
    [Range(0f, 1f)]
    [SerializeField] private float spawnSFXVolume;
    [Range(-3f, 3f)]
    [SerializeField] private float spawnSFXPitch;
    [SerializeField] private AudioClip hitSFX;
    [Range(0f, 1f)]
    [SerializeField] private float hitSFXVolume;
    [Range(-3f, 3f)]
    [SerializeField] private float hitSFXPitch;
    [SerializeField] private GameObject hitSFXPrefab;

    [Header("Animation")]
    [SerializeField] private AnimationClip castAnimation;

    [Header("UI")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite reticle;

    [Header("Unlock Status")]
    [SerializeField] private bool isUnlocked;

    #region GETTERS
    public string Name => name;
    public int CurrentMana => currentMana;
    public int MaxMana => maxMana;
    public int Damage => damage;
    public float LifeSpan => lifeSpan;
    public float CastDelayTime => castDelayTime;
    public float CastCooldownTime => castCooldownTime;
    public float MoveSpeed => moveSpeed;
    public Vector3 ProjectileSize => projectileSize;
    public bool DestroyOnEnemyImpact => destroyOnEnemyImpact;
    public bool DestroyOnEnvironmentImpact => destroyOnEnvironmentImpact;
    public bool DamageOverTime => damageOverTime;
    public GameObject Projectile => projectile;
    public AudioClip SpawnSFX => spawnSFX;
    public float SpawnSFXVolume => spawnSFXVolume;
    public float SpawnSFXPitch => spawnSFXPitch;
    public AudioClip HitSFX => hitSFX;
    public float HitSFXPitch => hitSFXPitch;
    public float HitSFXVolume => hitSFXVolume;
    public GameObject HitSFXPrefab => hitSFXPrefab;
    public AnimationClip CastAnimation => castAnimation;
    public Sprite SpellIcon => icon;
    public Sprite Reticle => reticle;
    public bool IsUnlocked => isUnlocked;
    public bool CanMoveDuringCast => canMoveDuringCast;
    public bool CanJumpDuringCast => canJumpDuringCast;
    public int CurrentLevel => currentLevel;
    public int MaxLevel => maxLevel;
    public int CurrentXP => currentXP;
    public int XPToLevelUp => xpToLevelUp;
    #endregion

    public void Initialize()
    {
        currentMana = defaultCurrentMana;
        maxMana = defaultMaxMana;
        damage = defaultDamage;
        lifeSpan = defaultLifeSpan;
        castDelayTime = defaultCastDelayTime;
        castCooldownTime = defaultCastCooldownTime;
        moveSpeed = defaultMoveSpeed;
        projectileSize = defaultProjectileSize;
        destroyOnEnemyImpact = defaultDestroyOnEnemyImpact;
        destroyOnEnvironmentImpact = defaultDestroyOnEnvironmentImpact;
        damageOverTime = defaultDamageOverTime;
        canMoveDuringCast = defaultCanMoveDuringCast;
        canJumpDuringCast = defaultCanJumpDuringCast;

        currentLevel = defaultCurrentLevel;
        maxLevel = defaultMaxLevel;
        currentXP = defaultCurrentXP;
        xpToLevelUp = levelRequirements[0];
    }

    public void SetProjectileSize(Vector3 newValue) => projectileSize = newValue;
    public void SetMoveSpeed(float newValue) => moveSpeed = newValue;
    public void SetCastCooldownTime(float newValue) => castCooldownTime = newValue;
    public void SetDamage(int newValue) => damage = newValue;
    public void SetDestroyOnEnemyImpact(bool newValue) => destroyOnEnemyImpact = newValue;
    public void SetDestroyOnEnvironmentalImpact(bool newValue) => destroyOnEnvironmentImpact = newValue;
    public void SetDamageOverTime(bool value) => damageOverTime = value;
    public void ManaExpended() => currentMana--;

    public void SetMaxMana(int newValue)
    {
        maxMana = newValue;
        currentMana = maxMana;
    }

    public void SetCurrentXP(int newValue) => currentXP += newValue;
    public void LeveledUp() => currentLevel++;

    public void SetNextLevelUpRequirements()
    {
        if (currentLevel == maxLevel) return;

        xpToLevelUp = levelRequirements[currentLevel];
    }
}