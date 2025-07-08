using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonNode
{
    public string Type;
    public List<DungeonNode> Neighbors = new();
    public RectInt Rect;
    public Vector2Int Center => Rect.position + Rect.size / 2;

    public DungeonNode(string type, RectInt rect)
    {
        Type = type;
        Rect = rect;
    }
}
