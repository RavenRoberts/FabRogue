using UnityEngine;

public class Item : Entity
{
    [SerializeField] private Consumable consumable;
    [SerializeField] private Equippable equippable;
    public Consumable Consumable { get => consumable; }
    public Equippable Equippable { get => equippable; }

    private void OnValidate()
    {
        if (GetComponent<Consumable>())
        {
            consumable = GetComponent<Consumable>();
        }
    }

    void Start()
    {
        if (!GameManager.instance.Entities.Contains(this))
        {
            AddToGameManager();
        }
    }

    public override EntityState SaveState() => new ItemState(
        name: name,
        blocksMovement: BlocksMovement,
        isVisible: MapManager.instance.VisibleTiles.Contains(MapManager.instance.FloorMap.WorldToCell(transform.position)),
        position: transform.position,
        parent: transform.parent != null ? transform.parent.gameObject.name : ""
    );

    public void LoadState(ItemState state)
    {
        if (!state.IsVisible)
        {
            SpriteRenderer.enabled = false;
        }

        transform.position = state.Position;

        if (!string.IsNullOrEmpty(state.Parent))
        {
            GameObject parent = GameObject.Find(state.Parent);
            if (parent != null)
            {
                Inventory inventory = parent.GetComponent<Inventory>();
                if (inventory != null)
                {
                    inventory.Add(this);
                }

                if (equippable != null && state.Name.Contains("(E)"))
                {
                    Equipment equipment = parent.GetComponent<Equipment>();
                    if (equipment != null)
                    {
                        equipment.EquipItem(equippable);
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class ItemState : EntityState
{
    [SerializeField] private string parent;

    public string Parent { get => parent; set => parent = value; }

    public ItemState(EntityType type = EntityType.Item, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3(),
        string parent = "") : base(type, name, blocksMovement, isVisible, position)
    {
        this.parent = parent;
    }
}
