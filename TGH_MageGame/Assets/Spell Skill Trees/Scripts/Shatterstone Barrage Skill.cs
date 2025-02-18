using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill/Shatterstone Barrage Skill")]

public class ShatterstoneBarrageSkill : SpellSkillOld
{
    // ADDITIONAL PROJECTILES
    // PROJECTILE SIZE
    // SPELL DURATION
    // ADDITIONAL WAVE OF PROJECTILES
    // EXPLOSIVE IMPACTS

    public override void ApplyUpgrade(int index)
    {
        Debug.Log($"Upgraded {name} from index {index}");
    }
}
