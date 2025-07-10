
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    public delegate void DungeonEvent(List<DungeonNode> nodes);
    public Vector2Int StartRoomSize;
    public DungeonEvent OnDungeonGenerationComplete;

    [SerializeField] private bool _showDebugLines;
    [SerializeField] private Vector2Int _minimumRoomSize;
    [SerializeField] private float _delay;
    
    private readonly List<DungeonNode> _dungeonNodes = new();
    private Coroutine _coroutine;
    
    private void Start()
    {
        var rect1 = new RectInt(new Vector2Int(0,0), StartRoomSize);
        
        _dungeonNodes.Add(new DungeonNode("Room", rect1));
    }

    private void Update()
    {
        if(!_showDebugLines) return;
        
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
        
        if(!DFS(_dungeonNodes))
            Debug.LogWarning("Not all nodes are connected");
        
        OnDungeonGenerationComplete?.Invoke(_dungeonNodes);
        SoundManager.PlaySound("ding");
    }
    
    private IEnumerator GenerateRoom()
    {
        DungeonNode selected = new("Empty", RectInt.zero);

        foreach (DungeonNode node in _dungeonNodes)
        {
            if (node.Rect.width >= _minimumRoomSize.x * 2 || node.Rect.height >= _minimumRoomSize.y * 2)
            {
                selected = node;
                break;
            }
        }
        
        if(selected.Type == "Empty")
            yield break;
        
        if(selected.Rect.width > selected.Rect.height)
            SplitRectX(selected);
        else
            SplitRectY(selected);

        yield return new WaitForSeconds(_delay);
        yield return StartCoroutine(GenerateRoom());
    }
    
    [Button(enabledMode: EButtonEnableMode.Always)]
    private void Reset()
    {
        StopCoroutine(_coroutine);
        _dungeonNodes.Clear();
        Start();
    }

    private void SplitRectX(DungeonNode node)
    {
        int width = node.Rect.width;
        
        int random = Random.Range(_minimumRoomSize.x, width - _minimumRoomSize.x);

        Vector2Int position = node.Rect.position;
        
        
        _dungeonNodes.Add(new DungeonNode("Room", new RectInt(position, new Vector2Int(random, node.Rect.height))));
        
        _dungeonNodes.Add(new DungeonNode("Room", new RectInt(new Vector2Int(position.x + random, position.y), new Vector2Int(width - random, node.Rect.height))));
        
        _dungeonNodes.Remove(node);
    }
    
    private void SplitRectY(DungeonNode node)
    {
        int height = node.Rect.height;
        
        int random = Random.Range(_minimumRoomSize.y, height - _minimumRoomSize.y);
        
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
                
                yield return new WaitForSeconds(_delay);
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

        if(max - min <= 1) return;
        
        int offsetY;

        if (rect2.position.y == rect1.position.y + rect1.height)
            offsetY = rect1.position.y + rect1.size.y;
        else if (rect1.position.y == rect2.position.y + rect2.height)
            offsetY = rect1.position.y;
        else
            return;
        
        var newRect = new RectInt(new Vector2Int(
                Mathf.RoundToInt( Random.Range(min+1, max-1)),
                offsetY
            ),
            new Vector2Int(1, 1));
                
        var newNode = new DungeonNode("Door", newRect);
                
        newNode.Neighbors.Add(node1);
        newNode.Neighbors.Add(node2);
        node1.Neighbors.Add(newNode);
        node2.Neighbors.Add(newNode);
                
        _dungeonNodes.Add(newNode);
    }
    
    private void TryAddDoorVertical(DungeonNode node1, DungeonNode node2)
    {
        var rect1 = node1.Rect;
        var rect2 = node2.Rect;
        
        float min = Mathf.Max(rect1.position.y, rect2.position.y);
        float max = Mathf.Min(rect1.position.y + rect1.height, rect2.position.y + rect2.height);
        
        if(max - min <= 1) return;
        
        int offsetX = 0;
        
        if (rect2.position.x == rect1.position.x + rect1.width)
            offsetX = rect1.position.x + rect1.size.x;
        else if (rect1.position.x == rect2.position.x + rect2.width)
            offsetX = rect1.position.x;
        else
            return;
        
        var newRect = new RectInt(new Vector2Int(
                offsetX,
                Mathf.RoundToInt( Random.Range(min+1, max-1))
            ),
            new Vector2Int(1, 1));
                
        var newNode = new DungeonNode("Door", newRect);
                
        newNode.Neighbors.Add(node1);
        newNode.Neighbors.Add(node2);
        node1.Neighbors.Add(newNode);
        node2.Neighbors.Add(newNode);
                
        _dungeonNodes.Add(newNode);
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

    private bool DFS(List<DungeonNode> graph)
    {
        DungeonNode root = graph[0];
        
        var visited = new List<DungeonNode>();
        var stack = new Stack<DungeonNode>();
    
        stack.Push(root);
    
        while (stack.Count > 0)
        {
            var node = stack.Pop();

            if (visited.Contains(node)) continue;
            
            visited.Add(node);
            
            foreach (var neighboringCity in node.Neighbors)
                stack.Push(neighboringCity);
        }

        return visited.Count == graph.Count;
    }
}