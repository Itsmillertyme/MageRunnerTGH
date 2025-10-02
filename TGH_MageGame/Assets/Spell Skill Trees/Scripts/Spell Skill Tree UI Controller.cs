using TMPro;
using UnityEngine;

public class SpellSkillTreeUIController : MonoBehaviour
{
    [SerializeField] GameObject[] trees;
    [SerializeField] TextMeshProUGUI[] treesTexts;
    int activeTreeIndex = 0;
    bool uiEnabled = false;

    public void SetUIVisibility()
    {
        uiEnabled = !uiEnabled;
        trees[activeTreeIndex].SetActive(uiEnabled);
        treesTexts[activeTreeIndex].text = $"Skill Points: {trees[activeTreeIndex].GetComponent<SpellSkillTree>().SkillPoints}";
    }

    public void NextTree()
    {
        trees[activeTreeIndex].SetActive(false);

        if (activeTreeIndex < trees.Length - 1)
        {
            activeTreeIndex++;
        }
        else
        {
            activeTreeIndex = 0;
        }

        trees[activeTreeIndex].SetActive(true);
        treesTexts[activeTreeIndex].text = $"Skill Points: {trees[activeTreeIndex].GetComponent<SpellSkillTree>().SkillPoints}";
    }

    public void PreviousTree()
    {
        trees[activeTreeIndex].SetActive(false);

        if (activeTreeIndex > 0)
        {
            activeTreeIndex--;
        }
        else
        {
            activeTreeIndex = trees.Length - 1;
        }

        trees[activeTreeIndex].SetActive(true);
        treesTexts[activeTreeIndex].text = $"Skill Points: {trees[activeTreeIndex].GetComponent<SpellSkillTree>().SkillPoints}";
    }
}