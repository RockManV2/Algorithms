
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class DungeonSpawner : MonoBehaviour
{
    private DungeonGenerator _dungeonGenerator;
    [SerializeField] private GameObject _wallPrefab;
    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _cornerPrefab;
    [SerializeField] private Transform _dungeonParent;

    [SerializeField] private float _delay;
    
    private Dictionary<Vector3, GameObject> _dungeonMap = new();
    private NavMeshSurface _floorSurface;
    
    
    // Get refrence(s)
    private void Awake() =>
        _dungeonGenerator = GetComponent<DungeonGenerator>();

    // Subscribe methods to events with some lambda sorcery
    private void Start()
    {
        _dungeonGenerator.OnDungeonGenerationComplete += (nodes) => StartCoroutine(SpawnDungeon(nodes));
        _dungeonGenerator.OnDungeonReset += (_) => Reset();
    }
        
    // Main method used to create the while 
    private IEnumerator SpawnDungeon(List<DungeonNode> dungeonNodes)
    {
        yield return StartCoroutine(GenerateFloor());
        yield return StartCoroutine(GenerateWalls(dungeonNodes));
        yield return StartCoroutine(GenerateDoors(dungeonNodes));
        StartCoroutine(BakeFloor());
    }

    /// <summary>
    /// Loops through all rooms and generates walls on the 4 sides of the rooms, while checking if it doesnt already
    /// exist there.
    /// </summary>
    private IEnumerator GenerateWalls(List<DungeonNode> dungeonNodes)
    {
        foreach (DungeonNode node in dungeonNodes)
        {
            if (node.Type != "Room") continue;
            
            for (int i = 0; i < node.Rect.height; i++)
            {
                
                Vector3 leftWallPosition =
                    AlgorithmsUtils.Vector2IntToVector3(node.Rect.position) + new Vector3(0, 0, i);
                
                if (!_dungeonMap.ContainsKey(leftWallPosition))
                {
                    var x = Instantiate(_wallPrefab, leftWallPosition, Quaternion.Euler(0,90,0), _dungeonParent);
                
                    _dungeonMap[leftWallPosition] = x;
                }
                
                Vector3 rightWallPosition =
                    AlgorithmsUtils.Vector2IntToVector3(node.Rect.position) + new Vector3(node.Rect.width, 0, i);
                
                if (!_dungeonMap.ContainsKey(rightWallPosition))
                {
                    var y = Instantiate(_wallPrefab, rightWallPosition, Quaternion.Euler(0,90,0), _dungeonParent);
                
                    _dungeonMap[rightWallPosition] = y;
                }
            }
                    
            for (int i = 0; i < node.Rect.width; i++)
            {
                Vector3 topWallPosition =
                    AlgorithmsUtils.Vector2IntToVector3(node.Rect.position) + new Vector3(i, 0, node.Rect.height);

                if (!_dungeonMap.ContainsKey(topWallPosition))
                {
                    var newObject = Instantiate(_wallPrefab, topWallPosition, Quaternion.identity, _dungeonParent);
                    _dungeonMap[topWallPosition] = newObject;
                }
                
                Vector3 bottomWallPosition =
                    AlgorithmsUtils.Vector2IntToVector3(node.Rect.position) + new Vector3(i, 0, 0);
                
                
                if (!_dungeonMap.ContainsKey(bottomWallPosition))
                {
                    var y = Instantiate(_wallPrefab, bottomWallPosition, Quaternion.identity, _dungeonParent);
                    _dungeonMap[bottomWallPosition] = y;
                }
            }
            yield return new WaitForSeconds(_delay);
        }
    }
    
    /// <summary>
    /// Removes walls at places where doors should be
    /// </summary>
    private IEnumerator GenerateDoors(List<DungeonNode> dungeonNodes)
    {
        foreach (DungeonNode node in dungeonNodes)
        {
            if (node.Type != "Door") continue;

            var wall = _dungeonMap[AlgorithmsUtils.Vector2IntToVector3(node.Rect.position)];
            
            Destroy(wall);
            yield return new WaitForSeconds(_delay);
        }
    }

    /// <summary>
    /// Generates the floor in the middle of the dungeon at the correct size
    /// </summary>
    private IEnumerator GenerateFloor()
    {
        var floor = Instantiate(_floorPrefab, transform.position, Quaternion.identity, _dungeonParent);
        floor.transform.localScale =
            new Vector3(_dungeonGenerator.StartRoomSize.x * 0.1f, 1, _dungeonGenerator.StartRoomSize.y * 0.1f);
        
        floor.transform.position = new Vector3(_dungeonGenerator.StartRoomSize.x, -0.5f, _dungeonGenerator.StartRoomSize.y ) / 2;

        _floorSurface = floor.GetComponent<NavMeshSurface>();
        yield return new WaitForSeconds(_delay);
    }

    /// <summary>
    /// Bakes the floor.
    /// </summary>
    /// <returns></returns>
    [Button(enabledMode: EButtonEnableMode.Always)]
    private IEnumerator BakeFloor()
    {
        yield return new WaitForSeconds(1);
        if (_floorSurface != null)
        {
            // Honorary mention
            // Debug.Log("Yippie2bomba");
            _floorSurface.BuildNavMesh();
        }
    }

    /// <summary>
    /// Resets all spawned objects and clears the map dictionary
    /// </summary>
    private void Reset()
    {
        foreach (Transform child in _dungeonParent)
            Destroy(child.gameObject);
        
        _dungeonMap.Clear();
    }
}
