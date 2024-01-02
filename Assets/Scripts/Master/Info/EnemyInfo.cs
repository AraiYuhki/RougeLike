using System;
using UnityEngine;

[Serializable]
public class EnemyInfo : UnitData
{
    [SerializeField, CsvColumn("id")]
    private int id;

    public int Id => id;
    public EnemyInfo(int hp) : base(hp)
    {
    }

    public EnemyInfo Clone()
    {
        return new EnemyInfo(MaxHP)
        {
            name = name,
            atk = atk,
        };
    }
}
