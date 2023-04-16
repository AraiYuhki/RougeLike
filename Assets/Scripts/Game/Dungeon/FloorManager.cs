using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UIElements;

public class FloorManager : MonoBehaviour
{
    [SerializeField]
    private GameObject floor;
    [SerializeField]
    private GameObject wall;
    [SerializeField]
    private GameObject upStair;
    [SerializeField]
    private GameObject downStair;

    private AStar.Calculator rootFinder = null;
    private bool isTower;
    public FloorData FloorData { get; private set; }
    public Vector2Int Size { get; private set; } = new Vector2Int(20, 20);
    public TileData[,] Map => FloorData.Map;
    public int[] RoomIds => FloorData.Map.Cast<TileData>().Where(tile => tile.IsRoom).Select(tile => tile.Id).Distinct().ToArray();

    private Unit[,] units;
    private Item[,] items;

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
    public int GetUnitCount() => units.ToArray().Where(unit => unit != null).Count();

    public Item GetItem(int x, int y) => items[x, y];
    public Item GetItem(Vector2Int position) => items[position.x, position.y];
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
        var tiles = GetAroundTilesAt(candidate).Where(tile => CanDrop(tile.Position)).ToArray();
        return tiles.Any() ? tiles.Random() : null;
    }

    public List<TileData> GetRoomTiles() => Map.ToArray().Where(tile => tile.IsRoom).ToList();
    public List<TileData> GetRoomTiles(int roomId) => Map.ToArray().Where(tile => tile.IsRoom && tile.Id == roomId).ToList();
    public List<TileData> GetRoomTilesExcludeRoomId(int excludeRoomId) => Map.ToArray().Where(tile => tile.IsRoom && tile.Id != excludeRoomId).ToList();
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

    public void CreateAsync(int width, int height, int maxRoom, bool isTower)
    {
        Size = new Vector2Int(width, height);
        FloorData = DungeonGenerator.GenerateFloor(width, height, maxRoom);
        this.isTower = isTower;
        units = new Unit[Size.x, Size.y];
        StartCoroutine(Create());
    }

    public void Create(int width, int height, int maxRoom, bool isTower)
    {
        Size = new Vector2Int(width, height);
        FloorData = DungeonGenerator.GenerateFloor(width, height, maxRoom);
        units = new Unit[Size.x, Size.y];
        items = new Item[Size.x, Size.y];
        this.isTower = isTower;
        for (var x = 0; x < Size.x; x++)
        {
            for (var z = 0; z < Size.y; z++)
            {
                CreateVoxel(Map[x, z], x, z);
            }
        }
        transform.parent.position = new Vector3(-width * 0.5f, 0f, -height * 0.5f);
        rootFinder = new AStar.Calculator(Map);
    }

    public List<Vector2Int> GetRoot(Vector2Int startPosition, Vector2Int targetPosition)
    {
        rootFinder.StartPoint = startPosition;
        rootFinder.EndPoint = targetPosition;
        rootFinder.Clear();
        return rootFinder.Execute();
    }

    public void OnMoveUnit(Unit unit, Vector2Int destPos)
    {
        RemoveUnit(unit);
        SetUnit(unit, destPos);
    }

    public void SetUnit(Unit unit, Vector2Int position) => units[position.x, position.y] = unit;
    public void RemoveUnit(Vector2Int position) => units[position.x, position.y] = null;
    public void RemoveUnit(Unit unit)
    {
        for (var x = 0; x < Size.x; x++)
        {
            for (var y = 0; y < Size.y; y++)
            {
                if (units[x, y] == unit)
                {
                    units[x, y] = null;
                    return;
                }
            }
        }
    }

    public void SetItem(Item item, Vector2Int position) => items[position.x, position.y] = item;
    public void RemoveItem(Vector2Int position) => items[position.x, position.y] = null;
    public void RemoveItem(Item item)
    {
        for (var x = 0; x < Size.x; x++)
        {
            for (var y = 0; y < Size.y; y++)
            {
                if (items[x, y] == item)
                {
                    items[x, y] = null;
                    return;
                }
            }
        }
    }

    private IEnumerator Create()
    {
        var count = 0;
        for (var x = 0; x < Size.x; x++)
        {
            for (var z = 0; z < Size.y; z++)
            {
                CreateVoxel(Map[x, z], x, z);
                count++;
                if (count == 50)
                {
                    count = 0;
                    yield return null;
                }
            }
        }
        transform.parent.position = new Vector3(-Size.x * 0.5f, 0f, -Size.y * 0.5f);
    }

    public void Clear()
    {
        while (transform.childCount > 0) DestroyImmediate(transform.GetChild(0).gameObject);
    }

    private void CreateVoxel(TileData tileInfo, int x, int z)
    {
        GameObject obj;
        GameObject voxel;
        var y = 0f;
        if (FloorData.IsStair(x, z))
        {
            obj = isTower ? upStair : downStair;
            y = isTower ? 0.5f : -0.5f;
            voxel = Instantiate(obj, transform);
        }
        else
        {
            obj = tileInfo.IsWall ? wall : floor;
            y = tileInfo.IsWall ? 0.5f : -0.5f;
            voxel = Instantiate(obj, transform);
        }
        voxel.transform.localPosition = new Vector3(x, y, z);
        voxel.name = $"Voxel({x},{z})";
    }
}
