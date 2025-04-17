using UnityEngine;

[CreateAssetMenu(menuName = "Spells/Heaven's Lament")]

public class HeavensLament : Spell
{
    [Header("Unique Spell Attributes")]
    [SerializeField] private GameObject vfxPrefab;
}