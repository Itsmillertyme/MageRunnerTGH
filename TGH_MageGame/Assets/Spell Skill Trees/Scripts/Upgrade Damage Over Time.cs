using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Damage Over Time")]

public class UpgradeDamageOverTime : SpellSkillUpgrade
{
    public override void Apply(Spell spell)
    {
        switch (spell)
        {
            case ThunderlordsCascade tc:
                tc.SetDamageOverTime(true);
                break;
            default:
                break;
        }
    }
}