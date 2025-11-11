using UnityEngine;

public class Drain : Ability
{
    private void Awake()
    {
        magnitude = 5;
        cost = 5;
    }

    protected override void ApplyEffect(Actor caster, Entity target)
    {
        Actor prey = target.GetComponent<Actor>();

        int drainAmount = Mathf.Min(magnitude, prey.Hp);

        Effects.DamageHealth(prey, drainAmount);
        Effects.RestoreHealth(caster, drainAmount);


        UIManager.instance.AddMessage($"{caster.name} drains {drainAmount} HP from {target.name}!", "#ff0000");
    }


}

