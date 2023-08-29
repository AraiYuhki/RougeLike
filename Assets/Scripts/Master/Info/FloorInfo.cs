using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Xeon.Master
{
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
        [SerializeField, CsvColumn("floorMaterial"), AddressableMaterial]
        private string floorMaterialName;
        [SerializeField, CsvColumn("wallMaterial"), AddressableMaterial]
        private string wallMaterialName;
        [SerializeField, CsvColumn("enemySpawnGroupId")]
        private int enemySpawnGroupId;

        public int Id => id;
        public int DungeonId => dungeonId;
        public int SameSettingCount => sameSettingCount;
        public Vector2Int Size => size;
        public int MaxRoomCount => maxRoomCount;
        public float DeletePathProbability => deletePathProbability;
        public string FloorMaterialName => floorMaterialName;
        public string WallMaterialName => wallMaterialName;
        public int EnemySpawnGroupId => enemySpawnGroupId;

        public Material FloorMaterial
            => Addressables.LoadAssetAsync<Material>(FloorMaterial).WaitForCompletion();
        public Material WallMaterial
            => Addressables.LoadAssetAsync<Material>(WallMaterial).WaitForCompletion();

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
}
