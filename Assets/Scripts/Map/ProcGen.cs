using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using SysRandom = System.Random;
using UnityRandom = UnityEngine.Random;


sealed class ProcGen
{
    private List<Tuple<int, int>> maxItemsByFloor = new List<Tuple<int, int>>
    {
        new Tuple<int, int>(1, 1),
        new Tuple<int, int>(4, 2),
        new Tuple<int, int>(7, 3),
        new Tuple<int, int>(10, 4),
    };
    private List<Tuple<int, int>> maxMonstersByFloor = new List<Tuple<int, int>>
    {
        new Tuple<int, int>(1, 2),
        new Tuple<int, int>(4, 3),
        new Tuple<int, int>(6, 5),
        new Tuple<int, int>(8, 7),
        new Tuple<int, int>(10, 10),
    };
    private List<Tuple<int, string, int>> itemChances = new List<Tuple<int, string, int>>
    {
        new Tuple<int, string, int>(0, "Potion of Health", 35),
        new Tuple<int, string, int>(0, "Apple", 60),
        new Tuple<int, string, int>(0, "Confusion Scroll", 10),
        new Tuple<int, string, int>(0, "Lightning Scroll", 25), new Tuple<int, string, int>(0, "Sword", 5),
        new Tuple<int, string, int>(0, "Fireball Scroll", 25), new Tuple<int, string, int>(0, "Chain Mail", 15),
    };
    private List<Tuple<int, string, int>> monsterChances = new List<Tuple<int, string, int>>
    {
        new Tuple<int, string, int>(1, "Orc", 40),
        new Tuple<int, string, int>(1, "Goblin", 60),
        new Tuple<int, string, int>(1, "Slime", 20),
        new Tuple<int, string, int>(0, "Troll", 15),
        new Tuple<int, string, int>(5, "Troll", 30),
        new Tuple<int, string, int>(7, "Troll", 60),
    };

    private readonly HashSet<Vector3Int> tunnelCoords = new();

    public int GetMaxValueForFloor(List<Tuple<int, int>> values, int floor)
    {
        int currentValue = 0;

        foreach (Tuple<int, int> value in values)
        {
            if (floor >= value.Item1)
            {
                currentValue = value.Item2;
            }
        }

        return currentValue;
    }

    public List<string> GetEntitiesAtRandom(List<Tuple<int, string, int>> chances, int numberOfEntities, int floor)
    {
        List<string> entities = new List<string>();
        List<int> weightedChances = new List<int>();

        foreach(Tuple<int, string, int> chance in chances)
        {
            if (floor >= chance.Item1)
            {
                entities.Add(chance.Item2);
                weightedChances.Add(chance.Item3);
            }
        }

        SysRandom rnd = new SysRandom();
        List<string> chosenEntities = rnd.Choices(entities, weightedChances, numberOfEntities);

        return chosenEntities;
    }




    /// <summary>
    /// Generate new dungeon map
    /// </summary>

