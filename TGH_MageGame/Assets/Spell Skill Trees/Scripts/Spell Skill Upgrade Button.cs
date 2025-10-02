using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SpellSkillUpgradeButton : MonoBehaviour
{
    [SerializeField] private SpellSkillNode upgradeNode;

    private SpellSkillTree spellTree;
    private Button button;
    private TextMeshProUGUI buttonText;

    private void Awake()
    {
        spellTree = GetComponentInParent<SpellSkillTree>();
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        button.onClick.AddListener(ApplyUpgrade);
        UpdateButtonState();
    }

    private void ApplyUpgrade()
    {
        spellTree.ApplyUpgrade(upgradeNode);
        UpdateButtonState();
    }

    public void UpdateButtonState()
    {
        if (spellTree.UpgradeOwned(upgradeNode)) // IF OWNED
        {
            button.interactable = false;
            buttonText.text = "Owned";
        }
        else if (spellTree.CanUpgrade(upgradeNode).HasPrereqsButNotSkillPoints()) // IF CANNOT UPGRADE BUT HAS MET PREREQS
        {
            button.interactable = false;
            buttonText.text = $"{upgradeNode.UpgradeName}: {upgradeNode.UpgradeCost}";
        }
        else if (spellTree.CanUpgrade(upgradeNode).HasSkillPointsButNotPrereqs()) // IF CANNOT UPGRADE BUT HAS MET SKILLPOINTS
        {
            button.interactable = false;
            buttonText.text = "Locked";
        }
        else if (spellTree.CanUpgrade(upgradeNode).MeetsAllRequirements()) // IF MEETS ALL REQUIREMENTS AND CAN BUY
        {
            button.interactable = true;
            buttonText.text = $"{upgradeNode.UpgradeName}: {upgradeNode.UpgradeCost}";
        }
    }
}