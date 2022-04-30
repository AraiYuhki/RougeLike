using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Unit
{
    public EnemyData Data { get; private set; }
    public bool IsEncounted { get; set; }
    public TileData TargetTile { get; set; }
    public int TargetRoomId => TargetTile.Id;

    public void Initialize(int lv, int hp, int atk, int def, int exp)
    {
        Data = new EnemyData(hp)
        {
            Lv = lv,
            Atk = atk,
            Def = def,
            Exp = exp
        };
    }
}
