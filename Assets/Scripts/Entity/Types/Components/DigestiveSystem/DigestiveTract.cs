using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
public class DigestiveTract : MonoBehaviour
{
    [SerializeField] private Actor owner;
    [SerializeField] private List<Entity> stomachContents = new List<Entity>();
    [SerializeField] private List<Entity> intestineContents = new List<Entity>();
    [SerializeField] private int acidity = 0;
    public Actor Owner { get => owner; }
    public List<Entity> StomachContents { get => stomachContents; }
    public List <Entity> IntestineContents { get => intestineContents; }
    public int Acidity { get => acidity; }

   

    private void Awake()
    {
        owner = GetComponent<Actor>();
    }

    public void AddToStomach(Entity food)
    {
        stomachContents.Add(food);
    }

    public void MoveToIntestine(Entity food)
    {
        if (stomachContents.Contains(food))
        {
            stomachContents.Remove(food);
            intestineContents.Add(food);
        }
    }

    public void EmptyIntestine(Entity food)
    {
        if (intestineContents.Contains(food))
        {
            intestineContents.Remove(food);
        }
    }

}
