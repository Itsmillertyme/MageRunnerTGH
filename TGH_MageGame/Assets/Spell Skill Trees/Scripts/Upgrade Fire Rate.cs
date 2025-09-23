using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Fire Rate")]

public class UpgradeFireRate : SpellSkillUpgrade
{
    [Tooltip("Amount to be subtracted from the base value of cast cooldown time")]
    [SerializeField] private float reduction;

    public override void Apply(Spell spell)
    {
        spell.SetCastCooldownTime(spell.CastCooldownTime - reduction);
    }
}