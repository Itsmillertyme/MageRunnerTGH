using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Projectile Size")]

public class UpgradeProjectileSize : SpellSkillUpgrade
{
    [Tooltip("Amount to be added to the base value")]
    [SerializeField] private Vector3 sizeIncrease;

    public override void Apply(Spell spell)
    {
        spell.SetProjectileSize(spell.ProjectileSize + sizeIncrease);
        Debug.Log($"{spell.Name} projectile size increased {spell.ProjectileSize + sizeIncrease}");
    }
}