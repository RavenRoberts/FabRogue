using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Item))]
public class Equippable : MonoBehaviour
{
    [SerializeField] private EquipmentSlot equipmentSlot;

    [Header("Damage")]
    [SerializeField] private List<DamageBonusEntry> damageBonuses = new();

    [System.Serializable]
    public struct DamageBonusEntry
    {
        public DamageType type;
        public int value;
    }

    [Header("Flat Defense")]
    [SerializeField] private List<FlatDefenseEntry> flatDefense = new();

    [System.Serializable]
    public struct FlatDefenseEntry
    {
        public DamageType type;
        public int value;
    }

    [Header("Percent Defense")]
    [SerializeField] private List<PercentDefenseEntry> percentDefense = new();

    [System.Serializable]
    public struct PercentDefenseEntry
    {
        public DamageType type;
        public float value; // stored as percentage 0-100
    }

    public EquipmentSlot EquipmentSlot { get => equipmentSlot; }

    public int GetFlatDefense(DamageType type)
    {
        foreach (var entry in flatDefense)
        {
            if (entry.type == type)
            {
                return entry.value;
            }
        }
        return 0;
    }

    public float GetPercentDefense(DamageType type)
    {
        foreach (var entry in percentDefense)
        {
            if (entry.type == type)
            {
                return entry.value;
            }
        }
        return 0f;
    }

    public int GetDamageBonus(DamageType type)
    {
        foreach (var entry in damageBonuses)
        {
            if (entry.type == type)
            {
                return entry.value;
            }
        }
        return 0;
    }
}

public enum EquipmentSlot
{
    Weapon,
    Head,
    Chest,
    Hands,
    Feet
}

