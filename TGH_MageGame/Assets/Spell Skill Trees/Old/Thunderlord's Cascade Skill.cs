using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill/Thunderlord's Cascade Skill")]

public class ThunderlordsCascadeSkill : SpellSkillOld
{
    // ADDITIONAL BOLT
    // REDUCED MANA COST
    // ADDITIONAL BOLT
    // ADDITIONAL WAVE OF BOLTS
    // DAMAGE OVER TIME
    // CHAIN DAMAGE

    public override void ApplyUpgrade(int index)
    {
        Debug.Log($"Upgraded {name} from index {index}");
    }
}
