using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Damage")]

public class UpgradeDamage : SpellSkillUpgrade
{
    [Tooltip("Amount to be added to the base value of damage")]
    [SerializeField] private int increase;

    public override void Apply(Spell spell)
    {
        spell.SetDamage(spell.Damage + increase);
    }
}