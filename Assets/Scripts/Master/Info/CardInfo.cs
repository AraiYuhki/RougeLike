using System;
using UnityEngine;

namespace Xeon.Master
{
    [Serializable]
    public class CardInfo
    {
        [SerializeField, CsvColumn("id")]
        private int id;
        [SerializeField, CsvColumn("name")]
        private string name;
        [SerializeField, CsvColumn("type")]
        private CardType type;
        [SerializeField, CsvColumn("param")]
        private float param;
        [SerializeField, CsvColumn("range")]
        private int range;
        [SerializeField, CsvColumn("price")]
        private int price;
        [SerializeField, CsvColumn("attackAreaId")]
        private int attackAreaDataId = -1;
        [SerializeField, CsvColumn("passiveEffectId")]
        private int passiveEffectId = -1;

        public int Id => id;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public virtual CardType Type
        {
            get => type;
            set => type = value;
        }

        public float Param
        {
            get => param;
            set => param = value;
        }

        public int Range
        {
            get => range;
            set => range = value;
        }

        public int Price
        {
            get => price;
            set => price = value;
        }

        public int AttackAreaDataId
        {
            get => attackAreaDataId;
            set => attackAreaDataId = value;
        }

        public bool IsPassive => passiveEffectId >= 0;
        public int PassiveEffectId
        {
            get => passiveEffectId;
            set => passiveEffectId = value;
        }

        public virtual CardData Clone()
        {
            return new CardData
            {
                Name = Name,
                Type = Type,
                Param = Param,
                Range = Range,
                Price = Price,
                AttackAreaDataId = AttackAreaDataId,
                PassiveEffectId = PassiveEffectId,
            };
        }

        public virtual bool CanUse(Unit user)
        {
            return true;
        }
    }
}