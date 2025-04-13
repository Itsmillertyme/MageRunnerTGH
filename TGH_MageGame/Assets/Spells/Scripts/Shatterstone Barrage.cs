using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Shatterstone Barrage")]

public class ShatterstoneBarrage : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private int projectileCount;
    [SerializeField] private float spawnRadius;
    [SerializeField] private float riseHeight;
    [SerializeField] private float riseSpeed;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float delayBetweenSpawns;
    [SerializeField] private float riseMaxTime;
    [SerializeField] private float pauseBetweenPhases;
    [SerializeField] private float riseHeightVariation;
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