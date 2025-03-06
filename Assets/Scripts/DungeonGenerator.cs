
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int _startRoomSize;
    [SerializeField] private Vector2Int _minimumRoomSize;
    
    private List<RectInt> _rects = new();
    
    
    private void Start()
    {
        var rect1 = new RectInt(new Vector2Int(0,0), _startRoomSize);
        _rects.Add(rect1);
    }

    private void Update()
    {
        foreach (RectInt rect in _rects)
            AlgorithmsUtils.DebugRectInt(rect, Color.red);
    }

    [Button(enabledMode: EButtonEnableMode.Always)]
    private IEnumerator GenerateDungeon()
    {
        yield return StartCoroutine(GenerateRoom());
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
            Debug.Log(selected.width);
            Debug.Log(selected.height);
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
    private void ResetRooms()
    {
        _rects.Clear();
        _rects.Add(new RectInt(new Vector2Int(0,0), _startRoomSize));
    }

    private void SplitRectX(RectInt rect)
    {
        int width = rect.width;

        int random = 0;
        while (_minimumRoomSize.x > random)
            random = (int)Random.Range(width * 0.3f, width * 0.7f);

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
}