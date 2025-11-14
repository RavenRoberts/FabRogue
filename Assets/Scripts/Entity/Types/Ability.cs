using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    [SerializeField] protected int magnitude = 0;
    //[SerializeField] private string damageType = this might be a bit complex for now
    [SerializeField] protected int duration = 0;
    [SerializeField] protected int radius = 1;
    [SerializeField] protected int range = 1;
    [SerializeField] protected int cost = 0;
    [SerializeField] protected bool requiresTarget = true;
    [SerializeField] protected bool affectsSelf = false;

    public int Magnitude { get => magnitude; set => magnitude = value; }
    public int Duration { get => duration; set => duration = value; }
    public int Radius { get => radius; set => radius = value; }
    public int Range { get => range; set => range = value; }
    public int Cost { get => cost; set => cost = value; }
    public bool RequiresTarget { get => requiresTarget; set => requiresTarget = value; }
    public bool AffectsSelf { get => affectsSelf; set => affectsSelf = value; }


    public virtual bool Activate(Actor caster) // if the ability needs a target, open up target mode
    {
        //Actor caster = caster.GetComponent<Actor>();
        if (caster.Stamina < cost)
        {
            UIManager.instance.AddMessage($"{caster.name} doesn't have enough stamina!", "#808080");
            return false;
        }

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
        List<Entity> targets = new List<Entity>();
        if (caster.Stamina < cost)
        {
            UIManager.instance.AddMessage($"{caster.name} doesn't have enough stamina!", "#808080");
            return false;
        }

        caster.Stamina -= cost;

        if (radius <= 1) {
            Entity target = GameManager.instance.GetEntityAtLocation(targetPosition);
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
            foreach (Entity entity in GameManager.instance.Entities)
            {
                if (bounds.Contains(entity.transform.position) && (entity != caster || affectsSelf))
                {
                    targets.Add(entity);
                }
            }
        }

        if (targets.Count == 0)
        {
            UIManager.instance.AddMessage($"No valid target.", "#808080");
            return false;
        }

        foreach (Entity t in targets)
        {
            ApplyEffect(caster, t);
        }

        return true;
    }

    protected virtual void ApplyEffect(Actor caster, Entity target)
    {

    }
}
