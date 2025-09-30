using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        if (spellTree.UpgradeOwned(upgradeNode))
        {
            button.interactable = false;
            buttonText.text = "Owned";
        }
        else if (!spellTree.CanUpgrade(upgradeNode))
        {
            button.interactable = false;
            buttonText.text = "Locked";
        }
        else
        {
            button.interactable = true;
            buttonText.text = $"{upgradeNode.UpgradeName}: {upgradeNode.UpgradeCost}";
        }
    }
}