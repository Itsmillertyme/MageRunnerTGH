using UnityEngine;

[CreateAssetMenu] // REMOVE AFTER REPLACE OF ASSETS IN INSPECTOR

public class SpellSkill : ScriptableObject
{
    [SerializeField] private bool isOwned = false;
    [SerializeField] private bool canPurchase = false;
    [SerializeField] private int skillTier;

    public bool IsOwned => isOwned;
    public bool CanPurchase => canPurchase;
    public int SkillTier => skillTier;

    public void Purchase()
    {
        if (canPurchase)
        {
            isOwned = true;
        }
    }

    public void SetCanPurchase()
    {
        canPurchase = true;
    }

    public void SetCannotPurchase()
    {
        canPurchase = false;
    }

    public void SetNotOwned()
    {
        isOwned = false;
    }
}