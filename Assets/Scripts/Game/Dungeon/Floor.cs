using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Floor : MonoBehaviour
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

    public List<TileData> GetRoomTiles(int roomId) => Map.ToArray().Where(tile => tile.IsRoom && tile.Id == roomId).ToList();
    public TileData GetTile(int x, int y) => Map[x, y];
    public TileData GetTile(Vector2Int position) => GetTile(position.x, position.y);

    public void CreateAsync(int width, int height, int maxRoom, bool isTower)
    {
        Size = new Vector2Int(width, height);
        FloorData = DungeonGenerator.GenerateFloor(width, height, maxRoom);
        this.isTower = isTower;
        StartCoroutine(Create());
    }

    public void Create(int width, int height, int maxRoom, bool isTower)
    {
        Size = new Vector2Int(width, height);
        FloorData = DungeonGenerator.GenerateFloor(width, height, maxRoom);
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
        return rootFinder.Execute();
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
