using UnityEngine;

public class SpellSkillTreeUIController : MonoBehaviour
{
    [SerializeField] GameObject[] trees;
    int lastActiveTreeIndex = 0;
    bool uiEnabled = false;

    public void SetUIVisibility()
    {
        uiEnabled = !uiEnabled;
        trees[lastActiveTreeIndex].SetActive(uiEnabled);
    }

    public void NextTree()
    {
        trees[lastActiveTreeIndex].SetActive(false);

        if (lastActiveTreeIndex < trees.Length - 1)
        {
            lastActiveTreeIndex++;
        }
        else
        {
            lastActiveTreeIndex = 0;
        }

        trees[lastActiveTreeIndex].SetActive(true);
    }

    public void PreviousTree()
    {
        trees[lastActiveTreeIndex].SetActive(false);

        if (lastActiveTreeIndex > 0)
        {
            lastActiveTreeIndex--;
        }
        else
        {
            lastActiveTreeIndex = trees.Length - 1;
        }

        trees[lastActiveTreeIndex].SetActive(true);
    }
}