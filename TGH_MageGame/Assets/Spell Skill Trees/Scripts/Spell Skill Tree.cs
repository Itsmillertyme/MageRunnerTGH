using System.Collections.Generic;
using UnityEngine;

public class SpellSkillTree : MonoBehaviour
{
    [SerializeField] private Spell spell;
    [SerializeField] private int skillPoints;
    private SpellSkillTreeUIController uiController;
    private readonly HashSet<SpellSkillNode> ownedUpgrades = new();
    public int SkillPoints => skillPoints;
    public Spell SelectedSpell => spell;

    private void Awake()
    {
        uiController = GetComponentInParent<SpellSkillTreeUIController>();
    }

    // CHECK IF UPGRADE IS ALREADY OWNED
    public bool UpgradeOwned(SpellSkillNode upgrade)
    {
        return ownedUpgrades.Contains(upgrade);
    }   

    // CHECK IF ELIGIBLE TO UPGRADE
    public DoubleBool CanUpgrade(SpellSkillNode upgrade)
    {
        return upgrade.CanUpgrade(ownedUpgrades, skillPoints);
    }

    // PERFORM THE UPGRADE
    public void ApplyUpgrade(SpellSkillNode upgrade)
    {
        // IF SHOULD NOT UPGRADE, RETURN
        if (UpgradeOwned(upgrade) || !CanUpgrade(upgrade).MeetsAllRequirements())
        {
            return;
        }

        // UPGRADE
        ownedUpgrades.Add(upgrade);
        upgrade.ApplyUpgrade(spell);
        skillPoints -= upgrade.UpgradeCost;

        // UPDATE BUTTON TEXT AND INTERACTABILITY
        foreach (var button in FindObjectsByType<SpellSkillUpgradeButton>(FindObjectsSortMode.None))
        {
            button.UpdateButtonState();
        }

        uiController.UpdateSkillPoints();
    }

    public void SkillPointEarned()
    {
        skillPoints++;

        // UPDATE BUTTON TEXT AND INTERACTABILITY
        foreach (var button in FindObjectsByType<SpellSkillUpgradeButton>(FindObjectsSortMode.None))
        {
            button.UpdateButtonState();
        }
    }
}