using System.Diagnostics;
using UnityEngine;

static public class Action
{
    static public void EscapeAction()
    {
        UnityEngine.Debug.Log("Quit");
        //Application.Quit();
    }

    static public bool BumpAction(Entity entity, Vector2 direction)
    {
        Entity target = GameManager.instance.GetBlockingEntityAtLocation(entity.transform.position + (Vector3)direction);

        if (target)
        {
            MeleeAction(target);
            return false;
        }
        else
        {
            MovementAction(entity, direction);
            return true;
        }
    }

    static public void MeleeAction(Entity target)
    {
        UnityEngine.Debug.Log($"You attack the {target.name}");
        GameManager.instance.EndTurn();
    }

    static public void MovementAction(Entity entity, Vector2 direction)
    {
        //UnityEngine.Debug.Log($"{entity.name} moves {direction}!");
        entity.Move(direction);
        entity.UpdateFieldOfView();
        GameManager.instance.EndTurn();
    }

    static public void SkipAction(Entity entity)
    {
        if (entity.GetComponent<Player>())
        {
            //Debug.Log("You Decided to skip your turn");
        }
        else
        {
            //Debug.log("The {entity.name} skips its turn");
        }
        GameManager.instance.EndTurn();
    }
}
