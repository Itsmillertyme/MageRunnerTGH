using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spell Skill Tree/Upgrade Node")]

public class SpellSkillNode : ScriptableObject
{
    [Tooltip("This name will show on the button's text")]
    [SerializeField] private string upgradeName; // UPGRADE BUTTON TEXT NAME
    [SerializeField] private int upgradeCost; // AMOUNT OF SKILL POINTS REQUIRED TO UPGRADE
    [SerializeField] private SpellSkillNode[] prerequisiteUpgrades; // UPGRADES REQUIRED BEFORE THIS NODE
    [SerializeField] private SpellSkillUpgrade upgrade; // BOOST TO APPLY IN UPGRADE

    // GETTERS
    public string UpgradeName => upgradeName;
    public int UpgradeCost => upgradeCost;

    public DoubleBool CanUpgrade(HashSet<SpellSkillNode> ownedUpgrades, int availableSkillPoints)
    {
        bool hasPrereqs = true;
        bool hasSkillPoints = true;

        foreach (SpellSkillNode prerequisite in prerequisiteUpgrades)
        {
            if (!ownedUpgrades.Contains(prerequisite))
            {
                hasPrereqs = false;
            }
        }

        if (upgradeCost > availableSkillPoints)
        {
            hasSkillPoints = false;
        }

        return new DoubleBool(hasPrereqs, hasSkillPoints);
    }

    public void ApplyUpgrade(Spell spell)
    {
        upgrade.Apply(spell);
    }
}

public struct DoubleBool
{
    private bool hasPrereqs;
    private bool hasSkillPoints;

    public DoubleBool(bool hasPrereqs, bool hasSkillPoints)
    {
        this.hasPrereqs = hasPrereqs;
        this.hasSkillPoints = hasSkillPoints;
    }

    public bool HasPrereqsButNotSkillPoints() => hasPrereqs && !hasSkillPoints;
    public bool HasSkillPointsButNotPrereqs() => !hasPrereqs && hasSkillPoints;
    public bool MeetsAllRequirements() => hasPrereqs && hasSkillPoints;
}