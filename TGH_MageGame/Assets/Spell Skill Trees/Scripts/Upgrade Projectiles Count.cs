using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Projectiles Count")]

public class UpgradeProjectilesCount : SpellSkillUpgrade
{
    [Tooltip("Amount to be added to the base value of the amount of projectiles")]
    [SerializeField] private int increase;

    public override void Apply(Spell spell)
    {
        switch (spell)
        {
            case ShatterstoneBarrage sb:
                sb.SetProjectileCount(sb.ProjectileCount + increase);
                break;
            case ThunderlordsCascade tc:
                tc.SetProjectileCount(tc.BoltCount + increase);
                break;
            default:
                break;
        }
    }
}