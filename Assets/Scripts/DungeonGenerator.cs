
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int _startRoomSize;
    [SerializeField] private Vector2Int _minimumRoomSize;
    
    private readonly List<DungeonNode> _dungeonNodes = new();
    
    private Coroutine _coroutine;
    
    private void Start()
    {
        var rect1 = new RectInt(new Vector2Int(0,0), _startRoomSize);
        
        _dungeonNodes.Add(new DungeonNode("Room", rect1));
    }

    private void Update()
    {
        foreach (var node in _dungeonNodes)
            if (node.Type == "Room")
            {
                AlgorithmsUtils.DebugRectInt(node.Rect, Color.red);
                DebugExtension.DebugCircle(AlgorithmsUtils.Vector2IntToVector3(node.Center), Color.green);
            }
        
        foreach (var node in _dungeonNodes)
            if (node.Type == "Door")
            {
                DebugExtension.DebugCircle(AlgorithmsUtils.Vector2IntToVector3(node.Center), Color.yellow);
                foreach (var neighbor in node.Neighbors)
                    Debug.DrawLine(AlgorithmsUtils.Vector2IntToVector3(node.Center), AlgorithmsUtils.Vector2IntToVector3(neighbor.Center), Color.yellow);
            }
    }

    [Button(enabledMode: EButtonEnableMode.Always)]
    private IEnumerator GenerateDungeon()
    {
        yield return _coroutine = StartCoroutine(GenerateRoom());
        yield return _coroutine = StartCoroutine(GenerateDoors());
        SoundManager.PlaySound("ding");
    }
    
    private IEnumerator GenerateRoom()
    {
        DungeonNode selected = new("Room", RectInt.zero);

        foreach (DungeonNode node in _dungeonNodes)
        {
            if(node.Rect.size.x > selected.Rect.width && node.Rect.x > selected.Rect.height || node.Rect.size.y > selected.Rect.width && node.Rect.size.y > selected.Rect.height)
                selected = node;
        }
            
        
        if (selected.Rect.width * 0.5f < _minimumRoomSize.x && selected.Rect.height * 0.5f < _minimumRoomSize.y)
        {
            yield break;
        }

        if(selected.Rect.width > selected.Rect.height)
            SplitRectX(selected);
        else
            SplitRectY(selected);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(GenerateRoom());
    }
    
    [Button(enabledMode: EButtonEnableMode.Always)]
    private void Reset()
    {
        StopCoroutine(_coroutine);
        _dungeonNodes.Clear();
    }

    private void SplitRectX(DungeonNode node)
    {
        int width = node.Rect.width;

        int random = 0;
        while (_minimumRoomSize.x > random)
            random = (int)Random.Range(width * 0.3f, width * 0.7f);

        Vector2Int position = node.Rect.position;
        
        
        _dungeonNodes.Add(new DungeonNode("Room", new RectInt(position, new Vector2Int(random, node.Rect.height))));
        
        _dungeonNodes.Add(new DungeonNode("Room", new RectInt(new Vector2Int(position.x + random, position.y), new Vector2Int(width - random, node.Rect.height))));
        
        _dungeonNodes.Remove(node);
    }
    
    private void SplitRectY(DungeonNode node)
    {
        int height = node.Rect.height;
        
        int random = 0;
        while (_minimumRoomSize.y > random)
            random = (int)Random.Range(height * 0.3f, height * 0.7f);
        
        Vector2Int position = node.Rect.position;
        
        _dungeonNodes.Add(new DungeonNode("Room", new RectInt(position, new Vector2Int(node.Rect.width, random))));
        
        _dungeonNodes.Add(new DungeonNode("Room", new RectInt(new Vector2Int(position.x, position.y + random), new Vector2Int(node.Rect.width, height - random))));
        
        _dungeonNodes.Remove(node);
    }

    private IEnumerator GenerateDoors()
    {
        
        for (int i = 0; i < _dungeonNodes.Count; i++)
        {
            for (int j = i + 1; j < _dungeonNodes.Count; j++)
            {
                if(_dungeonNodes[i].Type == "Door" || _dungeonNodes[j].Type == "Door") continue;
                
                var rect1 = _dungeonNodes[i].Rect;
                var rect2 = _dungeonNodes[j].Rect;
                
                if (!AlgorithmsUtils.Intersects(rect1, rect2)) continue;
                
                yield return new WaitForSeconds(0.2f);
                PlaceDoor(_dungeonNodes[i], _dungeonNodes[j]);
            }
        }
        
        yield return null;
    }

    private void PlaceDoor(DungeonNode node1, DungeonNode node2)
    {
        var rect1 = node1.Rect;
        var rect2 = node2.Rect;

        if (IsDiagonallyAdjacent(rect1, rect2)) return;
        
         TryAddDoorHorizontal(node1, node2);
         TryAddDoorVertical(node1, node2);
    }

    private void TryAddDoorHorizontal(DungeonNode node1, DungeonNode node2)
    {
        var rect1 = node1.Rect;
        var rect2 = node2.Rect;
        
        float min = Mathf.Max(rect1.position.x, rect2.position.x);
        float max = Mathf.Min(rect1.position.x + rect1.width, rect2.position.x + rect2.width);
        
        if (rect2.position.y == rect1.position.y + rect1.height)
        {
            var newRect = new RectInt(new Vector2Int(
                    (int)Mathf.Floor(min + (max - min) / 2),
                    rect1.position.y + rect1.size.y
                ),
                new Vector2Int(1, 1));
                
            var newNode = new DungeonNode("Door", newRect);
                
            newNode.Neighbors.Add(node1);
            newNode.Neighbors.Add(node2);
            node1.Neighbors.Add(newNode);
            node2.Neighbors.Add(newNode);
                
            _dungeonNodes.Add(newNode);
        }
        else if (rect1.position.y == rect2.position.y + rect2.height)
        {
            var newRect = new RectInt(new Vector2Int(
                    (int)Mathf.Floor(min + (max - min) / 2),
                    rect1.position.y
                ),
                new Vector2Int(1, 1));
                
            var newNode = new DungeonNode("Door", newRect);
                
            newNode.Neighbors.Add(node1);
            newNode.Neighbors.Add(node2);
            node1.Neighbors.Add(newNode);
            node2.Neighbors.Add(newNode);
                
            _dungeonNodes.Add(newNode);
        }
    }
    
    private void TryAddDoorVertical(DungeonNode node1, DungeonNode node2)
    {
        var rect1 = node1.Rect;
        var rect2 = node2.Rect;
        
        float min = Mathf.Max(rect1.position.y, rect2.position.y) + (rect1.height * 0.3f);
        float max = Mathf.Min(rect1.position.y + rect1.height, rect2.position.y + rect2.height) - (rect1.height * 0.3f);
        
        if (rect2.position.x == rect1.position.x + rect1.width)
        {
            var newRect = new RectInt(new Vector2Int(
                    rect1.position.x + rect1.size.x,
                    (int)Mathf.Floor(min + (max - min) / 2)
                ),
                new Vector2Int(1, 1));
                
            var newNode = new DungeonNode("Door", newRect);
                
            newNode.Neighbors.Add(node1);
            newNode.Neighbors.Add(node2);
            node1.Neighbors.Add(newNode);
            node2.Neighbors.Add(newNode);
                
            _dungeonNodes.Add(newNode);
        }
        else if (rect1.position.x == rect2.position.x + rect2.width)
        {
            var newRect = new RectInt(new Vector2Int(
                    rect1.position.x,
                    (int)Mathf.Floor(min + (max - min) / 2)
                ),
                new Vector2Int(1, 1));
                
            var newNode = new DungeonNode("Door", newRect);
                
            newNode.Neighbors.Add(node1);
            newNode.Neighbors.Add(node2);
            node1.Neighbors.Add(newNode);
            node2.Neighbors.Add(newNode);
                
            _dungeonNodes.Add(newNode);
        }
    }
    
    private bool IsDiagonallyAdjacent(RectInt a, RectInt b)
    {
        var aBottomRight = new Vector2Int(a.xMax, a.yMin);
        var aTopRight = new Vector2Int(a.xMax, a.yMax);
        var aTopLeft = new Vector2Int(a.xMin, a.yMax);
        var aBottomLeft = new Vector2Int(a.xMin, a.yMin);

        var bBottomRight = new Vector2Int(b.xMax, b.yMin);
        var bTopRight = new Vector2Int(b.xMax, b.yMax);
        var bTopLeft = new Vector2Int(b.xMin, b.yMax);
        var bBottomLeft = new Vector2Int(b.xMin, b.yMin);
        
        if (aTopRight == bBottomLeft) return true;
        if (aBottomRight == bTopLeft) return true;
        if (aTopLeft == bBottomRight) return true;
        if (aBottomLeft == bTopRight) return true;

        return false;
    }
}