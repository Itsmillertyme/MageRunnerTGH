using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Travel Speed")]

public class UpgradeTravelSpeed : SpellSkillUpgrade
{
    [Tooltip("Amount to be added to the base value of the move speed")]
    [SerializeField] private float movementIncrease;
    [Header("If Shatterstone spell")]
    [SerializeField] private float riseIncrease;

    public override void Apply(Spell spell)
    {
        switch (spell)
        {
            case ShatterstoneBarrage sb:
                sb.SetRiseSpeed(sb.RiseSpeed + riseIncrease);
                spell.SetMoveSpeed(spell.MoveSpeed + movementIncrease);
                break;
            default:
                spell.SetMoveSpeed(spell.MoveSpeed + movementIncrease);
                break;
        }
    }
}