    public void GenerateDungeon(int mapWidth, int mapHeight, int roomMaxSize, int roomMinSize, int maxRooms, List<RectangularRoom> rooms, bool isNewGame)
    {
       for (int roomNum = 0; roomNum < maxRooms; roomNum++)
        {
            int roomWidth = UnityRandom.Range(roomMinSize, roomMaxSize);
            int roomHeight = UnityRandom.Range(roomMinSize, roomMaxSize);

            int roomX = UnityRandom.Range(0, mapWidth - roomWidth - 1);
            int roomY = UnityRandom.Range(0, mapHeight - roomHeight - 1);

            RectangularRoom newRoom = new RectangularRoom(roomX, roomY, roomWidth, roomHeight);

            // Check for room overlaps
            if (newRoom.Overlaps(rooms))
            {
                continue;
            }
            // If no overlaps, room is valid

            //dig out this rooms inner area and build walls
            for (int x = roomX; x < roomX + roomWidth; x++)
            {
                for (int y = roomY; y < roomY + roomHeight; y++)
                {
                    if (x == roomX || x == roomX + roomWidth - 1 || y == roomY || y == roomY + roomHeight - 1)
                    {
                        if (SetWallTileIfEmpty(new Vector3Int(x, y)))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        SetFloorTile(new Vector3Int(x, y));
                    }
                }
            }

            if(rooms.Count != 0)
            {
                //tunnel to previous room
                TunnelBetween(rooms[rooms.Count - 1], newRoom);

            } else
            {
                
            }

            PlaceEntities(newRoom, SaveManager.instance.CurrentFloor);

            rooms.Add(newRoom);
        }

       //add doors to the rooms

       foreach (RectangularRoom room in rooms)
        {
            PlaceDoor(room, rooms);
        }

        //add stairs to the last room
        MapManager.instance.FloorMap.SetTile((Vector3Int)rooms[rooms.Count - 1].RandomPoint(), MapManager.instance.DownStairsTile);

        //add player to the first room
        Vector3Int playerPos = (Vector3Int)rooms[0].RandomPoint();
        int maxAttempts = 10, attempts = 0;

        while (GameManager.instance.GetActorAtLocation(new Vector2(playerPos.x + 0.5f, playerPos.y + 0.5f)) is not null)
        {
            playerPos = (Vector3Int)rooms[0].RandomPoint();
            if (attempts >= maxAttempts)
            {
                Actor actor = GameManager.instance.GetActorAtLocation(new Vector2(playerPos.x + 0.5f, playerPos.y + 0.5f));

                if (actor is not null)
                {
                    GameManager.instance.RemoveActor(actor);
                    GameManager.instance.RemoveEntity(actor);
                    GameManager.instance.DestroyEntity(actor);
                }
                break;
            }
            attempts++;
        }

        MapManager.instance.FloorMap.SetTile(playerPos, MapManager.instance.UpStairsTile);

        if (!isNewGame)
        {
            GameManager.instance.Actors[0].transform.position = new Vector3(playerPos.x + 0.5f, playerPos.y + 0.5f, 0);
        }
        else
        {
           GameObject player = MapManager.instance.CreateEntity("Player", (Vector2Int)playerPos);
           Actor playerActor = player.GetComponent<Actor>();

            Item starterWeapon = MapManager.instance.CreateEntity("Dagger", (Vector2Int)playerPos).GetComponent<Item>();
            Item starterArmor = MapManager.instance.CreateEntity("Leather Armor", (Vector2Int)playerPos).GetComponent<Item>();

            playerActor.Inventory.Add(starterWeapon);
            playerActor.Inventory.Add(starterArmor);

            playerActor.Equipment.EquipToSlot("Weapon", starterWeapon, false);
            playerActor.Equipment.EquipToSlot("Armor", starterArmor, false);
        }
    }

    private void PlaceDoor(RectangularRoom room, List<RectangularRoom> allRooms)
    {
        var wallPositions = room.GetWallPositions();

        foreach (var pos in wallPositions)
        {
            Vector3Int tilePos = new Vector3Int(pos.x, pos.y, 0);

            if (IsAdjacentToFloor(tilePos) && !IsPartOfAnotherRoom(tilePos, allRooms, room) && !IsAdjacentToDoor(tilePos))
            {
                //remove the floor tile
                MapManager.instance.FloorMap.SetTile(tilePos, null);
                //remove wall tile
                MapManager.instance.ObstacleMap.SetTile(tilePos, null);
                //add the door tile
                MapManager.instance.InteractableMap.SetTile(tilePos, MapManager.instance.ClosedDoor);
            }
        }
    }

