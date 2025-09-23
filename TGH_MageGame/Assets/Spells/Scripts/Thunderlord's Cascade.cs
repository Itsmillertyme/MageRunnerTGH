using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Thunderlord's Cascade")]

public class ThunderlordsCascade : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private int defaultBoltCount = 3;
    [SerializeField] private int defaultVolleyCount = 1;
    [SerializeField] private float boltSpread;
    [SerializeField] private float volleyCooldown;
    [SerializeField] private float boltAngularSpread;
    [SerializeField] private float boltSpawnDelay;

    private int boltCount;
    private int volleyCount;

    // GETTERS
    public int BoltCount => boltCount;
    public int VolleyCount => volleyCount;
    public float BoltSpread => boltSpread;
    public float VolleyCooldown => volleyCooldown;
    public float BoltAngularSpread => boltAngularSpread;
    public float BoltSpawnDelay => boltSpawnDelay;

    private void OnEnable()
    {
        boltCount = defaultBoltCount;
        volleyCount = defaultVolleyCount;
    }

    // SETTERS
    public void SetProjectileCount(int newValue) => boltCount = newValue;
    public void SetVolleyCount(int value) => volleyCount = value;
}