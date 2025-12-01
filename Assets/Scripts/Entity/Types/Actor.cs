using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Actor : Entity
{
    [SerializeField] private bool isAlive = true; //read only

    [Header("AI Properties")]
    [SerializeField] private int fieldOfViewRange = 8;
    [SerializeField] private List<Vector3Int> fieldOfView = new List<Vector3Int>();
    [SerializeField] private AI aI;

    [Header("Stats")]
    [SerializeField] private Level level;
    [SerializeField] private int maxHp, hp, maxStamina, stamina, basePower;
    [SerializeField] private List<FlatDefenseEntry> baseFlatDefense = new();
    [SerializeField] private List<PercentDefenseEntry> basePercentDefense = new();

    [System.Serializable]
    public struct Damage
    {
        public int Amount;
        public DamageType Type;

        public Damage(int amount, DamageType type)
        {
            Amount = amount;
            Type = type;
        }
    }

    [System.Serializable]
    public struct FlatDefenseEntry
    {
        public DamageType type;
        public int value;
    }

    [System.Serializable]
    public struct PercentDefenseEntry
    {
        public DamageType type;
        public float value; // stored as percentage 0-100
    }

    [Header("Containers")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private AbilitySlots abilitySlots;
    [SerializeField] private Equipment equipment;
    [SerializeField] private DigestiveTract digestiveTract;
    [SerializeField] private Actor target;

    AdamMilVisibility algorithm;

    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public List<Vector3Int> FieldOfView { get => fieldOfView; }
    public Level Level { get => level; set => level = value; }

    public int Hp
    {
        get => hp; set
        {
            hp = Mathf.Max(0, Mathf.Min(value, maxHp));

            if (GetComponent<Player>())
            {
                UIManager.instance.SetHealth(hp, maxHp);
            }
        }
    }
    public int MaxHp
    {
        get => maxHp; set
        {
            maxHp = value;
            if (GetComponent<Player>())
            {
                UIManager.instance.SetHealthMax(maxHp);
            }
        }
    }
    public int Stamina
    {
        get => stamina; set
        {
            stamina = Mathf.Max(0, Mathf.Min(value, maxStamina));

            if (GetComponent<Player>())
            {
                UIManager.instance.SetStamina(stamina, maxStamina);
            }

        }
    }
    public int MaxStamina
    {
        get => maxStamina; set
        {
            maxStamina = value;
            if (GetComponent<Player>())
            {
                UIManager.instance.SetStaminaMax(maxStamina);
            }
        }
    }

    public int BasePower { get => basePower; set => basePower = value; }
    public Actor Target { get => target; set => target = value; }

    public int Power(DamageType type = DamageType.Physical)
    {
        return basePower + GetDamageBonus(type);
    }

    public int GetDamageBonus(DamageType type)
    {
        return equipment != null ? equipment.GetDamageBonus(type) : 0;
    }
    
    public int GetFlatDefense(DamageType type)
    {
        int baseFlat = 0;
        foreach (var entry in baseFlatDefense)
        {
            if (entry.type == type)
            {
                baseFlat += entry.value;
            }
        }
        return baseFlat + (equipment ? equipment.GetFlatDefense(type) : 0);
    }

    public float GetPercentDefense(DamageType type)
    {
        float basePercent = 0f;
        foreach (var entry in basePercentDefense)
        {
            if (entry.type == type)
            {
                basePercent += entry.value;
            }
        }
        return basePercent + (equipment ? equipment.GetPercentDefense(type) : 0f);
    }


    public Inventory Inventory { get => inventory; }
    public AbilitySlots AbilitySlots { get => abilitySlots; }
    public Equipment Equipment { get => equipment; }
    public DigestiveTract DigestiveTract { get => digestiveTract; }
    public AI AI { get => aI; set => aI = value; }


    private void OnValidate()
    {
        if (GetComponent<AI>())
        {
            aI = GetComponent<AI>();
        }

        if (GetComponent<Inventory>())
        {
            inventory = GetComponent<Inventory>();
        }

        if (GetComponent<AbilitySlots>())
        {
            abilitySlots = GetComponent<AbilitySlots>();
        }

        if (GetComponent<Level>())
        {
            level = GetComponent<Level>();
        }

        if (GetComponent<Equipment>())
        {
            equipment = GetComponent<Equipment>();
        }
        if (GetComponent<DigestiveTract>())
        {
            digestiveTract = GetComponent<DigestiveTract>();
        }
    }

    void Start()
    {
        if (!GameManager.instance.Actors.Contains(this))
        {
            AddToGameManager();
        }

        if (isAlive)
        {
            algorithm = new AdamMilVisibility();
            UpdateFieldOfView();
        }

        if (Size.x > 1 || Size.y > 1)
        {
            OccupiedTiles = GetOccupiedTiles();
        }

        if (GetComponent<Player>())
        {
            UIManager.instance.SetHealthMax(maxHp);
            UIManager.instance.SetHealth(hp, maxHp);
            UIManager.instance.SetStaminaMax(maxStamina);
            UIManager.instance.SetStamina(stamina, maxStamina);
        }

    }

    public override void AddToGameManager()
    {
        base.AddToGameManager();

        if (GetComponent<Player>())
        {
            GameManager.instance.InsertActor(this, 0);
        }
        else
        {
            GameManager.instance.AddActor(this);
        }
    }

    public void UpdateFieldOfView()
    {
        Vector3Int gridPosition = MapManager.instance.FloorMap.WorldToCell(transform.position);

        fieldOfView.Clear();
        algorithm.Compute(gridPosition, fieldOfViewRange, fieldOfView);

        if (GetComponent<Player>())
        {
            MapManager.instance.UpdateFogMap(fieldOfView);
            MapManager.instance.SetEntitiesVisibilities();
        }
    }

    public override EntityState SaveState() => new ActorState(
        name: name,
        blocksMovement: BlocksMovement,
        isAlive: IsAlive,
        isVisible: MapManager.instance.VisibleTiles.Contains(MapManager.instance.FloorMap.WorldToCell(transform.position)),
        position: transform.position,
        currentAI: aI != null ? AI.SaveState() : null,

        hp: hp,
        maxHp: maxHp,
        stamina: stamina,
        maxStamina: maxStamina,

        levelState: level != null && GetComponent<Player>() ? level.SaveState() : null
    );

    public void LoadState(ActorState state)
    {
        transform.position = state.Position;
        isAlive = state.IsAlive;

        if (!isAlive)
        {
            GameManager.instance.RemoveActor(this);
        }

        if (!state.IsVisible)
        {
            SpriteRenderer.enabled = false;
        }

        if (state.CurrentAI != null)
        {
            if (state.CurrentAI.Type == "HostileEnemy")
            {
                aI = GetComponent<HostileEnemy>();
            }
            else if (state.CurrentAI.Type == "Confused Enemy")
            {
                aI = gameObject.AddComponent<ConfusedEnemy>();

                ConfusedState confusedState = state.CurrentAI as ConfusedState;
                ((ConfusedEnemy)aI).LoadState(confusedState);
            }
        }

        Hp = state.Hp;
        MaxHp = state.MaxHp;
        Stamina = state.Stamina;
        MaxStamina = state.MaxStamina;

        if (state.LevelState != null)
        {
            level.LoadState(state.LevelState);
        }
    }
}

[System.Serializable]
public class ActorState : EntityState
{
    [SerializeField] private bool isAlive;
    [SerializeField] private AIState currentAI;
    [SerializeField] private LevelState levelState;
    [SerializeField] private int hp;
    [SerializeField] private int maxHp;
    [SerializeField] private int stamina;
    [SerializeField] private int maxStamina;



    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public AIState CurrentAI { get => currentAI; set => currentAI = value; }
    public LevelState LevelState { get => levelState; set => levelState = value; }
    public int Hp { get => hp; set => hp = value; }
    public int MaxHp { get => maxHp; set => maxHp = value; }
    public int Stamina { get => stamina; set => stamina = value; }
    public int MaxStamina { get => maxStamina; set => maxStamina = value; }

    public ActorState(
        string name,
        bool blocksMovement,
        bool isVisible,
        Vector3 position,
        bool isAlive,
        AIState currentAI, 
        int hp,
        int maxHp,
        int stamina,
        int maxStamina,
        LevelState levelState
    ) : base(EntityType.Actor, name, blocksMovement, isVisible, position)
    {
        this.isAlive = isAlive;
        this.currentAI = currentAI;
        this.hp = hp;
        this.maxHp = maxHp;
        this.stamina = stamina;
        this.maxStamina = maxStamina;
        this.levelState = levelState;
    }
}