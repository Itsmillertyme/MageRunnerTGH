using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrade Node")]

public class SpellSkillNode : ScriptableObject
{
    [Tooltip("This name will show on the button's text")]
    [SerializeField] private string upgradeName; // UPGRADE BUTTON TEXT NAME
    [SerializeField] private SpellSkillNode[] prerequisiteUpgrades; // UPGRADES REQUIRED BEFORE THIS NODE
    [SerializeField] private SpellSkillUpgrade upgrade; // BOOST TO APPLY IN UPGRADE

    #region GETTERS
    public string UpgradeName => upgradeName;
    #endregion

    public bool CanUpgrade(HashSet<SpellSkillNode> ownedUpgrades)
    {
        foreach (SpellSkillNode prerequisite in prerequisiteUpgrades)
        {
            if (!ownedUpgrades.Contains(prerequisite))
            {
                return false;
            }
        }

        return true;
    }

    public void ApplyUpgrade(Spell spell)
    {
        upgrade.Apply(spell);
    }
}