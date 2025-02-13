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

    [Header("NO EDIT - Current Attributes")]
    [SerializeField] private int manaCost;
    [SerializeField] private int damage;
    [SerializeField] private float lifeSpan;
    [Tooltip("Time it takes to perform cast")]
    [SerializeField] private float castDelayTime;
    [Tooltip("Time between casts")]
    [SerializeField] private float castCooldownTime;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Vector3 projectileSize;

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

    //[Header("Debugging")]
    //private Vector3 targetPosition;

    #region GETTERS
    [Header("Metadata")]
    public string Name => name;

    [Header("NO EDIT - Current Attributes")]
    public int ManaCost => manaCost;
    public int Damage => damage;
    public float LifeSpan => lifeSpan;
    public float CastDelayTime => castDelayTime;
    public float CastCooldownTime => castCooldownTime;
    public float MoveSpeed => moveSpeed;
    public Vector3 ProjectileSize => projectileSize;

    [Header("Prefab")]
    public GameObject Projectile => projectile;

    [Header("SFX")]
    public AudioClip SpawnSFX => spawnSFX;

    [Header("Animation")]
    public AnimationClip CastAnimation => castAnimation;

    [Header("UI")]
    public Sprite SpellIcon => icon;
    public Sprite Reticle => reticle;

    [Header("Unlock Status")]
    public bool IsUnlocked => isUnlocked;

    //[Header("Debugging")]
    //public Vector3 TargetPosition => targetPosition;
    #endregion

    public void SetDefaultValues()
    {
        manaCost = defaultManaCost;
        damage = defaultDamage;
        lifeSpan = defaultLifeSpan;
        castDelayTime = defaultCastDelayTime;
        castCooldownTime = defaultCastCooldownTime;
        moveSpeed = defaultMoveSpeed;
        projectileSize = defaultProjectileSize;
    }    
    
    public abstract void Cast(Vector3 position, Vector3 direction);
}