using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Projectile Size")]

public class UpgradeProjectileSize : SpellSkillUpgrade
{
    [Header("Only edit one of the two attributes")]
    [Tooltip("Amount to be added to the base value")]
    [SerializeField] private Vector3 sizeIncreaseVector3;
    [SerializeField] private float sizeIncreaseFloat;

    public override void Apply(Spell spell)
    {
        switch (spell)
        {
            case ShatterstoneBarrage sb:
                sb.SetProjectileSizeScalar(sb.ProjectileSizeScalar + sizeIncreaseFloat);
                break;
            default:
                spell.SetProjectileSize(spell.ProjectileSize + sizeIncreaseVector3);
                break;
        }
    }
}