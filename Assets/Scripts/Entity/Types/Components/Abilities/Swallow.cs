using UnityEngine;

public class Swallow : Ability
{
    protected override void ApplyEffect(Actor caster, Entity target)
    {
        Actor prey = target.GetComponent<Actor>();

        if (target != target.GetComponent<Actor>())// there is probably a cleaner way of doing this
        {
            Effects.Swallow(caster, target);
            UIManager.instance.AddMessage($"{caster.name} swallows the {target.name}!", "#63ff63");
        }
        else
        {
            int staminaCost = prey.Hp;

            if (caster.Stamina < staminaCost)
            {
                UIManager.instance.AddMessage($"{caster.name} is too exhasted to swallow {target.name}...", "#808080");
                return;
            }

            caster.Stamina -= staminaCost;
        
            Effects.Swallow(caster, target);
            UIManager.instance.AddMessage($"{caster.name} swallows the {target.name} whole!", "#63ff63");
        }
    }
}
