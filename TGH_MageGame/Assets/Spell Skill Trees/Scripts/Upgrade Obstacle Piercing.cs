using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Obstacle Piercing")]

public class UpgradeObstaclePiercing : SpellSkillUpgrade
{
    public override void Apply(Spell spell)
    {
        spell.SetDestroyOnEnemyImpact(false);
        spell.SetDestroyOnEnvironmentalImpact(false);
    }
}