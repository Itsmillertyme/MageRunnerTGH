using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]

public class SpellSkill : ScriptableObject
{
    [SerializeField] private bool isOwned = false;
    [SerializeField] private bool canPurchase = false;
    [SerializeField] private int skillTier;

    public bool IsOwned => isOwned;
    public bool CanPurchase => canPurchase;
    public int SkillTier => skillTier;

    private void Awake()
    {
        if (skillTier == 0)
        {
            canPurchase = true;
        }
    }

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

    public void SetCantPurchase()
    {
        canPurchase = false;
    }

    public void SetNotOwned()
    {
        isOwned = false;
    }
}