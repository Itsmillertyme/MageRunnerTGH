using UnityEngine;

public abstract class SpellSkillOld : ScriptableObject
{
    [SerializeField] private bool isOwned = false;
    [SerializeField] private bool canPurchase = false;
    [SerializeField] private int skillTier;

    public bool IsOwned => isOwned;
    public bool CanPurchase => canPurchase;
    public int SkillTier => skillTier;

    public void Purchase(int index)
    {
        if (canPurchase)
        {
            isOwned = true;
            ApplyUpgrade(index);
        }
    }

    public abstract void ApplyUpgrade(int index);

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