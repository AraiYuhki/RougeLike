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
    public List<Path> DeletedPaths { get; private set; }

    public List<Room> Rooms => rooms;
    public List<Path> Paths => paths;

    public bool IsSpawnPoint(Point index) => SpawnPoint == index;
    public bool IsSpawnPoint(int x, int y) => IsSpawnPoint(new Point(x, y));
    public bool IsStair(Point index) => StairPosition == index;
    public bool IsStair(int x, int y) => IsStair(new Point(x, y));

    public FloorData() { }

    public FloorData(TileData[,] map, List<Room> roomList, List<Path> paths, List<Path> deletedPath)
    {
        Create(map, roomList, paths);
        DeletedPaths = deletedPath;
    }

    public FloorData(int width, int height, List<Room> roomList, List<Path> pathList)
    {
        map = new TileData[width, height];
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                map[x, y] = new TileData() { Position = new Point(x, y), Type = TileType.Wall };

        rooms = roomList;
        paths = pathList;

        UpdateMap();

        var roomTiles = map.ToArray().Where(tile => tile.IsRoom).ToArray();
        stairPosition = roomTiles.Random().Position;
        spawnPoint = roomTiles.Random().Position;
    }

    public void DeletePath(float deletePercent)
    {
        var deletedPath = new List<Path>();
        foreach (var room in rooms.Where(room => room.ConnectedRooms.Count > 1))
        {
            foreach (var next in room.ConnectedRooms.ToList())
            {
                var nextRoom = rooms.First(room => room.Id == next);
                if (room.ConnectedRooms.Count <= 1 || nextRoom.ConnectedRooms.Count <= 1) continue;
                var random = UnityEngine.Random.Range(0f, 1f);
                if (random > deletePercent) continue;
                room.RemovePath(next);
                nextRoom.RemovePath(room.Id);
                var target = paths.FirstOrDefault(path
                    => (path.FromRoomId == room.Id && path.ToRoomId == next)
                    || (path.FromRoomId == next && path.ToRoomId == room.Id));
                paths.Remove(target);
                deletedPath.Add(target);
                Debug.Log($"delete path {room.Id} -> {next}");
            }
        }
        var closedRooms = new List<Room>();
        var retryCount = 0;
        while (true)
        {
            closedRooms = Dijkstra.FindClosedPath(rooms, paths);
            if (closedRooms == null) break;
            foreach (var room in closedRooms)
            {
                var target = deletedPath.FirstOrDefault(path => path.FromRoomId == room.Id || path.ToRoomId == room.Id);
                if (target != null)
                {
                    var from = rooms.First(room => room.Id == target.FromRoomId);
                    var to = rooms.First(room => room.Id == target.ToRoomId);
                    from.AddPath(to.Id, target, target.From);
                    to.AddPath(from.Id, target, target.To);
                    paths.Add(target);
                    deletedPath.Remove(target);
                    Debug.Log($"restore path {target.FromRoomId} -> {target.ToRoomId}");
                    break;
                }
            }
            retryCount++;
            if (retryCount >= 100)
            {
                Debug.LogError("Over retry counts");
                break;
            }
        }
        DeletedPaths = deletedPath;
        UpdateMap();
    }

    public void UpdateMap()
    {
        if (DeletedPaths != null)
        {
            foreach (var path in DeletedPaths.Where(path => path != null))
            {
                Debug.Log($"deleted path {path.FromRoomId} -> {path.ToRoomId}");
                foreach (var position in path.PathPositionList)
                {
                    var tile = map[position.X, position.Y];
                    if (tile.Type == TileType.Room) continue;
                    tile.Type = TileType.Deleted;
                }
            }
        }

        foreach (var path in paths)
        {
            foreach (var position in path.PathPositionList)
            {
                map[position.X, position.Y].Position = position;
                map[position.X, position.Y].Type = TileType.Path;
                map[position.X, position.Y].Id = path.Id;
            }
        }

        foreach (var room in rooms)
        {
            var x = room.X;
            var y = room.Y;
            var roomWidth = room.Width;
            var roomHeight = room.Height;
            for (var row = y; row < y + roomHeight; row++)
            {
                for (var column = x; column < x + roomWidth; column++)
                {
                    map[column, row].Position = new Point(column, row);
                    map[column, row].Type = TileType.Room;
                    map[column, row].Id = room.Id;
                }
            }
        }
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
