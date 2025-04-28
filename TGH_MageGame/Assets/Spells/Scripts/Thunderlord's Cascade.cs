using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Thunderlord's Cascade")]

public class ThunderlordsCascade : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private int boltCount;
    [SerializeField] private int volleyCount;
    [SerializeField] private float boltSpread;
    [SerializeField] private float volleyCooldown;
    [SerializeField] private float boltAngularSpread;
    [SerializeField] private float boltSpawnDelay;

    // GETTERS
    public int BoltCount => boltCount;
    public int VolleyCount => volleyCount;
    public float BoltSpread => boltSpread;
    public float VolleyCooldown => volleyCooldown;
    public float BoltAngularSpread => boltAngularSpread;
    public float BoltSpawnDelay => boltSpawnDelay;

    // SETTERS
    public void SetBoltCont(int value) => boltCount = value;
    public void SetVolleyCont(int value) => volleyCount = value;
}