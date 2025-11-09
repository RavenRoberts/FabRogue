using System.Collections.Generic;
using UnityEngine;

public class DigestionManager : MonoBehaviour
{

    [SerializeField] private DigestiveTract digestiveTract;

    private Dictionary<Entity, int> stomachTimers = new Dictionary<Entity, int>();
    private Dictionary<Entity, int> intestineTimers = new Dictionary<Entity, int>();


    private void Awake()
    {
        if (digestiveTract == null)
        {
            digestiveTract = GetComponent<DigestiveTract>();
        }
    }

    public void TickDigestion()
    {
        HandleStomach();
        HandleIntestine();
    }

    private void HandleStomach()
    {
        var stomach = new List<Entity>(digestiveTract.StomachContents);

        foreach (var food in stomach)
        {
            int timer = stomachTimers[food];
            if (!stomachTimers.ContainsKey(food))
            {
                timer = food.Integrity;
            }
            timer -= digestiveTract.Acidity;
            UIManager.instance.AddMessage($"The {food.name} melts in the heat of {digestiveTract.Owner.name}'s stomach", "#0BA10B");

            if (timer <= 0)
            {
                digestiveTract.MoveToIntestine(food);
                intestineTimers[food] = food.Integrity;
                stomachTimers.Remove(food);
                UIManager.instance.AddMessage($"The sludgy remnants of the {food.name} oozes into {digestiveTract.Owner.name}'s intestine", "#0BA10B");
            }
        }
    }

    private void HandleIntestine()
    {
        var intestine = new List<Entity>(digestiveTract.IntestineContents);

        foreach ( var food in intestine)
        {
            int timer = intestineTimers[food];
            if (!intestineTimers.ContainsKey(food))
            {
                timer = food.Integrity;
            }
            timer -= digestiveTract.Acidity;
            digestiveTract.Owner.GetComponent<Fighter>().Heal(food.Nutrition);
            UIManager.instance.AddMessage($"The {food.name} is absorbed by {digestiveTract.Owner.name}'s greedy intestine", "#0BA10B");

            if (timer <= 0)
            {
                digestiveTract.EmptyIntestine(food);
                intestineTimers.Remove(food);

                UIManager.instance.AddMessage($"{digestiveTract.Owner.name} spreads their cheeks, releasing the {food.name} back into the dungeon as shit", "#0BA10B");
            }
        }

    }


}
