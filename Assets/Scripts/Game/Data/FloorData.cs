using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class FloorData
{
    [SerializeField]
    private TileData[,] map;
    [SerializeField]
    private Point stairPosition;
    [SerializeField]
    private Point spawnPoint;
    [SerializeField]
    private List<Room> rooms;
    [SerializeField]
    private List<Path> paths;

    public TileData[,] Map { get => map; set => map = value; }
    public Point StairPosition { get => stairPosition; set => stairPosition = value; }
    public Point SpawnPoint { get => spawnPoint; set => spawnPoint = value; }
    public Point Size => new Point(Map.GetLength(0), Map.GetLength(1));

    public List<Room> Rooms => rooms;
    public List<Path> Paths => paths;

    public bool IsSpawnPoint(Point index) => SpawnPoint == index;
    public bool IsSpawnPoint(int x, int y) => IsSpawnPoint(new Point(x, y));
    public bool IsStair(Point index) => StairPosition == index;
    public bool IsStair(int x, int y) => IsStair(new Point(x, y));

    public FloorData() { }

    public FloorData(TileData[,] map, List<Room> roomList, List<Path> paths)
    {
        Create(map, roomList, paths);
    }

    public void Create(TileData[,] map, List<Room> roomList, List<Path> paths)
    {
        this.map = map;
        var roomTiles = map.ToArray().Where(tile => tile.IsRoom).ToArray();
        var spawnPointIndex = UnityEngine.Random.Range(0, roomTiles.Length);
        var stairPointIndex = UnityEngine.Random.Range(0, roomTiles.Length);
        
        stairPosition = roomTiles[stairPointIndex].Position;
        spawnPoint = roomTiles[spawnPointIndex].Position;
        rooms = roomList;
        this.paths = paths;
    }
}
