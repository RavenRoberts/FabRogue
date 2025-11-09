using UnityEngine;

public class Swallow : Ability
{
    protected override void ApplyEffect(Actor caster, Entity target)
    {
        DigestiveTract gut = caster.GetComponent<DigestiveTract>();

        GameManager.instance.RemoveEntity(target);
        gut.AddToStomach(target);
        UIManager.instance.AddMessage($"{caster.name} swallows {target.name} whole!", "#63ff63");

    }

}
