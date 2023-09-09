using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DB
{
    private static DB instance = null;
    public static DB Instance
    {
        get
        {
            if (instance == null)
                instance = new DB();
            return instance;
        }
    }

    private MEnemy enemyData;
    private MDungeon dungeonData;
    private MFloor floorData;
    private MFloorEnemySpawn floorEnemySpawnData;
    private MCard cardData;
    private MPassiveEffect passiveEffectData;
    private MAttack attackData;
    private MAttackArea attackAreaData;

    public MEnemy MEnemy => enemyData;
    public MDungeon MDungeon => dungeonData;
    public MFloor MFloor => floorData;
    public MFloorEnemySpawn MFloorEnemySpawn => floorEnemySpawnData;
    public MCard MCard => cardData;
    public MPassiveEffect MPassiveEffect => passiveEffectData;
    public MAttack MAttack => attackData;
    public MAttackArea MAttackArea => attackAreaData;

    private DB()
    {
        enemyData = Addressables.LoadAssetAsync<MEnemy>("MEnemy").WaitForCompletion();
        passiveEffectData = Addressables.LoadAssetAsync<MPassiveEffect>("MPassiveEffect").WaitForCompletion();
        cardData = Addressables.LoadAssetAsync<MCard>("MCard").WaitForCompletion();
        attackData = Addressables.LoadAssetAsync<MAttack>("MAttack").WaitForCompletion();
        attackAreaData = Addressables.LoadAssetAsync<MAttackArea>("MAttackArea").WaitForCompletion();
        floorEnemySpawnData = Addressables.LoadAssetAsync<MFloorEnemySpawn>("MFloorEnemySpawn").WaitForCompletion();
        floorData = Addressables.LoadAssetAsync<MFloor>("MFloor").WaitForCompletion();
        dungeonData = Addressables.LoadAssetAsync<MDungeon>("MDungeon").WaitForCompletion();
    }

    ~DB()
    {
        SafeRelease(enemyData);
        SafeRelease(passiveEffectData);
        SafeRelease(cardData);
        SafeRelease(attackData);
        SafeRelease(attackAreaData);
        SafeRelease(floorEnemySpawnData);
        SafeRelease(floorData);
        SafeRelease(dungeonData);
    }

    private void SafeRelease(object target)
    {
        if (target == null) return;
        Addressables.Release(target);
    }

#if UNITY_EDITOR
    [MenuItem("Debug/Import")]
    public static void Import()
    {
        var newEnemyData = CsvParser.Parse<EnemyInfo>(System.IO.Path.Combine(Application.streamingAssetsPath, "MEnemy.tsv"));
        var newPassiveEffectData = CsvParser.Parse<PassiveEffectInfo>(System.IO.Path.Combine(Application.streamingAssetsPath, "MPassiveEffect.tsv"));
        var newCardData = CsvParser.Parse<CardInfo>(System.IO.Path.Combine(Application.streamingAssetsPath, "MCard.tsv"));
        var newAttackData = CsvParser.Parse<AttackInfo>(System.IO.Path.Combine(Application.streamingAssetsPath, "MAttack.tsv"));
        var newAttackAreaData = CsvParser.Parse<AttackAreaInfo>(System.IO.Path.Combine(Application.streamingAssetsPath, "MAttackArea.tsv"));
        var newFloorEnemySpawnData = CsvParser.Parse<FloorEnemySpawnInfo>(System.IO.Path.Combine(Application.streamingAssetsPath, "MFloorEnemySpawn.tsv"));
        var newFloorData = CsvParser.Parse<FloorInfo>(System.IO.Path.Combine(Application.streamingAssetsPath, "MFloor.tsv"));
        var newDungeonData = CsvParser.Parse<DungeonInfo>(System.IO.Path.Combine(Application.streamingAssetsPath, "MDungeon.tsv"));

    }
#endif
}
