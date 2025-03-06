
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    private List<RectInt> _rects = new();
    private int _roomSplits = 5;
    
    private void Start()
    {
        var rect1 = new RectInt(new Vector2Int(0,0), new Vector2Int(20,20));
        _rects.Add(rect1);
    }
    
    [Button(enabledMode: EButtonEnableMode.Always)]
    private void SplitRoom()
    {
        bool x = Random.Range(0, 2) != 0;
        int randomRect = Random.Range(0, _rects.Count);
            
        if(x)
            SplitRectX(_rects[randomRect]);
        else
            SplitRectY(_rects[randomRect]);
    }

    private void Update()
    {
        foreach (RectInt rect in _rects)
            AlgorithmsUtils.DebugRectInt(rect, Color.red);
    }

    private void SplitRectX(RectInt rect)
    {
        int width = rect.width;
        
        int random = (int)Random.Range(width * 0.2f, width * 0.8f);
        Vector2Int position = rect.position;
        
        // Base Rect
        _rects.Add(new RectInt(position, new Vector2Int(random, rect.height)));
        
        // New Rect
        _rects.Add(new RectInt(new Vector2Int(position.x + random, position.y), new Vector2Int(width - random, rect.height)));
        
        _rects.Remove(rect);
    }
    
    private void SplitRectY(RectInt rect)
    {
        int height = rect.height;
        
        int random = (int)Random.Range(height * 0.2f, height * 0.8f);
        Vector2Int position = rect.position;
        
        // Base Rect
        _rects.Add(new RectInt(position, new Vector2Int(rect.width, random)));
        
        // New Rect
        _rects.Add(new RectInt(new Vector2Int(position.x, position.y + random), new Vector2Int(rect.width, height - random)));
        
        _rects.Remove(rect);
    }
}