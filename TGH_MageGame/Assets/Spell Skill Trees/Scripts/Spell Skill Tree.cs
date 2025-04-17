using System.Collections.Generic;
using UnityEngine;

public class SpellSkillTree : MonoBehaviour
{
    [SerializeField] private Spell spell;

    private readonly HashSet<SpellSkillNode> ownedUpgrades = new();

    // CHECK IF UPGRADE IS ALREADY OWNED
    public bool UpgradeOwned(SpellSkillNode upgrade)
    {
        return ownedUpgrades.Contains(upgrade);
    }   

    // CHECK IF ELIGIBLE TO UPGRADE
    public bool CanUpgrade(SpellSkillNode upgrade)
    {
        return upgrade.CanUpgrade(ownedUpgrades);
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

        // UPDATE BUTTON TEXT AND INTERACTABILITY
        foreach (var button in FindObjectsByType<SpellSkillUpgradeButton>(FindObjectsSortMode.None))
        {
            button.UpdateButtonState();
        }
    }
}