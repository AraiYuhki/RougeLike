using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class FloorData
{
    [SerializeField]
    private Point stairPosition;
    [SerializeField]
    private Point spawnPoint;
    [SerializeField]
    private List<Room> rooms;
    [SerializeField]
    private List<Path> paths;

    public TileData[,] Map { get; private set; }
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

    public FloorData(FloorInfo master, FloorData original)
    {
        var size = master.Size;
        Map = new TileData[size.x, size.y];
        for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
                Map[x, y] = new TileData() { Position = new Point(x, y), Type = TileType.Wall };

        rooms = original.rooms;
        paths = original.paths;

        UpdateMap();

        var roomTiles = Map.ToArray().Where(tile => tile.IsRoom).ToArray();
        stairPosition = original.stairPosition;
        spawnPoint = original.spawnPoint;
    }

    public FloorData(int width, int height, List<Room> roomList, List<Path> pathList)
        => Initialize(width, height, roomList, pathList);

    private void Initialize(int width, int height, List<Room> roomList, List<Path> pathList)
    {
        Map = new TileData[width, height];
        for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                Map[x, y] = new TileData() { Position = new Point(x, y), Type = TileType.Wall };

        rooms = roomList;
        paths = pathList;

        UpdateMap();

        var roomTiles = Map.ToArray().Where(tile => tile.IsRoom).ToArray();
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
            closedRooms = BackTracking.FindIsolatedRoom(rooms, paths);
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
                    var tile = Map[position.X, position.Y];
                    if (tile.Type == TileType.Room) continue;
                    tile.Type = TileType.Deleted;
                }
            }
        }

        foreach (var path in paths)
        {
            foreach (var position in path.PathPositionList)
            {
                Map[position.X, position.Y].Position = position;
                Map[position.X, position.Y].Type = TileType.Path;
                Map[position.X, position.Y].Id = path.Id;
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
                    Map[column, row].Position = new Point(column, row);
                    Map[column, row].Type = TileType.Room;
                    Map[column, row].Id = room.Id;
                }
            }
        }
    }

    public void Create(TileData[,] map, List<Room> roomList, List<Path> paths)
    {
        this.Map = map;
        var roomTiles = map.ToArray().Where(tile => tile.IsRoom).ToArray();
        var spawnPointIndex = UnityEngine.Random.Range(0, roomTiles.Length);
        var stairPointIndex = UnityEngine.Random.Range(0, roomTiles.Length);
        
        stairPosition = roomTiles[stairPointIndex].Position;
        spawnPoint = roomTiles[spawnPointIndex].Position;
        rooms = roomList;
        this.paths = paths;
    }
}
