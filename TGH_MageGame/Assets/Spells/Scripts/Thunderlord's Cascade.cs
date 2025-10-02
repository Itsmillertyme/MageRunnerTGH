using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Thunderlord's Cascade")]

public class ThunderlordsCascade : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private int defaultBoltCount = 3;
    [SerializeField] private int defaultVolleyCount = 1;
    [SerializeField] private float defaultBoltSpread;
    [SerializeField] private float volleyCooldown;
    [SerializeField] private float boltAngularSpread;
    [SerializeField] private float boltSpawnDelay;
    [SerializeField] private float dimTimeFrame;
    [Tooltip("Expressed as a decimal. 1 = 100%")]
    [SerializeField] private float dimIntensityScale;

    // TEMP UNTIL WE HAVE A SAVE SYSTEM
    private int boltCount;
    private int volleyCount;
    private float boltSpread;

    // GETTERS
    public int BoltCount => boltCount;
    public int VolleyCount => volleyCount;
    public float BoltSpread => boltSpread;
    public float VolleyCooldown => volleyCooldown;
    public float BoltAngularSpread => boltAngularSpread;
    public float BoltSpawnDelay => boltSpawnDelay;
    public float DimTimeFrame => dimTimeFrame;
    public float DimIntensityScale => dimIntensityScale;

    private void OnEnable()
    {
        boltCount = defaultBoltCount;
        volleyCount = defaultVolleyCount;
        boltSpread = defaultBoltSpread;
    }

    // SETTERS
    public void SetProjectileCount(int newValue)
    {
        boltCount = newValue;
        boltSpread += 3;
    }
    public void SetVolleyCount(int value) => volleyCount = value;
}