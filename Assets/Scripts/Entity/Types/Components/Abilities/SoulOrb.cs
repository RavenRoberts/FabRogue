using UnityEditor;
using UnityEngine;

public class SoulOrb : Ability
{
    private void Awake()
    {
        magnitude = 7;
        radius = 2;
        range = 5;
        cost = 10;
        affectsSelf = true;
    }

    protected override void ApplyEffect(Actor caster, Entity target)
    {
        Actor area = target.GetComponent<Actor>();
        Effects.ApplyDamage(area, magnitude, StatType.Health, DamageType.Astral);

        UIManager.instance.AddMessage($"{caster.name} scorches the ground with soul energy!", "#ff0000");
    }

}
