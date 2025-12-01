using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class AbilitySlots : MonoBehaviour
{
    [SerializeField] private List<Ability> equippedAbilities = new List<Ability>(5);

    public List<Ability> EquippedAbilities { get => equippedAbilities; }

    private void Awake()
    {
        while (EquippedAbilities.Count < 5)
            equippedAbilities.Add(null);
    }

    private void Start()//i think this should autoequip?
    {
        GetComponent<AbilitySlots>().EquipAbility(1, GetComponent<Drain>());
        GetComponent<AbilitySlots>().EquipAbility(2, GetComponent<Swallow>());
        GetComponent<AbilitySlots>().EquipAbility(3, GetComponent<SoulOrb>());

    }

    public Ability GetAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedAbilities.Count)
            return null;
        return equippedAbilities[slotIndex];
    }

    public void EquipAbility(int slotIndex, Ability ability)
    {
        if (slotIndex < 0 || slotIndex >= equippedAbilities.Count)
        {
            return;
        }

        equippedAbilities[slotIndex] = ability;
        UIManager.instance.AddMessage($"{ability.name} equipped to slot {slotIndex + 1}.", "#63ffff");
    }

    public void UnequipAbility(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= EquippedAbilities.Count)
            return;

        var ability = equippedAbilities[slotIndex];
        if (ability != null)
        {
            UIManager.instance.AddMessage($"{ability.name} unequipped from slot {slotIndex + 1}.", "#808080");
            equippedAbilities[slotIndex] = null;
        }
    }

    public void UseAbility(int slotIndex, Actor caster)
    {
        if (slotIndex < 0 || slotIndex >= equippedAbilities.Count)
            return;

        Ability ability = equippedAbilities[slotIndex];
        if (ability == null)
        {
            UIManager.instance.AddMessage($"No ability in slot {slotIndex + 1}.", "#808080");
            return;
        }

        ability.Activate(caster);
    }
}
