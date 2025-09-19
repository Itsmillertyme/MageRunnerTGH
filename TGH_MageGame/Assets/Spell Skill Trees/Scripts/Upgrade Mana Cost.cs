using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Mana Cost")]

public class UpgradeManaCost : SpellSkillUpgrade
{
    [Tooltip("Amount to be reduced from the base value of mana cost")]
    [SerializeField] private int decrease;

    public override void Apply(Spell spell)
    {
        spell.SetManaCost(spell.ManaCost - decrease);
    }
}