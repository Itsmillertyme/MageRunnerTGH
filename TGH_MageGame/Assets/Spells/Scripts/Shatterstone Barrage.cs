using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Shatterstone Barrage")]

public class ShatterstoneBarrage : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private int projectileCount = 10;
    [SerializeField] private float spawnRadius = 3f;
    [SerializeField] private float riseHeight = 5.5f;
    [SerializeField] private float riseSpeed = 1.3f;
    [SerializeField] private float attackSpeed = 32f;
    [SerializeField] private float delayBetweenSpawns = 0.15f;
    [SerializeField] private float riseMaxTime = 1.25f;
    [SerializeField] private float pauseBetweenPhases = 1.1f;
    [SerializeField] private float riseHeightVariation = 1.25f;
    [SerializeField] private GameObject[] prefabs;

    // GETTERS
    public int ProjectileCount => projectileCount;
    public float SpawnRadius => spawnRadius;
    public float RiseHeight => riseHeight;
    public float RiseSpeed => riseSpeed;
    public float AttackSpeed => attackSpeed;
    public float DelayBetweenSpawns => delayBetweenSpawns;
    public float RiseMaxTime => riseMaxTime;
    public float PauseBetweenPhases => pauseBetweenPhases;
    public float RiseHeightVariation => riseHeightVariation;
    public GameObject[] Prefabs => prefabs;
}