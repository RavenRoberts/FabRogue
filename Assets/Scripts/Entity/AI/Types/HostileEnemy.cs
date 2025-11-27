using System;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class HostileEnemy : AI
{
    [SerializeField] private Actor actor;
    [SerializeField] private bool isFighting;

    private void OnValidate()
    {
        actor = GetComponent<Actor>();
        AStar = GetComponent<AStar>();
    }

    public override void RunAI()
    {
        if (!actor.Target)
        {
            actor.Target = GameManager.instance.Actors[0];
        }
        else if (actor.Target && !actor.Target.IsAlive)
        {
            actor.Target = null;
        }

        if (actor.Target)
        {
            Vector3Int targetPosition = MapManager.instance.FloorMap.WorldToCell(actor.Target.transform.position);
            if (isFighting || actor.FieldOfView.Contains(targetPosition))
            {
                if (!isFighting)
                {
                    isFighting = true;
                }

                float targetDistance;
                Vector3 closestTilePosition = transform.position;

                if (actor.Size.x > 1 || actor.Size.y > 1)
                {
                    float closestDistance = float.MaxValue;
                    for(int i = 0; i < actor.OccupiedTiles.Length; i++)
                    {
                        float distance = Vector3.Distance(actor.OccupiedTiles[i], actor.Target.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestTilePosition = actor.OccupiedTiles[i];
                        }
                    }
                    targetDistance = closestDistance;
                }
                else
                {
                    targetDistance = Vector3.Distance(transform.position, actor.Target.transform.position);
                }


                if (targetDistance <= 1.5f)
                {
                    Action.MeleeAction(actor, actor.Target);
                    return;
                }
                else
                {//if not in range, move toward target
                    MoveAlongPath(closestTilePosition, targetPosition);
                    return;
                }
            }
        }

        Action.WaitAction();
    }

    public override AIState SaveState() => new AIState(
        type: "HostileEnemy"
    );
}
