using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FloorInfo
{
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
    [SerializeField]
    private int installTrapMinCount = 3;
    [SerializeField]
    private int installTrapMaxCount = 3;
    [SerializeField]
    private Material floorMaterial;
    [SerializeField]
    private Material wallMaterial;
    [SerializeField, CsvColumn("enemySpawnGroupId")]
    private int enemySpawnGroupId;
    [SerializeField, CsvColumn("shopId")]
    private int shopId;
    [SerializeField]
    private int trapSettingGroupId;

    public int DungeonId => dungeonId;
    public int SameSettingCount => sameSettingCount;
    public Vector2Int Size => size;
    public int MaxRoomCount => maxRoomCount;
    public float DeletePathProbability => deletePathProbability;
    public int EnemySpawnGroupId => enemySpawnGroupId;
    public int TrapSettingGroupId => trapSettingGroupId;
    public int InitialSpawnEnemyCount => initialSpawnEnemyCount;
    public int SpawnEnemyIntervalTurn => spawnEnemyIntervalTurn;
    public int ShopId => shopId;
    public int InstallTrapMinNum => installTrapMinCount;
    public int InstallTrapMaxNum => installTrapMaxCount;

    public Material FloorMaterial => floorMaterial;
    public Material WallMaterial => wallMaterial;
    public List<int> Enemies
        => DB.Instance.MFloorEnemySpawn.GetByGroupId(EnemySpawnGroupId).Select(info => info.EnemyId).ToList();

    public FloorInfo() { }

    public FloorInfo Clone()
    {
        return new FloorInfo()
        {
            dungeonId = dungeonId,
            sameSettingCount = sameSettingCount,
            size = size,
            maxRoomCount = maxRoomCount,
            wallMaterial = wallMaterial,
            floorMaterial = floorMaterial,
            deletePathProbability = deletePathProbability,
            initialSpawnEnemyCount = initialSpawnEnemyCount,
            spawnEnemyIntervalTurn = spawnEnemyIntervalTurn,
            installTrapMaxCount = installTrapMaxCount,
            enemySpawnGroupId = enemySpawnGroupId,
            trapSettingGroupId = trapSettingGroupId
        };
    }

#if UNITY_EDITOR
    public FloorInfo(int dungeonId)
    {
        this.dungeonId = dungeonId;
    }
    public void SetSameSettingCount(int count) => sameSettingCount = Mathf.Max(count, 0);
    public void SetSize(Vector2Int size)
    {
        this.size = size;
        this.size.x = Mathf.Max(size.x, 10);
        this.size.y = Mathf.Max(size.y, 10);
    }
    public void SetMaxRoomCount(int count) => maxRoomCount = count;
    public void SetDeletePathProbability(float probability) => deletePathProbability = probability;
    public void SetInitialSpawnEnemyCount(int count) => initialSpawnEnemyCount = count;
    public void SetSpawnEnemyIntervalTurn(int turn) => spawnEnemyIntervalTurn = turn;
    public void SetInstallTrapMinCount(int count) => installTrapMinCount = count;
    public void SetInstallTrapMaxCount(int count) => installTrapMaxCount = count;
    public void SetEnemySpawnGroupId(int groupId) => enemySpawnGroupId = groupId;
    public void SetTrapSettingGroupId(int groupId) => trapSettingGroupId = groupId;
    public void SetFloorMaterial(Material material) => floorMaterial = material;
    public void SetWallMaterial(Material material) => wallMaterial = material;
#endif

}
