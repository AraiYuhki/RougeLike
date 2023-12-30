using UnityEditor;
using UnityEngine.AddressableAssets;

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
    private MTrap trapData;
    private MDungeon dungeonData;
    private MFloor floorData;
    private MFloorEnemySpawn floorEnemySpawnData;
    private MFloorTrap floorTrapData;
    private MFloorShop floorShop;
    private MCard cardData;
    private MPassiveEffect passiveEffectData;
    private MAttack attackData;
    private MAttackArea attackAreaData;

    public MEnemy MEnemy => enemyData;
    public MTrap MTrap => trapData;
    public MDungeon MDungeon => dungeonData;
    public MFloor MFloor => floorData;
    public MFloorEnemySpawn MFloorEnemySpawn => floorEnemySpawnData;
    public MFloorTrap MFloorTrap => floorTrapData;
    public MFloorShop MFloorShop => floorShop;
    public MCard MCard => cardData;
    public MPassiveEffect MPassiveEffect => passiveEffectData;
    public MAttack MAttack => attackData;
    public MAttackArea MAttackArea => attackAreaData;

    private DB()
    {
        Reload();
    }

    public void Reload()
    {
        enemyData = Addressables.LoadAssetAsync<MEnemy>("MEnemy").WaitForCompletion();
        trapData = Addressables.LoadAssetAsync<MTrap>("MTrap").WaitForCompletion();
        passiveEffectData = Addressables.LoadAssetAsync<MPassiveEffect>("MPassiveEffect").WaitForCompletion();
        cardData = Addressables.LoadAssetAsync<MCard>("MCard").WaitForCompletion();
        attackData = Addressables.LoadAssetAsync<MAttack>("MAttack").WaitForCompletion();
        attackAreaData = Addressables.LoadAssetAsync<MAttackArea>("MAttackArea").WaitForCompletion();
        floorShop = Addressables.LoadAssetAsync<MFloorShop>("MFloorShop").WaitForCompletion();
        floorEnemySpawnData = Addressables.LoadAssetAsync<MFloorEnemySpawn>("MFloorEnemySpawn").WaitForCompletion();
        floorTrapData = Addressables.LoadAssetAsync<MFloorTrap>("MFloorTrap").WaitForCompletion();
        floorData = Addressables.LoadAssetAsync<MFloor>("MFloor").WaitForCompletion();
        dungeonData = Addressables.LoadAssetAsync<MDungeon>("MDungeon").WaitForCompletion();
    }

    ~DB()
    {
        SafeRelease(enemyData);
        SafeRelease(trapData);
        SafeRelease(passiveEffectData);
        SafeRelease(cardData);
        SafeRelease(attackData);
        SafeRelease(attackAreaData);
        SafeRelease(floorShop);
        SafeRelease(floorEnemySpawnData);
        SafeRelease(floorTrapData);
        SafeRelease(floorData);
        SafeRelease(dungeonData);
    }

    private void SafeRelease(object target)
    {
        if (target == null) return;
        Addressables.Release(target);
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Master/DB再読み込み")]
    public static void ReloadDB()
    {
        instance?.Reload();
    }
#endif
}
