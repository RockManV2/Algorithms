
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int _startRoomSize;
    [SerializeField] private Vector2Int _minimumRoomSize;
    
    private readonly List<RectInt> _rects = new();
    private readonly List<RectInt> _doors = new();
    
    
    private void Start()
    {
        var rect1 = new RectInt(new Vector2Int(0,0), _startRoomSize);
        _rects.Add(rect1);
    }

    private void Update()
    {
        foreach (RectInt rect in _rects)
            AlgorithmsUtils.DebugRectInt(rect, Color.red);
        
        foreach (RectInt rect in _doors)
            AlgorithmsUtils.DebugRectInt(rect, Color.green);
    }

    [Button(enabledMode: EButtonEnableMode.Always)]
    private IEnumerator GenerateDungeon()
    {
        yield return StartCoroutine(GenerateRoom());
        yield return StartCoroutine(GenerateDoors());
        SoundManager.PlaySound("ding");
    }
    
    private IEnumerator GenerateRoom()
    {
        bool x = Random.Range(0, 2) != 0;

        RectInt selected = new();
        foreach (RectInt rect in _rects)
            if(rect.size.x > selected.width && rect.size.x > selected.height || rect.size.y > selected.width && rect.size.y > selected.height)
                selected = rect;
        
        if (selected.width * 0.5f < _minimumRoomSize.x && selected.height * 0.5f < _minimumRoomSize.y)
        {
            yield break;
        }
            
        
        if(selected.width > selected.height)
            SplitRectX(selected);
        else
            SplitRectY(selected);

        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(GenerateRoom());
    }
    
    [Button(enabledMode: EButtonEnableMode.Always)]
    private void Reset()
    {
        _rects.Clear();
        _doors.Clear();
        _rects.Add(new RectInt(new Vector2Int(0,0), _startRoomSize));
    }

    private void SplitRectX(RectInt rect)
    {
        int width = rect.width;

        int random = 0;
        while (_minimumRoomSize.x > random)
            random = (int)Random.Range(width * 0.3f, width * 0.7f);

        Vector2Int position = rect.position;
        
        _rects.Add(new RectInt(position, new Vector2Int(random, rect.height)));
        
        _rects.Add(new RectInt(new Vector2Int(position.x + random, position.y), new Vector2Int(width - random, rect.height)));
        
        _rects.Remove(rect);
    }
    
    private void SplitRectY(RectInt rect)
    {
        int height = rect.height;
        
        int random = 0;
        while (_minimumRoomSize.y > random)
            random = (int)Random.Range(height * 0.3f, height * 0.7f);
        
        Vector2Int position = rect.position;
        
        // Base Rect
        _rects.Add(new RectInt(position, new Vector2Int(rect.width, random)));
        
        // New Rect
        _rects.Add(new RectInt(new Vector2Int(position.x, position.y + random), new Vector2Int(rect.width, height - random)));
        
        _rects.Remove(rect);
    }

    private IEnumerator GenerateDoors()
    {
        for (int i = 0; i < _rects.Count; i++)
        {
            for (int j = i + 1; j < _rects.Count; j++)
            {
                var rect1 = _rects[i];
                var rect2 = _rects[j];
                
                if (!AlgorithmsUtils.Intersects(rect1, rect2)) continue;
                
                yield return new WaitForSeconds(0.5f);
                PlaceDoor(rect1, rect2);
            }
        }
        
        yield return null;
    }

    private void PlaceDoor(RectInt rect1, RectInt rect2)
    {
        if (rect1.position.y != rect2.position.y)
        {
            float min = Mathf.Min(rect1.position.x + rect1.width, rect2.position.x + rect2.width) * 0.9f;
            float max = Mathf.Max(rect1.position.x, rect2.position.x) * 1.1f;
            
            _doors.Add(new RectInt(new Vector2Int(
                    (int)Random.Range(min, max),
                    rect1.position.y + rect1.size.y
                ),
                new Vector2Int(1, 1)));
        }
        else if (rect1.position.x != rect2.position.x)
        {
            float min = Mathf.Min(rect1.position.y + rect1.height, rect2.position.y + rect2.height) * 0.9f;
            float max = Mathf.Max(rect1.position.y, rect2.position.y) * 1.1f;
            
            _doors.Add(new RectInt(new Vector2Int(
                    rect1.position.x + rect1.size.x,
                    (int)Random.Range(min, max)
                ),
                new Vector2Int(1, 1)));
        }

    }
}