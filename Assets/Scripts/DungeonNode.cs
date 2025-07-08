using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonNode
{
    private List<DungeonNode> Neighbors = new();
    public RectInt Bounds;

    public DungeonNode(RectInt bounds)
    {
        RectInt Bounds = bounds;
    }
}
