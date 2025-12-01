using UnityEngine;

public class Drain : Ability
{
    private void Awake()
    {
        magnitude = 10;
        cost = 10;
    }

    protected override void ApplyEffect(Actor caster, Entity target)
    {
        Actor prey = target.GetComponent<Actor>();

        int actualDamage = Effects.ApplyDamage(prey, magnitude, StatType.Health, DamageType.Corpus);
        if (actualDamage > 0)
        {
            Effects.RestoreHealth(caster, actualDamage);
            UIManager.instance.AddMessage($"{caster.name} drains {actualDamage} HP from {target.name}!", "#ff0000");
        }
        else
        {
            UIManager.instance.AddMessage($"{caster.name} attempts to drain {target.name}, but it has no effect!", "#808080");
        }
    }
}

