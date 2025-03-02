using UnityEngine;

[CreateAssetMenu(menuName = "Spell Skill/Infernal Embrace Skill")]

public class InfernalEmbraceSkill : SpellSkillOld
{
    // RANGE
    // REDUCED MANA COST
    // WIDER SPREAD
    // DAMAGE
    // DAMAGE OVER TIME
    // INCREASE DAMAGE OVER TIME

    public override void ApplyUpgrade(int index)
    {
        Debug.Log($"Upgraded {name} from index {index}");
    }
}
