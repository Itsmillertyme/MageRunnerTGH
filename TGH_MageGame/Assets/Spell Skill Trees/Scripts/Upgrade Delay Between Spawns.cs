using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Delay Between Spawns")]

public class UpgradeDelayBetweenSpawns : SpellSkillUpgrade
{
    [Tooltip("Amount to be reduced from the base value of the amount of time it takes to spawn projectiles")]
    [SerializeField] private float reduction;

    public override void Apply(Spell spell)
    {

        switch (spell)
        {
            case AbyssalFang af:
                break;
            case ShatterstoneBarrage sb:
                sb.SetDelayBetweenSpawns(sb.DelayBetweenSpawns - reduction);
                break;
            case ThunderlordsCascade tc:
                break;
        }
    }
}