using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class FloorData
{
    [SerializeField]
    private TileData[,] map;
    [SerializeField]
    private Vector2Int stairPosition;
    [SerializeField]
    private Vector2Int spawnPoint;

    public TileData[,] Map { get => map; set => map = value; }
    public Vector2Int StairPosition { get => stairPosition; set => stairPosition = value; }
    public Vector2Int SpawnPoint { get => spawnPoint; set => spawnPoint = value; }
    public Vector2Int Size => new Vector2Int(Map.GetLength(0), Map.GetLength(1));

    public bool IsSpawnPoint(Vector2Int index) => SpawnPoint == index;
    public bool IsSpawnPoint(int x, int y) => IsSpawnPoint(new Vector2Int(x, y));
    public bool IsStair(Vector2Int index) => StairPosition == index;
    public bool IsStair(int x, int y) => IsStair(new Vector2Int(x, y));

    public FloorData() { }

    public FloorData(TileData[,] map)
    {
        Create(map);
    }

    public void Create(TileData[,] map)
    {
        this.map = map;
        var roomTiles = map.ToArray().Where(tile => tile.IsRoom).ToArray();
        var spawnPointIndex = UnityEngine.Random.Range(0, roomTiles.Length);
        var stairPointIndex = UnityEngine.Random.Range(0, roomTiles.Length);
        
        stairPosition = roomTiles[stairPointIndex].Position;
        spawnPoint = roomTiles[spawnPointIndex].Position;
    }
}
