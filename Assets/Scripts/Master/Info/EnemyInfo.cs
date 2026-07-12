using System;
using UnityEngine;

/// <summary>
/// 敵の行動パターン
/// </summary>
public enum EnemyAIType
{
    Default,        // 通常(プレイヤーに接近して近接攻撃)
    Ranged,         // 遠距離攻撃(直線上のプレイヤーを射撃 param1:射程)
    AilmentThrower, // 状態異常投擲(同じ部屋のプレイヤーに毒を投げる param1:毒ダメージ param2:継続ターン)
    Thief,          // 盗み(隣接時にジェムを盗んで逃走 param1:盗む量)
    Split,          // 分裂(ダメージを受けると一定確率で分裂 param1:分裂確率%)
    DoubleSpeed,    // 倍速(1ターンに2回行動)
    Sleeper,        // 居眠り(プレイヤーが同じ部屋に入るか隣接するまで動かない)
}

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
    [SerializeField, CsvColumn("aiType")]
    private EnemyAIType aiType = EnemyAIType.Default;
    [SerializeField, CsvColumn("aiParam1")]
    private int aiParam1 = 0;
    [SerializeField, CsvColumn("aiParam2")]
    private int aiParam2 = 0;
    [SerializeField]
    private Enemy prefab;

    public int Id => id;
    public string Name => name;
    public float Hp => hp;
    public int Atk => atk;
    public int Def => def;
    public EnemyAIType AIType => aiType;
    public int AIParam1 => aiParam1;
    public int AIParam2 => aiParam2;
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
            aiType = aiType,
            aiParam1 = aiParam1,
            aiParam2 = aiParam2,
        };
    }

#if UNITY_EDITOR
    public void SetName(string name) => this.name = name;
    public void SetHp(float hp) => this.hp = hp;
    public void SetAtk(int atk) => this.atk = atk;
    public void SetDef(int def) => this.def = def;
    public void SetAIType(EnemyAIType aiType) => this.aiType = aiType;
    public void SetAIParam1(int param) => aiParam1 = param;
    public void SetAIParam2(int param) => aiParam2 = param;
#endif
}
