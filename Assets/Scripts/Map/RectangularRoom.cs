using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class RectangularRoom 
{
    [SerializeField] private int x, y, width, height;

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public int Width { get => width; set => width = value; }
    public int Height { get => height; set => height = value; }

    public RectangularRoom(int x, int y, int width, int height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }
    /// <summary>
    /// return the center of the room
    /// </summary>

    public Vector2Int Center() => new Vector2Int(x + width / 2, y + height / 2);

    /// <summary>
    /// Return a random inner position of the room
    /// </summary>

    public Vector2Int RandomPoint() => new Vector2Int(UnityEngine.Random.Range(x + 1, x + width - 1), UnityEngine.Random.Range(y + 1, y + height - 1));

    public Bounds GetBounds() => new Bounds(new Vector3(x, y, 0), new Vector3(width, height, 0));

    /// <summary>
    /// return the area of this room as boundsint
    /// </summary>
    
    public BoundsInt GetBoundsInt() => new BoundsInt(new Vector3Int(x, y, 0), new Vector3Int(width, height, 0));

    /// <summary>
    /// Return true if this room overlaps with another rectangular room
    /// </summary>
    
    public bool Overlaps(List<RectangularRoom> otherRooms)
    {
        foreach (RectangularRoom otherRoom in otherRooms)
        {
            if (GetBounds().Intersects(otherRoom.GetBounds()))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// return a list of positions along the walls of the room
    /// </summary>

    public List<Vector2Int> GetWallPositions()
    {
        List<Vector2Int> wallPositions = new List<Vector2Int>();

        //top and bottom walls
        for (int i = x; i < x + width; i++)
        {
            wallPositions.Add(new Vector2Int(i, y));//bottom
            wallPositions.Add(new Vector2Int(i, y + height - 1));//top
        }
        //left and right walls
        for (int j = y + 1; j < y + height - 1; j++)
        {
            wallPositions.Add(new Vector2Int(x, j));//left
            wallPositions.Add(new Vector2Int(x + width - 1, j));//right
        }

        return wallPositions;
    }
}
