using System.Collections.Generic;
using UnityEngine;

public class SpellSkillTree : MonoBehaviour
{
    [SerializeField] private List<SpellSkill> skillTree;
    [SerializeField] private int currentTier = 0;

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

        // MAKE NEW TIER PURCHASEABLE
        foreach (SpellSkill skill in skillTree)
        {
            if (skill.SkillTier == currentTier)
            {
                skill.SetCanPurchase();
            }
        }
    }
} 