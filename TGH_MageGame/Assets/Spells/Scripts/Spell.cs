using System.Collections;
using UnityEngine;

public abstract class Spell : ScriptableObject
{
    [Header("Metadata")]
    [SerializeField] private new string name;
    [SerializeField] private string description; // UNUSED // NO GETTER
    [SerializeField] private string loreText; // UNUSED // NO GETTER

    [Header("Default Attributes")]
    [SerializeField] private int defaultManaCost;
    [SerializeField] private int defaultDamage;
    [SerializeField] private float defaultLifeSpan;
    [Tooltip("Time it takes to perform cast")]
    [SerializeField] private float defaultCastDelayTime;
    [Tooltip("Time between casts")]
    [SerializeField] private float defaultCastCooldownTime;
    [SerializeField] private float defaultMoveSpeed;
    [SerializeField] private Vector3 defaultProjectileSize;
    [SerializeField] private bool defaultDestroyOnEnemyImpact;
    [SerializeField] private bool defaultDestroyOnEnvironmentImpact;
    [SerializeField] private bool defaultDamageOverTime;

    [Header("NO EDIT - Current Attributes")] // TEMP. ONCE WE HAVE SAVES, THIS WILL GO AWAY AND DEFAULT WILL BECOME CURRENT
    private int manaCost;
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

    [Header("Prefab")]
    [SerializeField] private GameObject projectile;

    [Header("SFX")]
    [SerializeField] private AudioClip spawnSFX;
    [SerializeField] private AudioClip hitSFX; // UNUSED // NO GETTER

    [Header("Animation")]
    [SerializeField] private AnimationClip castAnimation;

    [Header("UI")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Sprite reticle;

    [Header("Unlock Status")]
    [SerializeField] private bool isUnlocked;

    [Header("References Gathered On Awake")]
    private PlayerStats playerStats;

    private void OnEnable()
    {
        playerStats = FindFirstObjectByType<SpellBook>().PlayerStats;
    }

    #region GETTERS
    public string Name => name;
    public int ManaCost => manaCost;
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
    public AnimationClip CastAnimation => castAnimation;
    public Sprite SpellIcon => icon;
    public Sprite Reticle => reticle;
    public bool IsUnlocked => isUnlocked;
    #endregion

    public void Initialize()
    {
        manaCost = defaultManaCost;
        damage = defaultDamage + playerStats.baseAttackDamage; // xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx waiting on feedback
        lifeSpan = defaultLifeSpan;
        castDelayTime = defaultCastDelayTime;
        castCooldownTime = defaultCastCooldownTime;
        moveSpeed = defaultMoveSpeed;
        projectileSize = defaultProjectileSize;
        destroyOnEnemyImpact = defaultDestroyOnEnemyImpact;
        destroyOnEnvironmentImpact = defaultDestroyOnEnvironmentImpact;
        damageOverTime = defaultDamageOverTime;
    }    

    public void SetManaCost(int newValue) => manaCost = newValue;
    public void SetProjectileSize(Vector3 newValue) => projectileSize = newValue;

    public void SetMoveSpeed(float newValue) => moveSpeed = newValue;

    public void SetCastCooldownTime(float newValue) => castCooldownTime = newValue;

    public void SetDamage(int newValue) => damage = newValue;

    public void SetDestroyOnEnemyImpact(bool newValue) => destroyOnEnemyImpact = newValue;

    public void SetDestroyOnEnvironmentalImpact(bool newValue) => destroyOnEnvironmentImpact = newValue;

    public void SetDamageOverTime(bool value) => damageOverTime = value;
}