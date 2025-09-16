using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Automatic Fire")]

public class UpgradeAutomaticFire : SpellSkillUpgrade
{
    [Tooltip("True means can fire full auto")]
    [SerializeField] private bool autoFire;

    public override void Apply(Spell spell)
    {
        spell.SetAutomaticFireRate(autoFire);
        Debug.Log("Auotmatic fire unlocked");
    }
}