using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpellUI : MonoBehaviour
{
    [SerializeField] private TMP_Text spellBookUIText;
    [SerializeField] private Image activeSpellIcon;
    [SerializeField] private Image activeSpellReticle;

    public void GetSpellUIData(string newValue)
    {
        spellBookUIText.text = newValue;
    }

    public void GetActiveSpellIcon(Sprite newIcon)
    {
        activeSpellIcon.sprite = newIcon;
    }

    public void GetActiveReticle(Sprite newReticle)
    {
        activeSpellReticle.sprite = newReticle;
    }

    public void UpdateSpellUI(string newValue, Sprite newIcon, Sprite newReticle)
    {
        GetSpellUIData(newValue);
        GetActiveSpellIcon(newIcon);
        GetActiveReticle(newReticle);
    }
}