using System;
using UnityEngine;

/// <summary>
/// a confused enemy with stumble around aimlessly for a given number of turns before reverting to its previous ai
/// if an actor occupies a tile it is randomly moving into, it will attack
/// </summary>

[RequireComponent(typeof(Actor))]
public class ConfusedEnemy : AI
{
    [SerializeField] private AI previousAI;
    [SerializeField] private int turnsRemaining;

    public AI PreviousAI { get => previousAI; set => previousAI = value; }
    public int TurnsRemaining { get => turnsRemaining; set => turnsRemaining = value; }

    public override void RunAI()
    {
        // revert the ai back to original state when effect is ended
        if (turnsRemaining <= 0)
        {
            UIManager.instance.AddMessage($"The {gameObject.name} is no longer confused.", "#ff0000");
            GetComponent<Actor>().AI = previousAI;
            GetComponent<Actor>().AI.RunAI();
            Destroy(this);
        }
        else
        {
            //move randomly
            Vector2Int direction = UnityEngine.Random.Range(0, 8) switch
            {
                0 => new Vector2Int(0, 1), // North west
                1 => new Vector2Int(0, -1), // North
                2 => new Vector2Int(1, 0), // North east
                3 => new Vector2Int(-1, 0), // West
                4 => new Vector2Int(1, 1), // East
                5 => new Vector2Int(1, -1), // South west
                6 => new Vector2Int(-1, 1), // South
                7 => new Vector2Int(-1, -1), // South east
                _ => new Vector2Int(0, 0)
            };
            // the actor will either try to move or attack in the chosen random direction
            //its possible the actor will bump into a  wall, wasting a turn
            Action.BumpAction(GetComponent<Actor>(), direction);
            turnsRemaining--;
        }
    }

    public override AIState SaveState() => new ConfusedState(
        type: "ConfusedEnemy",
        previousAI: previousAI,
        turnsRemaining: turnsRemaining
    );

    public void LoadState(ConfusedState state)
    {
        if (state.PreviousAI == "HostileEnemy")
        {
            previousAI = GetComponent<HostileEnemy>();
        }
        turnsRemaining = state.TurnsRemaining;
    }
}

[System.Serializable]
public class ConfusedState : AIState
{
    [SerializeField] private string previousAI;
    [SerializeField] private int turnsRemaining;

    public string PreviousAI { get => previousAI; set => previousAI = value; }
    public int TurnsRemaining { get => turnsRemaining; set => turnsRemaining = value; }

    public ConfusedState(string type = "", AI previousAI = null, int turnsRemaining = 0) : base(type)
    {
        this.previousAI = previousAI.GetType().ToString();
        this.turnsRemaining = turnsRemaining;
    }
}