using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class FloorInfo
{
    [SerializeField, CsvColumn("id")]
    private int id;
    [SerializeField, CsvColumn("dungeonId")]
    private int dungeonId;
    [SerializeField, CsvColumn("sameSettingCount")]
    private int sameSettingCount;
    [SerializeField, CsvColumn("size")]
    private Vector2Int size;
    [SerializeField, CsvColumn("maxRoomCount")]
    private int maxRoomCount = 3;
    [SerializeField, Range(0f, 1f), CsvColumn("deletePathProbability")]
    private float deletePathProbability;
    [SerializeField, Range(0, 99), CsvColumn("initialSpawnEnemyCount")]
    private int initialSpawnEnemyCount = 4;
    [SerializeField, Range(5, 9999), CsvColumn("spawnEnemyIntervalTurn")]
    private int spawnEnemyIntervalTurn = 30;
    [SerializeField, CsvColumn("floorMaterial")]
    private string floorMaterialName;
    [SerializeField, CsvColumn("wallMaterial")]
    private string wallMaterialName;
    [SerializeField, CsvColumn("enemySpawnGroupId")]
    private int enemySpawnGroupId;
    [SerializeField, CsvColumn("shopId")]
    private int shopId;

    public int Id => id;
    public int DungeonId => dungeonId;
    public int SameSettingCount => sameSettingCount;
    public Vector2Int Size => size;
    public int MaxRoomCount => maxRoomCount;
    public float DeletePathProbability => deletePathProbability;
    public string FloorMaterialName => floorMaterialName;
    public string WallMaterialName => wallMaterialName;
    public int EnemySpawnGroupId => enemySpawnGroupId;
    public int InitialSpawnEnemyCount => initialSpawnEnemyCount;
    public int SpawnEnemyIntervalTurn => spawnEnemyIntervalTurn;
    public int ShopId => shopId;

    public Material FloorMaterial
        => Addressables.LoadAssetAsync<Material>(floorMaterialName).WaitForCompletion();
    public Material WallMaterial
        => Addressables.LoadAssetAsync<Material>(wallMaterialName).WaitForCompletion();
    public List<int> Enemies
        => DB.Instance.MFloorEnemySpawn.GetByGroupId(EnemySpawnGroupId).Select(info => info.EnemyId).ToList();

    public FloorInfo Clone()
    {
        return new FloorInfo()
        {
            dungeonId = dungeonId,
            sameSettingCount = sameSettingCount,
            size = size,
            maxRoomCount = maxRoomCount,
            deletePathProbability = deletePathProbability,
            floorMaterialName = floorMaterialName,
            wallMaterialName = wallMaterialName,
            enemySpawnGroupId = enemySpawnGroupId,
        };
    }

}
