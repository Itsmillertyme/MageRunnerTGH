using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrades/Projectiles Volley")]

public class UpgradeProjectileVolley : SpellSkillUpgrade
{
    [Tooltip("Amount to be added to the base value of the amount of volleys of projectiles")]
    [SerializeField] private int increase;

    public override void Apply(Spell spell)
    {
        switch (spell)
        {
            case ThunderlordsCascade tc:
                tc.SetVolleyCount(tc.VolleyCount + increase);
                break;
            default:
                break;
        }
    }
}