using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill/Abyssal Fang Skill")]

public class AbyssalFangSkill : SpellSkillOld
{
    [Header("Upgrade Attributes")]
    [SerializeField] private float projectileSizeUpgrade;
    [SerializeField] private float projectileTravelSpeedUpgrade;
    [SerializeField] private float fireRateUpgrade1;
    [SerializeField] private float fireRateUpgrade2;
    [SerializeField] private bool automaticFireUpgrade = true;
    [SerializeField] private bool obstaclePiercingUpgrade = true;

    // PROJECTILE SIZE
    // TRAVEL SPEED
    // FIRE RATE
    // FIRE RATE
    // AUTO FIRE
    // OBSTACLE PIERCING

    public override void ApplyUpgrade(int index)
    {
        Debug.Log($"Upgraded {name} from index {index}");
    }
}
//STARTED 145 PM