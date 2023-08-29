using System;
using UnityEngine;

namespace Xeon.Master
{
    [Serializable]
    public class PassiveEffectInfo
    {
        [SerializeField, CsvColumn("Id")]
        private int id;
        [SerializeField, CsvColumn("EffectType")]
        private PassiveEffectType effectType;
        [SerializeField, CsvColumn("Param1")]
        private int param1 = 0;
        [SerializeField, CsvColumn("Param2")]
        private int param2 = 0;

        public int Id => id;
        public PassiveEffectType EffectType => effectType;
        public int Param1 => param1;
        public int Param2 => param2;

        public PassiveEffectInfo Clone()
        {
            return new PassiveEffectInfo()
            {
                id = id,
                effectType = effectType,
                param1 = param1,
                param2 = param2
            };
        }
    }
}
