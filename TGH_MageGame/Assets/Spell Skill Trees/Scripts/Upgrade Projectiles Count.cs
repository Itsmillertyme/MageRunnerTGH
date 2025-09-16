using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Projectiles Count")]

public class UpgradeProjectilesCount : SpellSkillUpgrade
{
    [Tooltip("Amount to be added to the base value of the amount of projectiles")]
    [SerializeField] private float boost;

    public override void Apply(Spell spell)
    {
        spell.SetMoveSpeed(spell.MoveSpeed + boost);
    }
}
