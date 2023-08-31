using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xeon.Master
{
    public enum CardType
    {
        NormalAttack,       // 通常攻撃
        RangeAttack,        //　範囲攻撃
        LongRangeAttack,    // 遠距離攻撃
        RoomAttack,         // 部屋全体攻撃
        Heal,               // 回復
        StaminaHeal,        // スタミナ回復
        Charge,             // 攻撃力2倍(重複するごとに倍率が100％追加され、最大で4倍)
        ResourceAttack,     // これでとどめを刺すと必ずお金を落とす代わりに攻撃力が低め

        Passive,
    }

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

        private AttackAreaInfo attackAreaData;

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

        public AttackAreaInfo AttackAreaData
        {
            get
            {
                if (attackAreaData == null)
                    attackAreaData = DB.Instance.MAttackArea.GetById(attackAreaDataId);
                return attackAreaData;
            }
        }

        public bool IsPassive => passiveEffectId >= 0;
        public int PassiveEffectId
        {
            get => passiveEffectId;
            set => passiveEffectId = value;
        }

        public virtual CardInfo Clone()
        {
            return new CardInfo
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
