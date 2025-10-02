using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Mana Increase")]

public class UpgradeMaxMana : SpellSkillUpgrade
{
    [Tooltip("Amount to be added to the base value of mana capacity")]
    [SerializeField] private int increase;

    public override void Apply(Spell spell)
    {
        spell.SetMaxMana(spell.CurrentMana + increase);
    }
}