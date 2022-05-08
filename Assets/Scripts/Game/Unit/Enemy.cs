using DG.Tweening;
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

    public override void SetDestAngle(Vector2Int move)
    {
        EndRotation = false;
        var destAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(move.x, 0f, move.y), Vector3.up);
        transform.DORotate(new Vector3(0f, destAngle, 0f), 0.1f).SetEase(Ease.OutCubic).OnComplete(() => EndRotation = true);
    }
}
