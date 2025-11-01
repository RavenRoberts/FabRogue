using UnityEngine;

[System.Serializable]
public class TileData
{
    [SerializeField] private bool isExplored, isVisible;

    public bool IsExplored { get => isExplored; set => isExplored = value; }
    public bool IsVisible { get => isVisible; set => isVisible = value; }
}
