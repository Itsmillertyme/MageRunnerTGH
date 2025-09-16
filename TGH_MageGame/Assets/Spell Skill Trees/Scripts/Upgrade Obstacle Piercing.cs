using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Obstacle Piercing")]

public class UpgradeObstaclePiercing : SpellSkillUpgrade
{
    [Tooltip("True means projectiles pass through environmental/enemy objects")]
    [SerializeField] private bool pierceObjects;

    public override void Apply(Spell spell)
    {
        spell.SetDestroyOnEnemyImpact(pierceObjects);
        spell.SetDestroyOnEnvironmentalImpact(pierceObjects);
        Debug.Log("Obstacle piercing unlocked");
    }
}