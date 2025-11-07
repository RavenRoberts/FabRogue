using UnityEngine;

public class Drain : Ability
{
    private void Awake()
    {
        magnitude = 5;


    }

    protected override void ApplyEffect(Actor caster, Actor target)
    {
        Fighter targetFighter = target.GetComponent<Fighter>();
        Fighter casterFighter = caster.GetComponent<Fighter>();

        int drainAmount = Mathf.Min(magnitude, targetFighter.Hp);

        targetFighter.Hp -= drainAmount;
        casterFighter.Hp += drainAmount;

        UIManager.instance.AddMessage($"{caster.name} drains {drainAmount} HP from {target.name}!", "#ff0000");
    }


}

