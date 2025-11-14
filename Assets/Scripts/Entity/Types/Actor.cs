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
    [SerializeField] private int maxHp, hp, maxStamina, stamina, baseDefense, basePower;

    [Header("Containers")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private AbilitySlots abilitySlots;
    [SerializeField] private Equipment equipment;
    [SerializeField] private DigestiveTract digestiveTract;
    [SerializeField] private Fighter fighter;
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

            if (hp == 0)
            {
                Effects.Die(this);
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
    public int BaseDefense { get => baseDefense; set => baseDefense = value; }
    public int BasePower { get => basePower; set => basePower = value; }
    public Actor Target { get => target; set => target = value; }

    public int Power()
    {
        return basePower + PowerBonus();
    }

    public int Defense()
    {
        return baseDefense + DefenseBonus();
    }

    public int DefenseBonus()
    {
        if (GetComponent<Equipment>() is not null)
        {
            return GetComponent<Equipment>().DefenseBonus();
        }

        return 0;
    }
    public int PowerBonus()
    {
        if (GetComponent<Equipment>() is not null)
        {
            return GetComponent<Equipment>().PowerBonus();
        }

        return 0;
    }

    public Inventory Inventory { get => inventory; }
    public AbilitySlots AbilitySlots { get => abilitySlots; }
    public Equipment Equipment { get => equipment; }
    public DigestiveTract DigestiveTract { get => digestiveTract; }
    public AI AI { get => aI; set => aI = value; }
    public Fighter Fighter { get => fighter; set => fighter = value; }


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

        if (GetComponent<Fighter>())
        {
            fighter = GetComponent<Fighter>();
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
        else if (fighter != null)
        {
            Effects.Die(this);
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
        fighterState: fighter != null ? fighter.SaveState() : null,
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

        if (state.FighterState != null)
        {
            fighter.LoadState(state.FighterState);
        }

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
    [SerializeField] private FighterState fighterState;
    [SerializeField] private LevelState levelState;

    public bool IsAlive { get => isAlive; set => isAlive = value; }
    public AIState CurrentAI { get => currentAI; set => currentAI = value; }
    public FighterState FighterState { get => fighterState; set => fighterState = value; }
    public LevelState LevelState { get => levelState; set => levelState = value; }

    public ActorState(EntityType type = EntityType.Actor, string name = "", bool blocksMovement = false, bool isVisible = false, Vector3 position = new Vector3(),
        bool isAlive = true, AIState currentAI = null, FighterState fighterState = null, LevelState levelState = null) : base(type, name, blocksMovement, isVisible, position)
    {
        this.isAlive = isAlive;
        this.currentAI = currentAI;
        this.fighterState = fighterState;
        this.levelState = levelState;
    }
}