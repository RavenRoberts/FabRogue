using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confusion : Consumable
{
    [SerializeField] private int numberOfTurns = 10;

    public int NumberOfTurns { get => numberOfTurns; }

    public override bool Activate(Actor consumer)
    {
        consumer.GetComponent<Inventory>().SelectedConsumable = this;
        consumer.GetComponent<Player>().ToggleTargetMode();
        UIManager.instance.AddMessage($"Select a target to confuse.", "#63ffff");
        return false;
    }

    public override bool Cast(Actor consumer, Actor target)
    {
        if (target.TryGetComponent(out ConfusedEnemy confusedEnemy))
        {
            if (confusedEnemy.TurnsRemaining > 0)
            {
                UIManager.instance.AddMessage($"The {target.name} is already confused.", "#ff0000");
                consumer.GetComponent<Inventory>().SelectedConsumable = null;
                consumer.GetComponent<Player>().ToggleTargetMode();
                return false;
            }
        }
        else
        {
            confusedEnemy = target.gameObject.AddComponent<ConfusedEnemy>();
        }
        confusedEnemy.PreviousAI = target.AI;
        confusedEnemy.TurnsRemaining = NumberOfTurns;

        UIManager.instance.AddMessage($"The eyes of the {target.name} look vacant as it starts to stumble around!", "#ff0000");
        target.AI = confusedEnemy;
        Consume(consumer);
        consumer.GetComponent<Player>().ToggleTargetMode();
        return true;
    }
}
