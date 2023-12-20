using System;
using UnityEngine;

[Serializable]
public class EnemyInfo : UnitData
{
    [SerializeField, CsvColumn("id")]
    private int id;
    [SerializeField, CsvColumn("name")]
    private string name;
    [SerializeField, CsvColumn("hp")]
    private int maxHp;
    [SerializeField, CsvColumn("def")]
    private int def = 0;

    public int Id => id;
    public string Name => name;
    public override int MaxHP => maxHp;
    public EnemyInfo(int hp) : base(hp)
    {
    }

    public EnemyInfo Clone()
    {
        return new EnemyInfo(MaxHP)
        {
            name = name,
            maxHp = maxHp,
            atk = atk,
            def = def
        };
    }
}
