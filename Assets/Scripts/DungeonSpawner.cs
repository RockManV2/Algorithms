
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
    
    private Dictionary<Vector3, GameObject> _dungeonMap = new();
    [SerializeField] private NavMeshSurface _floorSurface;

    private void Awake()
    {
        _dungeonGenerator = GetComponent<DungeonGenerator>();
    }

    private void Start() =>
        _dungeonGenerator.OnDungeonGenerationComplete += SpawnDungeon;
    
    private void SpawnDungeon(List<DungeonNode> dungeonNodes)
    {
        GenerateFloor();
        GenerateWalls(dungeonNodes);
        GenerateDoors(dungeonNodes);
         StartCoroutine(BakeFloor());
    }

    private void GenerateWalls(List<DungeonNode> dungeonNodes)
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
                    var x = Instantiate(_wallPrefab, leftWallPosition, Quaternion.Euler(0,90,0));
                
                    _dungeonMap[leftWallPosition] = x;
                }
                
                Vector3 rightWallPosition =
                    AlgorithmsUtils.Vector2IntToVector3(node.Rect.position) + new Vector3(node.Rect.width, 0, i);
                
                if (!_dungeonMap.ContainsKey(rightWallPosition))
                {
                    var y = Instantiate(_wallPrefab, rightWallPosition, Quaternion.Euler(0,90,0));
                
                    _dungeonMap[rightWallPosition] = y;
                }
            }
                    
            for (int i = 0; i < node.Rect.width; i++)
            {
                Vector3 topWallPosition =
                    AlgorithmsUtils.Vector2IntToVector3(node.Rect.position) + new Vector3(i, 0, node.Rect.height);

                if (!_dungeonMap.ContainsKey(topWallPosition))
                {
                    var x = Instantiate(_wallPrefab, topWallPosition, Quaternion.identity);
                    _dungeonMap[topWallPosition] = x;
                }
                
                Vector3 bottomWallPosition =
                    AlgorithmsUtils.Vector2IntToVector3(node.Rect.position) + new Vector3(i, 0, 0);
                
                
                if (!_dungeonMap.ContainsKey(bottomWallPosition))
                {
                    var y = Instantiate(_wallPrefab, bottomWallPosition, Quaternion.identity);
                    _dungeonMap[bottomWallPosition] = y;
                }
            }
        }
    }
    
    private void GenerateDoors(List<DungeonNode> dungeonNodes)
    {
        foreach (DungeonNode node in dungeonNodes)
        {
            if (node.Type != "Door") continue;

            var wall = _dungeonMap[AlgorithmsUtils.Vector2IntToVector3(node.Rect.position)];
            
            Destroy(wall);
        }
    }

    private void GenerateFloor()
    {
        var floor = Instantiate(_floorPrefab, transform.position, Quaternion.identity);
        floor.transform.localScale =
            new Vector3(_dungeonGenerator.StartRoomSize.x * 0.1f, 1, _dungeonGenerator.StartRoomSize.y * 0.1f);
        
        floor.transform.position = new Vector3(_dungeonGenerator.StartRoomSize.x, -0.5f, _dungeonGenerator.StartRoomSize.y ) / 2;

        _floorSurface = floor.GetComponent<NavMeshSurface>();
    }

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
}
