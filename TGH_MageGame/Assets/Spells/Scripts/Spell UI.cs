using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpellUI : MonoBehaviour
{
    [SerializeField] private TMP_Text spellBookUIText;
    [SerializeField] private Image activeSpellIcon;
    [SerializeField] private Image activeSpellReticle;
    [SerializeField] private Image xpBarImage;
    [SerializeField] private TextMeshProUGUI spellLevelText;

    public void SetSpellUIData(string newValue)
    {
        spellBookUIText.text = newValue;
    }

    public void SetActiveSpellIcon(Sprite newIcon)
    {
        activeSpellIcon.sprite = newIcon;
    }

    public void SetActiveReticle(Sprite newReticle)
    {
        activeSpellReticle.sprite = newReticle;
    }

    public void SetXPBarFill(float fill)
    {
        xpBarImage.fillAmount = fill;
    }

    public void SetSpellLevel(int value)
    {
        spellLevelText.text = value.ToString();
    }

    public void UpdateSpellUI(string newValue, Sprite newIcon, Sprite newReticle, float newXP, int newLevel)
    {
        SetSpellUIData(newValue);
        SetActiveSpellIcon(newIcon);
        SetActiveReticle(newReticle);
        SetXPBarFill(newXP);
        SetSpellLevel(newLevel);
    }
}