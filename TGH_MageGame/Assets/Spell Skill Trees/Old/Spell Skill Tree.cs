//using System;
//using UnityEngine;
//using UnityEngine.UI;

//public class SpellSkillTree : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private Spell spell;
//    [SerializeField] private SpellSkill[] skills;
//    [SerializeField] private Button[] buttons;
//    [SerializeField] private int currentSkillTier = 0; // SERIALIZED FOR DEBUGGING

//    private void Start()
//    {
//        ResetSkillTrees(); // TEMP UNTIL WE HAVE A SAVE SYSTEM IN PLACE
//    }

//    public void Purchase(int index)
//    {
//        skills[index].Purchase(index);
//        SetCurrentTier();
//    }

//    //public void Purchase()
//    //{
//    //    foreach (Button button in buttons)
//    //    {
            
//    //    }
//    //}

//    public void SetCurrentTier()
//    {
//        bool areAllOwned = true;
//        int highestTier = 0;

//        // CHECK CURRENT TIER TO SEE IF ALL SKILLS ARE OWNED
//        foreach (SpellSkill skill in skills)
//        {
//            if (skill.SkillTier == currentSkillTier && !skill.IsOwned)
//            {
//                areAllOwned = false;
//                break;
//            }

//            highestTier = skill.SkillTier;
//        }

//        // INCREMENT THE TIER IF ALL ARE OWNED
//        if (areAllOwned)
//        {
//            if (currentSkillTier != highestTier)
//            {
//                currentSkillTier++;
//            }       
//        }

//        // MAKE NEW TIER PURCHASABLE
//        for (int i = 0; i < skills.Length; i++)
//        {
//            if (skills[i].SkillTier == currentSkillTier)
//            {
//                skills[i].SetCanPurchase();

//                if (!skills[i].IsOwned)
//                {
//                    buttons[i].interactable = true;
//                }
//            }
//        }

//    }

//    public void ResetSkillTrees()
//    {
//        currentSkillTier = 0;

//        foreach (SpellSkill skill in skills)
//        {
//            if (skill.SkillTier == currentSkillTier)
//            {
//                skill.SetCanPurchase();
                
//            }
//            else
//            {
//                skill.SetCannotPurchase();
//            }

//            skill.SetNotOwned();
//        }
//    }
//} 