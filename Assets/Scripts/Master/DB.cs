using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xeon.Master
{
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
    }
}
