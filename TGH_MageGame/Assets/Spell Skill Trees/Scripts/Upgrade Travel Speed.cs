using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Travel Speed")]

public class UpgradeTravelSpeed : SpellSkillUpgrade
{
    [Tooltip("Amount to be added to the base value of the move speed")]
    [SerializeField] private float boost;

    public override void Apply(Spell spell)
    {
        spell.SetMoveSpeed(spell.MoveSpeed + boost);
    }
}