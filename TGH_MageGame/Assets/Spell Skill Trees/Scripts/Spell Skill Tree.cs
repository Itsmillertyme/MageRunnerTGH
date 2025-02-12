using UnityEngine;
using UnityEngine.UI;

public class SpellSkillTree : MonoBehaviour
{
    [SerializeField] private SpellSkill[] skillTree;
    [SerializeField] private Button[] buttons;
    [SerializeField] private int currentTier = 0;

    private void Start()
    {
        ResetSkillTrees(); // TEMP UNTIL WE HAVE A SAVE SYSTEM IN PLACE
    }

    public void Purchase(int index)
    {
        skillTree[index].Purchase();
        SetCurrentTier();
    }

    public void SetCurrentTier()
    {
        bool areAllOwned = true;
        int highestTier = 0;

        // CHECK CURRENT TIER TO SEE IF ALL SKILLS ARE OWNED
        foreach (SpellSkill skill in skillTree)
        {
            if (skill.SkillTier == currentTier && !skill.IsOwned)
            {
                areAllOwned = false;
                break;
            }

            highestTier = skill.SkillTier;
        }

        // INCREMENT THE TIER IF ALL ARE OWNED
        if (areAllOwned)
        {
            if (currentTier != highestTier)
            {
                currentTier++;
            }       
        }

        // MAKE NEW TIER PURCHASABLE
        for (int i = 0; i < skillTree.Length; i++)
        {
            if (skillTree[i].SkillTier == currentTier)
            {
                skillTree[i].SetCanPurchase();

                if (!skillTree[i].IsOwned)
                {
                    buttons[i].interactable = true;
                }
            }
        }

    }

    public void ResetSkillTrees()
    {
        currentTier = 0;

        foreach (SpellSkill skill in skillTree)
        {
            if (skill.SkillTier == currentTier)
            {
                skill.SetCanPurchase();
                
            }
            else
            {
                skill.SetCannotPurchase();
            }

            skill.SetNotOwned();
        }
    }
} 