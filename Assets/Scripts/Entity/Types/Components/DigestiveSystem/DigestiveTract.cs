using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class DigestiveTract : MonoBehaviour
{
    [SerializeField] private Actor owner;
    [SerializeField] private List<Entity> stomachContents = new List<Entity>();
    [SerializeField] private List<Entity> intestineContents = new List<Entity>();

    [SerializeField] private List<Entity> digestiveTractContents = new List<Entity>();
    [SerializeField] private Dictionary<Entity, DigestionChamber> foodLocation;


    [SerializeField] private int acidity = 0;
    [SerializeField] private int metabolism = 0;

    [SerializeField] private int passiveStamina = 0;

    public Actor Owner { get => owner; }
    public List<Entity> StomachContents { get => stomachContents; }
    public List <Entity> IntestineContents { get => intestineContents; }

    public List<Entity> DigestiveTractContents { get => digestiveTractContents; }
    public Dictionary<Entity, DigestionChamber> FoodLocation { get => foodLocation; }

    public int Acidity { get => acidity; }
    public int Metabolism { get => metabolism; }

    public Dictionary<DigestionChamber, DigestionChamber?> digestionPathways = new Dictionary<DigestionChamber, DigestionChamber?>()
    {
        {DigestionChamber.Stomach, DigestionChamber.Intestine },
        {DigestionChamber.Intestine, null }
    };

    public DigestionManager digestionManager;

    private void Awake()
    {
        owner = GetComponent<Actor>();
        foodLocation = new Dictionary<Entity, DigestionChamber>();
    }

    public void PassiveStamina() //putting this here for now because idk where else it can go neatly
    {
        passiveStamina++;

        if (passiveStamina >= Metabolism)
        {
            owner.Stamina++;
            passiveStamina = 0;
        }
    }

    public void AddToStomach(Entity food)
    {
        digestiveTractContents.Add(food);
        foodLocation[food] = DigestionChamber.Stomach;

    }

    public void MoveToChamber(Entity food)
    {
        var current = FoodLocation[food];
        if (!digestionPathways.ContainsKey(current))
        {
            return;
        }

        var next = digestionPathways[current];
        if (next == null)
        {
            //i guess we add scat logic here?
            UIManager.instance.AddMessage($"{Owner.name} spreads their cheeks, releasing the {food.name} back into the dungeon as shit", "#0BA10B");
            DigestiveTractContents.Remove(food);
            FoodLocation.Remove(food);
            digestionManager.CurrentIntegrity.Remove(food);
            digestionManager.MetabolicTimers.Remove(food);
            return;
        }

        FoodLocation[food] = next.Value;
        digestionManager.CurrentIntegrity[food] = food.BaseIntegrity;

    }
}

public enum DigestionChamber
{
    Stomach,
    Intestine,

    Duodenum,
    Jejenum,
    Ileum,
    RisingColon,
    TransverseColon,
    DescentingColon,
}