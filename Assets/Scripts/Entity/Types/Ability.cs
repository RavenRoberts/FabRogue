using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    [SerializeField] protected int magnitude = 0;
    //[SerializeField] private string damageType = this might be a bit complex for now
    [SerializeField] protected int duration = 0;
    [SerializeField] protected int radius = 1;
    [SerializeField] protected int range = 1;
    [SerializeField] protected bool requiresTarget = true;
    [SerializeField] protected bool affectsSelf = false;

    public int Magnitude { get => magnitude; set => magnitude = value; }
    public int Duration { get => duration; set => duration = value; }
    public int Radius { get => radius; set => radius = value; }
    public int Range { get => range; set => range = value; }
    public bool RequiresTarget { get => requiresTarget; set => requiresTarget = value; }
    public bool AffectsSelf { get => affectsSelf; set => affectsSelf = value; }


    public virtual bool Activate(Actor caster)
    {
        if (requiresTarget)
        {
            caster.GetComponent<Player>().ToggleTargetMode(this);
            UIManager.instance.AddMessage($"Select target.", "#63ffff");
            return false;
        }
        return Cast(caster, caster.transform.position);
    }
    public virtual bool Cast(Actor caster, Vector3 targetPosition)
    {
        List<Actor> targets = new List<Actor>();

        if (radius <= 1) {
            Actor target = GameManager.instance.GetActorAtLocation(targetPosition);
            if (target != null && target != caster)
            {
                targets.Add(target);
            }
            else if (affectsSelf)
            {
                targets.Add(caster);
            }
        }
        else
        {
            Bounds bounds = new Bounds(targetPosition, Vector3.one * radius * 2);
            foreach (Actor a in GameManager.instance.Actors)
            {
                if (bounds.Contains(a.transform.position) && (a != caster || affectsSelf))
                {
                    targets.Add(a);
                }
            }
        }

        if (targets.Count == 0)
        {
            UIManager.instance.AddMessage($"No valid target.", "#808080");
            return false;
        }

        foreach (Actor t in targets)
        {
            ApplyEffect(caster, t);
        }

        return true;
    }

    protected virtual void ApplyEffect(Actor caster, Actor target)
    {

    }
}
