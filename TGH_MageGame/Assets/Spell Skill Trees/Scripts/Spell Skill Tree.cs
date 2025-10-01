using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class SpellSkillTree : MonoBehaviour
{
    [SerializeField] private Spell spell;
    [SerializeField] private int skillPoints;

    private readonly HashSet<SpellSkillNode> ownedUpgrades = new();

    // CHECK IF UPGRADE IS ALREADY OWNED
    public bool UpgradeOwned(SpellSkillNode upgrade)
    {
        return ownedUpgrades.Contains(upgrade);
    }   

    // CHECK IF ELIGIBLE TO UPGRADE
    public bool CanUpgrade(SpellSkillNode upgrade)
    {
        return upgrade.CanUpgrade(ownedUpgrades, skillPoints);
    }

    // PERFORM THE UPGRADE
    public void ApplyUpgrade(SpellSkillNode upgrade)
    {
        // IF SHOULD NOT UPGRADE, RETURN
        if (UpgradeOwned(upgrade) || !CanUpgrade(upgrade))
        {
            return;
        }

        // UPGRADE
        ownedUpgrades.Add(upgrade);
        upgrade.ApplyUpgrade(spell);
        skillPoints = skillPoints - upgrade.UpgradeCost;

        // UPDATE BUTTON TEXT AND INTERACTABILITY
        foreach (var button in FindObjectsByType<SpellSkillUpgradeButton>(FindObjectsSortMode.None))
        {
            button.UpdateButtonState();
        }
    }

    public void SkillPointEarned()
    {
        skillPoints++;

        // tempPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP
        foreach (var button in FindObjectsByType<SpellSkillUpgradeButton>(FindObjectsSortMode.None))
        {
            button.UpdateButtonState();
        }
    }
}

public struct DoubleBool
{
    private bool first;
    private bool second;

    public bool First => first;
    public bool Second => second;

    public DoubleBool(bool newFirst, bool newSecond)
    {
        first = newFirst;
        second = newSecond;
    }
}