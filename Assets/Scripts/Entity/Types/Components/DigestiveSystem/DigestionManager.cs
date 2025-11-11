using System.Collections.Generic;
using UnityEngine;

public class DigestionManager : MonoBehaviour
{

    [SerializeField] private DigestiveTract digestiveTract;

    [SerializeField] private Dictionary<Entity, int> stomachTimers = new Dictionary<Entity, int>();
    [SerializeField] private Dictionary<Entity, int> intestineTimers = new Dictionary<Entity, int>();


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
            if (!stomachTimers.ContainsKey(food))
            {
                stomachTimers[food] = food.Integrity;
            }
            stomachTimers[food] -= digestiveTract.Acidity;
            UIManager.instance.AddMessage($"The {food.name} melts in the heat of {digestiveTract.Owner.name}'s stomach", "#0BA10B");

            if (stomachTimers[food] <= 0)
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
            if (!intestineTimers.ContainsKey(food))
            {
                intestineTimers[food] = food.Integrity;
            }
            intestineTimers[food] -= digestiveTract.Acidity;
            Effects.RestoreHealth(digestiveTract.Owner, food.Nutrition);
            digestiveTract.Owner.Stamina += food.Nutrition;
            UIManager.instance.AddMessage($"The {food.name} is absorbed by {digestiveTract.Owner.name}'s greedy intestine", "#0BA10B");

            if (intestineTimers[food] <= 0)
            {
                digestiveTract.EmptyIntestine(food);
                intestineTimers.Remove(food);

                UIManager.instance.AddMessage($"{digestiveTract.Owner.name} spreads their cheeks, releasing the {food.name} back into the dungeon as shit", "#0BA10B");
            }
        }

    }


}
