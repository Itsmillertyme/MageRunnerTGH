using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Shatterstone Barrage")]

public class ShatterstoneBarrage : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private int defaultProjectileCount = 10;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private float riseHeight = 5.5f;
    [SerializeField] private float defaultRiseSpeed = 1.3f;
    [SerializeField] private float defaultDelayBetweenSpawns = 0.15f;
    [SerializeField] private float riseMaxTime = 1.25f;
    [SerializeField] private float pauseBetweenPhases = 1.1f;
    [SerializeField] private float riseHeightVariation = 1.25f;
    [Tooltip("1.0 equals 100%")]
    [SerializeField] private float defaultProjectileSizeScalar = 1f;
    [SerializeField] private GameObject[] prefabs;

    private int projectileCount;
    private float riseSpeed;
    private float delayBetweenSpawns;
    private float projectileSizeScalar;

    private void OnEnable()
    {
        projectileCount = defaultProjectileCount;
        riseSpeed = defaultRiseSpeed;
        delayBetweenSpawns = defaultDelayBetweenSpawns;
        projectileSizeScalar = defaultProjectileSizeScalar;
    }

    // GETTERS
    public int ProjectileCount => projectileCount;
    public float SpawnRadius => spawnRadius;
    public float RiseHeight => riseHeight;
    public float RiseSpeed => riseSpeed;
    public float DelayBetweenSpawns => delayBetweenSpawns;
    public float RiseMaxTime => riseMaxTime;
    public float PauseBetweenPhases => pauseBetweenPhases;
    public float RiseHeightVariation => riseHeightVariation;
    public float ProjectileSizeScalar => projectileSizeScalar;
    public GameObject[] Prefabs => prefabs;

    public void SetProjectileCount(int newValue) => projectileCount = newValue;

    public void SetRiseSpeed(float newValue) => riseSpeed = newValue;

    public void SetDelayBetweenSpawns(float newValue) => delayBetweenSpawns = newValue;

    public void SetProjectileSizeScalar(float newValue) => projectileSizeScalar = newValue;
}