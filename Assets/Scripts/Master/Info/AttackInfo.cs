using System;
using UnityEngine;

namespace Xeon.Master
{
    [Serializable]
    public class AttackInfo
    {
        [SerializeField, CsvColumn("id")]
        private int id;
        [SerializeField, CsvColumn("groupId")]
        private int groupId;
        [SerializeField, CsvColumn("offset")]
        private Vector2Int offset;
        [SerializeField, CsvColumn("rate")]
        private uint rate;

        public int Id => id;
        public int GroupId => groupId;
        public Vector2Int Offset => offset;
        public uint Rate => rate;

        public AttackInfo() { }

        public AttackInfo Clone()
        {
            return new AttackInfo()
            {
                groupId = groupId,
                offset = offset,
                rate = rate
            };
        }

#if DEBUG
        public AttackInfo(int groupId, Vector2Int offset, uint rate)
        {
            this.groupId = groupId;
            this.offset = offset;
            this.rate = rate;
        }
#endif
    }
}
