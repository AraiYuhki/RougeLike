using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class FloorManager : MonoBehaviour, IUnitContainer
{
    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private GameObject wall;
    [SerializeField]
    private GameObject upStair;
    [SerializeField]
    private GameObject downStair;

    private Dijkstra dijkstra = null;
    private AStar aStar = null;
    private Minimap minimap;

    public bool IsTower { get; private set; }
    public FloorData FloorData { get; private set; }
    public FloorInfo FloorInfo { get; private set; }
    public Vector2Int Size { get; private set; } = new Vector2Int(20, 20);
    public TileData[,] Map => FloorData.Map;
    public int[] RoomIds => FloorData.Map.Cast<TileData>().Where(tile => tile.IsRoom).Select(tile => tile.Id).Distinct().ToArray();

    private Unit[,] units;
    private TrapData[,] traps;
    private ItemData[,] items;

    public void SetMinimap(Minimap minimap)
    {
        this.minimap = minimap;
        if (FloorData != null) minimap.Initialize(FloorData);
    }

    public bool CanDrop(int x, int y)
    {
        var tile = GetTile(x, y);
        if (tile.IsWall)
            return false;
        if (GetItem(x, y) != null)
            return false;
        return true;
    }

    public bool CanDrop(Vector2Int position) => CanDrop(position.x, position.y);

    public Unit GetUnit(int x, int y)
    {
        if (x < 0 || x >= units.GetLength(0) || y < 0 || y >= units.GetLength(1))
            return null;
        return units[x, y];
    }
    public Unit GetUnit(Vector2Int position) => GetUnit(position.x, position.y);
    public bool ExistsUnit(Vector2Int position) => GetUnit(position.x, position.y) != null;
    public int GetUnitCount() => units.ToArray().Where(unit => unit != null).Count();

    public TrapData GetTrap(int x, int y) => traps[x, y];
    public TrapData GetTrap(Vector2Int position) => traps[position.x, position.y];

    public ItemData GetItem(int x, int y) => items[x, y];
    public ItemData GetItem(Vector2Int position) => items[position.x, position.y];
    public int GetItemCount() => items.ToArray().Where(unit => unit != null).Count();

    public List<TileData> GetAroundTilesAt(Vector2Int position)
    {
        var candidate = new List<TileData>() {
            GetTile(position + new Vector2Int(-1, 1)),
            GetTile(position + new Vector2Int(0, 1)),
            GetTile(position + new Vector2Int(1, 1)),
            GetTile(position + new Vector2Int(1, 0)),
            GetTile(position + new Vector2Int(-1, 0)),
            GetTile(position + new Vector2Int(1, -1)),
            GetTile(position + new Vector2Int(0, -1)),
            GetTile(position + new Vector2Int(-1, -1)),
        };
        return candidate.Where(tile => tile != null).ToList();
    }

    public TileData GetCanDropTile(Vector2Int candidate)
    {
        if (CanDrop(candidate))
            return GetTile(candidate);
        var tiles = GetAroundTilesAt(candidate).Where(tile => CanDrop(tile.Position)).ToArray();
        return tiles.Any() ? tiles.Random() : null;
    }

    public List<TileData> GetRoomTiles() => Map.ToArray().Where(tile => tile.IsRoom).ToList();
    public List<TileData> GetRoomTiles(int roomId) => Map.ToArray().Where(tile => tile.IsRoom && tile.Id == roomId).ToList();
    public List<TileData> GetRoomTilesExcludeRoomId(int excludeRoomId) => Map.ToArray().Where(tile => tile.IsRoom && tile.Id != excludeRoomId).ToList();
    public List<TileData> GetEmptyRoomTiles()
        => Map.ToArray()
        .Where(tile => tile.IsRoom && GetUnit(tile.Position) == null)
        .ToList();
    public List<TileData> GetEmptyRoomTiles(int excludeRoomId)
        => Map.ToArray()
        .Where(tile => tile.IsRoom && tile.Id != excludeRoomId && GetUnit(tile.Position) == null)
        .ToList();

    public TileData GetTile(int x, int y)
    {
        if (x < 0 || x >= Map.GetLength(0)
        ||  y < 0 || y >= Map.GetLength(1))
            return null;
        return Map[x, y];
    }
    public TileData GetTile(Vector2Int position) => GetTile(position.x, position.y);

    public (int length, Vector2Int position, Enemy enemy) GetHitPosition(Vector2Int startPosition, Vector2Int vector, int length = 10)
    {
        for (var count = 1; count < length; count++)
        {
            var unit = GetUnit(startPosition + vector * count);
            if (unit is Enemy enemy) return (count, unit.Position, enemy);
            var tile = GetTile(startPosition + vector * count);
            if (tile.IsWall)
                return (count - 1, GetTile(startPosition + vector * (count - 1)).Position, null);
        }
        return (length, startPosition + vector * length, null);
    }

    public FloorInfo Create(DungeonInfo dungeonData, int currentFloor)
    {
        var floorInfo = dungeonData.GetFloor(currentFloor);
        wall.GetComponent<Renderer>().sharedMaterial = floorInfo.WallMaterial;
        floor.GetComponent<Renderer>().sharedMaterial = floorInfo.FloorMaterial;
        Clear();
        FloorInfo = floorInfo;

        Initialize(floorInfo.Size, floorInfo.MaxRoomCount, dungeonData.IsTower);

        return floorInfo;
    }

    public void CreateMesh()
    {
        CombineMesh();
        wall.transform.localPosition = Vector3.zero;
    }

    public void Create(int width, int height, int maxRoom, bool isTower)
    {
        Initialize(new Vector2Int(width, height), maxRoom, isTower);

        CombineMesh();

        wall.transform.localPosition = Vector3.zero;
    }

    private void Initialize(Vector2Int size, int maxRoom, bool isTower)
    {
        Size = size;
        FloorData = DungeonGenerator.GenerateFloor(Size.x, Size.y, maxRoom);
        units = new Unit[Size.x, Size.y];
        items = new ItemData[Size.x, Size.y];
        traps = new TrapData[Size.x, Size.y];
        IsTower = isTower;

        aStar = new AStar(FloorData, this);
        dijkstra = new Dijkstra(FloorData);
        minimap?.Initialize(FloorData);
    }

    private void CombineMesh()
    {
        var transform = wall.transform;
        var wallMesh = wall.GetComponent<MeshFilter>().sharedMesh;
        var floorMesh = floor.GetComponent<MeshFilter>().sharedMesh;

        var combinedWall = new List<CombineInstance>();
        var combinedFloor = new List<CombineInstance>();

        for (var x = 0; x < Size.x; x++)
        {
            for (var z = 0; z < Size.y; z++)
            {
                if (FloorData.IsStair(x, z))
                {
                    CreateStair(x, z);
                    continue;
                }
                var combine = new CombineInstance();
                if (traps[x, z] != null && traps[x, z].Type == TrapType.Pitfall) continue;
                if (Map[x, z].IsWall)
                {
                    transform.localPosition = new Vector3(x, 0.5f, z);
                    combine.transform = transform.localToWorldMatrix;
                    combine.mesh = wallMesh;
                    combinedWall.Add(combine);
                    continue;
                }
                transform.localPosition = new Vector3(x, -0.5f, z);
                combine.transform = transform.localToWorldMatrix;
                combine.mesh = floorMesh;
                combinedFloor.Add(combine);
            }
        }
        CreateCombinedMesh(combinedWall, wall.GetComponent<MeshRenderer>().sharedMaterial, "walls");
        CreateCombinedMesh(combinedFloor, floor.GetComponent<MeshRenderer>().sharedMaterial, "floors");
        this.transform.parent.position = new Vector3(-Size.x * 0.5f, 0f, -Size.y * 0.5f);
    }

    public List<Vector2Int> GetRoot(Vector2Int startPosition, Vector2Int targetPosition)
    {
        return aStar.FindRoot(startPosition, targetPosition);
    }

    public List<int> GetRootRooms(Vector2Int startPosition, Vector2Int targetPosition)
    {
        var startTile = GetTile(startPosition);
        var targetTile = GetTile(targetPosition);
        if (startTile == null || targetTile == null) return null;
        if (!startTile.IsRoom || !targetTile.IsRoom) return null;
        return dijkstra.GetRoot(startTile.Id, targetTile.Id);
    }

    public Room GetRoom(int roomId) => FloorData.Rooms.FirstOrDefault(room => room.Id == roomId);

    public List<Vector2Int> GetCheckpoints(Vector2Int start, Vector2Int end) => dijkstra.GetCheckpoints(start, end);

    public List<TileData> CreateCheckPointList(Vector2Int startPosition, Vector2Int targetPosition)
    {
        var startTile = GetTile(startPosition);
        var targetTile = GetTile(targetPosition);
        var result = new List<TileData>();

        var roomList = dijkstra.GetRoot(startTile.Id, targetTile.Id).Select(roomId => GetRoom(roomId)).ToList();

        foreach ((var room, var index) in roomList.Select((room, index) => (room, index)))
        {
            // 目的の部屋
            if (room == roomList.Last())
            {
                result.Add(targetTile);
                continue;
            }
            // 次の部屋に繋がる通路を取得
            var nextPath = room.ConnectedPathList[roomList[index + 1].Id];
            result.Add(GetTile(nextPath.From));
            result.Add(GetTile(nextPath.To));
        }

        return result;
    }

    public void OnMoveUnit(Unit unit, Vector2Int destPos)
    {
        RemoveUnit(unit.Position);
        SetUnit(unit, destPos);
    }

    public void SetUnit(Unit unit, Vector2Int position) => units[position.x, position.y] = unit;
    public void RemoveUnit(Vector2Int position) => units[position.x, position.y] = null;

    public void SetTrap(TrapData trap, Vector2Int position) => traps[position.x, position.y] = trap;
    public void RemoveTranp(Vector2Int position) => traps[position.x, position.y] = null;

    public void SetItem(ItemData item, Vector2Int position) => items[position.x, position.y] = item;
    public void RemoveItem(Vector2Int position) => items[position.x, position.y] = null;

    public void Clear()
    {
        while (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
    }

    private void CreateStair(int x, int z)
    {
        var obj = IsTower ? upStair : downStair;
        var voxel = Instantiate(obj, transform);
        voxel.transform.localPosition = new Vector3(x, IsTower ? 0.5f : -0.5f, z);
        voxel.name = $"Stair({x},{z})"; ;
    }

    private void CreateCombinedMesh(List<CombineInstance> combineData, Material material, string name)
    {
        var gameObject = new GameObject();
        gameObject.name = name;
        gameObject.transform.parent = transform;
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;

        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.sharedMesh.CombineMeshes(combineData.ToArray());

        gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
    }
}
