using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill/Heaven's Lament Skill")]

public class HeavensLamentSkill : SpellSkillOld
{
    // PROJECTILE SIZE
    // ENEMY PENETRATION
    // PROJECTILE SIZE
    // CONTINUOUS FIRE
    // DIRECTABLE DAMAGE/BEAM
    // BONUS DAMAGE ON ENEMY PENETRATION

    public override void ApplyUpgrade(int index)
    {
        Debug.Log($"Upgraded {name} from index {index}");
    }
}
