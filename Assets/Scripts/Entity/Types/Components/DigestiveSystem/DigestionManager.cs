using System.Collections.Generic;
using UnityEngine;

public class DigestionManager : MonoBehaviour
{

    [SerializeField] private DigestiveTract digestiveTract;

    [SerializeField] private Dictionary<Entity, int> metabolicTimers = new Dictionary<Entity, int>();
    [SerializeField] private Dictionary<Entity, int> currentIntegrity = new Dictionary<Entity, int>();

    public Dictionary<Entity, int> MetabolicTimers { get => metabolicTimers; set => metabolicTimers = value; }
    public Dictionary<Entity, int> CurrentIntegrity { get => currentIntegrity; set => currentIntegrity = value; }




    private void Awake()
    {
        if (digestiveTract == null)
        {
            digestiveTract = GetComponent<DigestiveTract>();
        }
    }
    public void HandleDigestion()
    {
        var gut = new List<Entity>(digestiveTract.DigestiveTractContents);

        foreach (var food in gut)
        {
            if (!metabolicTimers.ContainsKey(food))
            {
                metabolicTimers[food] = 1;
            }
            if (!currentIntegrity.ContainsKey(food))
            {
                currentIntegrity[food] = food.BaseIntegrity;
            }

            metabolicTimers[food] -= 1;

            if (metabolicTimers[food] <= 0)
            {
                switch (digestiveTract.FoodLocation[food])
                {
                    case DigestionChamber.Stomach:
                        HandleStomach(food);
                        break;
                    case DigestionChamber.Intestine:
                        HandleIntestine(food);
                        break;
                }
                metabolicTimers[food] = digestiveTract.Metabolism;
            }
        }
    }

    private void HandleStomach(Entity food)
    {
        currentIntegrity[food] -= digestiveTract.Acidity;
        UIManager.instance.AddMessage($"The {food.name} melts in the heat of {digestiveTract.Owner.name}'s stomach", "#0BA10B");
        if (currentIntegrity[food] <= 0)
        {
            digestiveTract.MoveToChamber(food);
            UIManager.instance.AddMessage($"The sludgy remnants of the {food.name} oozes into {digestiveTract.Owner.name}'s intestine", "#0BA10B");
        }

    }

    private void HandleIntestine(Entity food)
    {
        currentIntegrity[food] -= digestiveTract.Acidity;
        Effects.RestoreHealth(digestiveTract.Owner, food.Nutrition);
        digestiveTract.Owner.Stamina += food.Nutrition;
        UIManager.instance.AddMessage($"The {food.name} is absorbed by {digestiveTract.Owner.name}'s greedy intestine", "#0BA10B");
        if (currentIntegrity[food] <= 0)
        {
            digestiveTract.MoveToChamber(food);
        }
    }
}
