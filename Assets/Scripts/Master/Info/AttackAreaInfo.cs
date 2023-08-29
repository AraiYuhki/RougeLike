using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xeon.Master
{
    [Serializable]
    public class AttackAreaInfo
    {
        [SerializeField, CsvColumn("id")]
        private int id;
        [SerializeField, CsvColumn("maxSize")]
        private int maxSize;
        [SerializeField, CsvColumn("center")]
        private Vector2Int center;
        [SerializeField, CsvColumn("attackGroupId")]
        private int attackGroupId;

        private List<AttackInfo> data;
        private List<AttackData>[] stripedData;

        private static int maxRadius = 10;
        private static Dictionary<int, List<(int x, int y)>> offsetMap;

        public int Id => id;
        public int MaxSize => MaxSize;
        public Vector2Int Center => center;
        public int AttackGroupId => attackGroupId;

    }
}
