using System;
using UnityEngine;

[Serializable]
public class EnemyInfo
{
    [SerializeField, CsvColumn("id")]
    private int id;
    [SerializeField, CsvColumn("name")]
    private string name;
    [SerializeField, CsvColumn("hp")]
    private float hp = 15f;
    [SerializeField, CsvColumn("atk")]
    private int atk = 10;
    [SerializeField, CsvColumn("def")]
    private int def = 0;
    [SerializeField]
    private Enemy prefab;
    
    public int Id => id;
    public string Name => name;
    public float Hp => hp;
    public int Atk => atk;
    public int Def => def;
    public Enemy Prefab => prefab;

    public EnemyInfo()
    {
    }

    public EnemyInfo Clone()
    {
        return new EnemyInfo()
        {
            id = id,
            name = name,
            hp = hp,
            atk = atk,
            def = def,
        };
    }

#if UNITY_EDITOR
    public void SetName(string name) => this.name = name;
    public void SetHp(float hp) => this.hp = hp;
    public void SetAtk(int atk) => this.atk = atk;
    public void SetDef(int def) => this.def = def;
#endif
}
