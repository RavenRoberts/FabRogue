using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{

    public static MapManager instance;

    [Header("Map Settings")]
    [SerializeField] private int width = 80;
    [SerializeField] private int height = 45;
    [SerializeField] private int roomMaxSize = 10;
    [SerializeField] private int roomMinSize = 6;
    [SerializeField] private int maxRooms = 30;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase fogTile;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorMap;
    [SerializeField] private Tilemap obstacleMap;
    [SerializeField] private Tilemap fogMap;

    [Header("Features")]
    [SerializeField] private List<RectangularRoom> rooms = new List<RectangularRoom>();
    [SerializeField] private List<Vector3Int> visibleTiles = new List<Vector3Int>();
    [SerializeField] private Dictionary<Vector3Int, TileData> tiles = new Dictionary<Vector3Int, TileData>();


    public TileBase FloorTile { get => floorTile; }
    public TileBase WallTile { get => wallTile; }
    public Tilemap FloorMap { get => floorMap; }
    public Tilemap ObstacleMap { get => obstacleMap; }

    public List<RectangularRoom> Rooms { get => rooms; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ProcGen procGen = new ProcGen();
        procGen.GenerateDungeon(width, height, roomMaxSize, roomMinSize, maxRooms, rooms);

        
        Instantiate(Resources.Load<GameObject>("NPC"), new Vector3(40 + 5.5f, 25 + 0.5f, 0), Quaternion.identity).name = "NPC";

        Camera.main.transform.position = new Vector3(40, 20.25f, -10);
        Camera.main.orthographicSize = 27;
    }

    /// <summary>Return True is x and y are inside the bounds of the map.</summary>
    public bool InBounds(int x, int y) => 0 <= x && x < width && 0 <= y && y < height;

    public void CreatePlayer(Vector2 position)
    {
        Instantiate(Resources.Load<GameObject>("Player"), new Vector3(position.x + 0.5f, position.y + 0.5f, 0), Quaternion.identity).name = "Player";
    }


}
//testing