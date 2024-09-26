using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;
using UnityEngine.UI;

[CreateAssetMenu]

public class Spell : ScriptableObject
{
    public enum MoveMethod
    {
        SpawnAtPoint,
        SpawnAbove,
        SpawnAndHold
    }

    [Header("Information")]
    [SerializeField] private new string name;
    [SerializeField] private string description;
    [SerializeField] private string loreText;

    [Header("Levels")]
    [SerializeField] private int currentLevel;
    [SerializeField] private int maxLevel;

    [Header("Current Spell Attributes")]
    //[SerializeField] private int projectileCount;
    [SerializeField] private int manaCost;
    [SerializeField] private int damage; // TOTAL COMBINED DAMAGE
    [SerializeField] private float lifeSpan;
    [SerializeField] private float castDelayTime;
    //[SerializeField] private float damageDuration;
    [SerializeField] private float moveSpeed;
    //[SerializeField] private float impactForce;
    //[SerializeField] private float range;
    [SerializeField] private Vector3 projectileSize;
    [SerializeField] private MoveMethod moveMethod;

    [Header("Prefab")]
    [SerializeField] private GameObject spawnObjectLvl1;
    [SerializeField] private GameObject spawnObjectLvl2;
    [SerializeField] private GameObject spawnObjectLvl3;
    
    [Header("SFX")]
    [SerializeField] private AudioClip spawnSFX;
    [SerializeField] private AudioClip hitSFX;

    [Header("Animation")]
    [SerializeField] private AnimationClip castAnimation;

    //[Header("FX")]
    //[SerializeField] private ParticleSystem spawnFX;
    //[SerializeField] private ParticleSystem hitFX;

    [Header("UI Icon")]
    [SerializeField] private Sprite icon;

    // Projectile Cast Method (can't do headers on Enums)

    //[SerializeField] private enum Element { Air, Earth, Fire, Water }
    [Header("Unlock Status")]
    [SerializeField] private bool isUnlocked;

    [Header("Debugging")]
    private Vector3 targetPosition;

    [Header("Level Specific Attributes")]
    [Tooltip("These are assigned to the values up above. The ones above act as current values")]
    [Header("Level 1 Attributes")]
    //[SerializeField] private int level1_ProjectileCount;
    [SerializeField] private int level1_ManaCost;
    [SerializeField] private int level1_Damage; // TOTAL COMBINED DAMAGE
    [SerializeField] private float level1_LifeSpan;
    [SerializeField] private float level1_CastDelayTime;
    //[SerializeField] private float level1_DamageDuration;
    [SerializeField] private float level1_MoveSpeed;
    //[SerializeField] private float level1_ImpactForce;
    //[SerializeField] private float level1_Range;
    [SerializeField] private Vector3 level1_ProjectileSize;

    [Header("Level 2 Attributes")]
    //[SerializeField] private int level2_ProjectileCount;
    [SerializeField] private int level2_ManaCost;
    [SerializeField] private int level2_Damage; // TOTAL COMBINED DAMAGE
    [SerializeField] private float level2_LifeSpan;
    [SerializeField] private float level2_CastDelayTime;
    //[SerializeField] private float level2_DamageDuration;
    [SerializeField] private float level2_MoveSpeed;
    //[SerializeField] private float level2_ImpactForce;
    //[SerializeField] private float level2_Range;
    [SerializeField] private Vector3 level2_ProjectileSize;

    [Header("Level 3 Attributes")]
    //[SerializeField] private int level3_ProjectileCount;
    [SerializeField] private int level3_ManaCost;
    [SerializeField] private int level3_Damage; // TOTAL COMBINED DAMAGE
    [SerializeField] private float level3_LifeSpan;
    [SerializeField] private float level3_CastDelayTime;
    //[SerializeField] private float level3_DamageDuration;
    [SerializeField] private float level3_MoveSpeed;
    //[SerializeField] private float level3_ImpactForce;
    //[SerializeField] private float level3_Range;
    [SerializeField] private Vector3 level3_ProjectileSize;

    [Header("Unity Events")]
    [SerializeField] private UnityEvent spellLeveledUp;
    [SerializeField] private UnityEvent spellLeveledDown;


    #region // GETTERS
    public int CurrentLevel => currentLevel;
    public int ManaCost => manaCost;
    public int Damage => damage;
    public float MoveSpeed => moveSpeed;
    public float LifeSpan => lifeSpan;
    public float CastDelayTime => castDelayTime;
    public string Name => name;
    public AnimationClip CastAnimation => castAnimation;
    public Vector3 TargetPosition => targetPosition;
    public bool IsUnlocked => isUnlocked;
    public GameObject SpawnObjectLvl1 => spawnObjectLvl1;
    public GameObject SpawnObjectLvl2 => spawnObjectLvl2;
    public GameObject SpawnObjectLvl3 => spawnObjectLvl3;
    public Sprite SpellIcon => icon;
    public AudioClip SpawnSFX => spawnSFX;
    public AudioClip HitSFX => hitSFX;
    #endregion

    public void LevelUp()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            SetLevelAttributes(currentLevel);
            spellLeveledUp.Invoke();
        }
    }

    // TEMP for debugging
    public void LevelDown()
    {
        if (currentLevel > 1)
        {
            currentLevel--;
            SetLevelAttributes(currentLevel);
            spellLeveledDown.Invoke();
        }
    }

    public void SetLevelAttributes(int level)
    {
        switch (level)
        {
            case 1:
                // 
                manaCost = level1_ManaCost;
                damage = level1_Damage; 
                lifeSpan = level1_LifeSpan;
                castDelayTime = level1_CastDelayTime;
                //
                moveSpeed = level1_MoveSpeed;
                //
                //
                projectileSize = level1_ProjectileSize;
                break;
            case 2:
                // 
                manaCost = level2_ManaCost;
                damage = level2_Damage;
                lifeSpan = level2_LifeSpan;
                castDelayTime = level2_CastDelayTime;
                //
                moveSpeed = level2_MoveSpeed;
                //
                //                
                projectileSize = level2_ProjectileSize;
                break;
            case 3:
                // 
                manaCost = level3_ManaCost;
                damage = level3_Damage;
                lifeSpan = level3_LifeSpan;
                castDelayTime = level3_CastDelayTime;
                //
                moveSpeed = level3_MoveSpeed;
                //
                //
                projectileSize = level3_ProjectileSize;
                break;
        }
    }
}