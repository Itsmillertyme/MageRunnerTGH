using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpellUI : MonoBehaviour
{
    [SerializeField] private TMP_Text spellBookUIText;
    [SerializeField] private SpellBook SpellBook;
    [SerializeField] private Image activeSpellIcon;

    private void Start()
    {
        UpdateSpellUI();
    }
    public void GetSpellUIData()
    {
        int spellIndex = SpellBook.ActiveSpell;
        spellBookUIText.text = $"{SpellBook.GetSpellUIData()}";
    }

    public void GetActiveSpellIcon()
    {
        activeSpellIcon.sprite = SpellBook.GetSpellIconData();
    }

    public void UpdateSpellUI()
    {
        GetSpellUIData();
        GetActiveSpellIcon();
    }
}