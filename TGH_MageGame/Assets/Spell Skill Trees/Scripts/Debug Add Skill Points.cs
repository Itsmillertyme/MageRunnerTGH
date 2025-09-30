using UnityEngine;

public class DebugAddSkillPoints : MonoBehaviour
{
    [SerializeField] private SpellSkillTree[] skillTrees;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            skillTrees[0].SkillPointEarned();
            Debug.Log(skillTrees[0].name + "Skill point earned");
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            skillTrees[1].SkillPointEarned();
            Debug.Log(skillTrees[1].name + "Skill point earned");
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            skillTrees[2].SkillPointEarned();
            Debug.Log(skillTrees[2].name + "Skill point earned");
        }
    }
}