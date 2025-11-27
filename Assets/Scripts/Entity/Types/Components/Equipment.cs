using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class Equipment : MonoBehaviour
{
    private Dictionary<EquipmentSlot, Equippable> equippedItems = new();

    public Equippable GetEquippedItem(EquipmentSlot slot) => 
        equippedItems.TryGetValue(slot, out var item) ? item : null;

    public void EquipItem(Equippable item)
    {
        if (item == null) return;

        if (equippedItems.TryGetValue(item.EquipmentSlot, out var current))
            UnequipItem(current);

        equippedItems[item.EquipmentSlot] = item;
        UIManager.instance.AddMessage($"You equip the {item.name}.", "#da8ee");
        item.name = $"{item.name} (E)";
    }

    public void UnequipItem(Equippable item)
    {
        if (item == null || !equippedItems.ContainsKey(item.EquipmentSlot)) return;

        equippedItems.Remove(item.EquipmentSlot);
        UIManager.instance.AddMessage($"You remove the {item.name}.", "#da8ee");
        item.name = item.name.Replace(" (E)", "");
    }

    public void ToggleEquip(Equippable item)
    {
        if (item == null) return;

        if (equippedItems.TryGetValue(item.EquipmentSlot, out var current) && current == item)
            UnequipItem(item);
        else
            EquipItem(item);
    }

    public int GetFlatDefense(DamageType type)
    {
        int total = 0;
        foreach (var item in equippedItems.Values)
            if (item != null) total += item.GetFlatDefense(type);
        return total;
    }

    public float GetPercentDefense(DamageType type)
    {
        float total = 0f;
        foreach (var item in equippedItems.Values)
            if (item != null) total += item.GetPercentDefense(type);
        return total;
    }

    public int GetDamageBonus(DamageType type)
    {
        int total = 0;
        foreach (var item in equippedItems.Values)
            if (item != null) total += item.GetDamageBonus(type);
        return total;
    }
}