    private bool IsAdjacentToFloor(Vector3Int pos)
    {
        var adjacentTiles = new Vector3Int[]
        {
            pos + Vector3Int.up,
            pos + Vector3Int.down,
            pos + Vector3Int.left,
            pos + Vector3Int.right,
        };

        bool isHorizontalCorridor = MapManager.instance.FloorMap.GetTile(adjacentTiles[2]) && MapManager.instance.FloorMap.GetTile(adjacentTiles[3]);
        bool isVerticalCorridor = MapManager.instance.FloorMap.GetTile(adjacentTiles[0]) && MapManager.instance.FloorMap.GetTile(adjacentTiles[1]);

        if (isHorizontalCorridor || isVerticalCorridor)
        {
            Vector3Int orthogonalDirection = isHorizontalCorridor ? adjacentTiles[0] : adjacentTiles[2];
            Vector3Int oppositeDirection = isHorizontalCorridor ? adjacentTiles[1] : adjacentTiles[3];

            if (!MapManager.instance.FloorMap.GetTile(orthogonalDirection) && !MapManager.instance.FloorMap.GetTile(oppositeDirection))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPartOfAnotherRoom(Vector3Int pos, List<RectangularRoom> allRooms, RectangularRoom currentRoom)
    {
        foreach (RectangularRoom room in allRooms)
        {
            if (room != currentRoom && room.GetBoundsInt().Contains(pos))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsAdjacentToDoor(Vector3Int pos)
    {
        var adjacentTiles = new Vector3Int[]
        {
            pos + Vector3Int.up,
            pos + Vector3Int.down,
            pos + Vector3Int.left,
            pos + Vector3Int.right
        }; 
        
        foreach (Vector3Int tile in adjacentTiles)
        {
            if (MapManager.instance.InteractableMap.GetTile(tile) == MapManager.instance.ClosedDoor)
            {
                return true;
            }
        }
        return false;
    }

    ///<summary>
    ///return L shaped corridor between two points using bresenham lines
    ///</summary>

    private void TunnelBetween(RectangularRoom oldRoom, RectangularRoom newRoom)
    {
        Vector2Int oldRoomCenter = oldRoom.Center();
        Vector2Int newRoomCenter = newRoom.Center();
        Vector2Int tunnelCorner;

        if (UnityRandom.value < 0.5f)
        {
            //horizontal then vertical
            tunnelCorner = new Vector2Int(newRoomCenter.x, oldRoomCenter.y);
        }
        else
        {
            //vertical then horizontal
            tunnelCorner = new Vector2Int(oldRoomCenter.x, newRoomCenter.y);
        }

        //gen coords for this tunnel
        List<Vector2Int> tunnelCoords = new List<Vector2Int>();
        BresenhamLine.Compute(oldRoomCenter, tunnelCorner, tunnelCoords);
        BresenhamLine.Compute(tunnelCorner, newRoomCenter, tunnelCoords);

        //set tiles for this tunnel
        for (int i = 0; i < tunnelCoords.Count; i++)
        {
            SetFloorTile(new Vector3Int(tunnelCoords[i].x, tunnelCoords[i].y));
            this.tunnelCoords.Add(new Vector3Int(tunnelCoords[i].x, tunnelCoords[i].y));

            //set floor tile
            MapManager.instance.FloorMap.SetTile(new Vector3Int(tunnelCoords[i].x, tunnelCoords[i].y, 0), MapManager.instance.FloorTile);

            //set wall tiles around tunnel
            for (int x = tunnelCoords[i].x - 1; x <= tunnelCoords[i].x + 1; x++)
            {
                for (int y = tunnelCoords[i].y - 1; y <= tunnelCoords[i].y + 1; y++)
                {
                    if (SetWallTileIfEmpty(new Vector3Int(x, y)))
                    {
                        continue;
                    }
                }
            }


        }
    }

    private bool SetWallTileIfEmpty(Vector3Int pos)
    {
        if (MapManager.instance.FloorMap.GetTile(pos))
        {
            return true;
        }
        else
        {
            MapManager.instance.ObstacleMap.SetTile(pos, MapManager.instance.WallTile);
            return false;
        }
    }

    private void SetFloorTile(Vector3Int pos)
    {
        if (MapManager.instance.ObstacleMap.GetTile(pos))
        {
            MapManager.instance.ObstacleMap.SetTile(pos, null);
        }
        MapManager.instance.FloorMap.SetTile(pos, MapManager.instance.FloorTile);

    }

    private void PlaceEntities(RectangularRoom newRoom, int floorNumber)
    {
        int maxAttempts = 10;
        int numberOfMonsters = UnityRandom.Range(0, GetMaxValueForFloor(maxMonstersByFloor, floorNumber) + 1);
        int numberOfItems = UnityRandom.Range(0, GetMaxValueForFloor(maxItemsByFloor, floorNumber) + 1);

        List<string> monsterNames = GetEntitiesAtRandom(monsterChances, numberOfMonsters, floorNumber);
        List<string> itemNames = GetEntitiesAtRandom(itemChances, numberOfItems, floorNumber);

        List<string> entityNames = monsterNames.Concat(itemNames).ToList();

        foreach (string entityName in entityNames)
        {
            Vector2Int entityPos = Vector2Int.zero;
            Entity entity = MapManager.instance.CreateEntity(entityName, entityPos).GetComponent<Entity>();
            bool canPlace = false;

            for (int attempts = 0; attempts < maxAttempts; attempts++)
            {
                entityPos = newRoom.RandomPoint();
                Vector2 entityPosFloat = new Vector3(entityPos.x + 0.5f, entityPos.y + 0.5f);

                if (entity.Size.x > 1 || entity.Size.y > 1)
                {
                    entity.transform.position = entityPosFloat;
                    entity.OccupiedTiles = entity.GetOccupiedTiles();

                    canPlace = entity.OccupiedTiles.All(pos => MapManager.instance.IsValidPosition(pos)
                                    && GameManager.instance.GetActorsAtLocation(pos).Length <= 1);
                    if (canPlace)
                    {
                        break;
                    }
                }
                else if (GameManager.instance.GetActorAtLocation(entityPosFloat) == null)
                {
                    entity.transform.position = entityPosFloat;
                    canPlace = true;
                    break;
                }
            }

            if (!canPlace)
            {
                if (entity.GetComponent<Actor>() is not null)
                {
                    GameManager.instance.RemoveActor(entity.GetComponent<Actor>());
                }

                GameManager.instance.RemoveEntity(entity);
                GameManager.instance.DestroyEntity(entity);
            }
        }
    }
}
