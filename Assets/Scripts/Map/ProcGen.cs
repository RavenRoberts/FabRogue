using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

sealed class ProcGen : MonoBehaviour
{
    /// <summary>
    /// Generate new dungeon map
    /// </summary>
    
    public void GenerateDungeon(int mapWidth, int mapHeight, int roomMaxSize, int roomMinSize, int maxRooms, int maxMonstersPerRoom, int maxItemsPerRoom, List<RectangularRoom> rooms)
    {
       for (int roomNum = 0; roomNum < maxRooms; roomNum++)
        {
            int roomWidth = UnityEngine.Random.Range(roomMinSize, roomMaxSize);
            int roomHeight = UnityEngine.Random.Range(roomMinSize, roomMaxSize);

            int roomX = UnityEngine.Random.Range(0, mapWidth - roomWidth - 1);
            int roomY = UnityEngine.Random.Range(0, mapHeight - roomHeight - 1);

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

            PlaceEntities(newRoom, maxMonstersPerRoom, maxItemsPerRoom);

            rooms.Add(newRoom);
        }

        //The first room where the player spawns
        MapManager.instance.CreateEntity("Player", rooms[0].Center());
    }

    ///<summary>
    ///return L shaped corridor between two points using bresenham lines
    ///</summary>

    private void TunnelBetween(RectangularRoom oldRoom, RectangularRoom newRoom)
    {
        Vector2Int oldRoomCenter = oldRoom.Center();
        Vector2Int newRoomCenter = newRoom.Center();
        Vector2Int tunnelCorner;

        if (UnityEngine.Random.value < 0.5f)
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

    private void PlaceEntities(RectangularRoom newRoom, int maximumMonsters, int maximumItems)
    {
        int numberOfMonsters = UnityEngine.Random.Range(0, maximumMonsters + 1);
        int numberOfItems = UnityEngine.Random.Range(0, maximumItems + 1);

        for (int monster = 0; monster < numberOfMonsters;)
        {
            int x = UnityEngine.Random.Range(newRoom.X, newRoom.X + newRoom.Width);
            int y = UnityEngine.Random.Range(newRoom.Y, newRoom.Y + newRoom.Height);

            if (x == newRoom.X || x == newRoom.X + newRoom.Width - 1 || y == newRoom.Y || y == newRoom.Y + newRoom.Height - 1)
            {
                continue;
            }

            for (int entity = 0; entity < GameManager.instance.Entities.Count; entity++)
            {
                Vector3Int pos = MapManager.instance.FloorMap.WorldToCell(GameManager.instance.Entities[entity].transform.position);

                if (pos.x == x && pos.y == y)
                {
                    return;
                }
            }

            if (UnityEngine.Random.value < 0.8f)
            {
                MapManager.instance.CreateEntity("Orc", new Vector2(x, y));
            }
            else
            {
                MapManager.instance.CreateEntity("Troll", new Vector2(x, y));
            }
            monster++;
        }

        for (int item = 0; item < numberOfItems;)
        {
            int x = UnityEngine.Random.Range(newRoom.X, newRoom.X + newRoom.Width);
            int y = UnityEngine.Random.Range(newRoom.Y, newRoom.Y + newRoom.Height);

            if (x == newRoom.X || x == newRoom.X + newRoom.Width - 1 || y == newRoom.Y || y == newRoom.Y + newRoom.Height - 1)
            {
                continue;
            }

            for (int entity = 0; entity < GameManager.instance.Entities.Count; entity++)
            {
                Vector3Int pos = MapManager.instance.FloorMap.WorldToCell(GameManager.instance.Entities[entity].transform.position);

                if (pos.x == x && pos.y == y)
                {
                    return;
                }
            }

            MapManager.instance.CreateEntity("Potion of Health", new Vector2(x, y));
            item++;
        }
    }



}
