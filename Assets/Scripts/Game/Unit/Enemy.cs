using DG.Tweening;
using UnityEngine;

public class Enemy : Unit, IPositionable
{
    private EnemyData data;
    public EnemyData Data => data;
    public override int Hp { get => Mathf.FloorToInt(Data.Hp); set => Data.Hp = value; }
    public override int MaxHp { get => Data.MaxHP; }
    public override Vector2Int Position { get => data.Position; set => data.Position = value; }
    public override Vector2Int Angle { get => data.Angle; protected set => data.Angle = value; }
    public override string Name => Data.Name;
    public bool IsEncounted { get => data.IsEncouted; set => data.IsEncouted = value; }
    public override float ChargeStack { get => data.ChargeStack; protected set => data.ChargeStack = value; }
    public TileData TargetTile { get; set; }
    public int TargetRoomId => TargetTile.Id;

    public void Initialize(
        int masterId,
        Vector2Int position,
        GameController gameController,
        FloorManager floorManager,
        EnemyManager enemyManager,
        ItemManager itemManager,
        NoticeGroup notice,
        DamagePopupManager damagePopupManager)
    {
        data = new EnemyData(masterId);
        SetManagers(gameController, floorManager, enemyManager, itemManager, notice, damagePopupManager);
        SetPosition(position);
    }

    public override void SetDestAngle(Vector2Int move)
    {
        EndRotation = false;
        var destAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(move.x, 0f, move.y), Vector3.up);
        var tween = transform.DORotate(new Vector3(0f, destAngle, 0f), 0.1f).SetEase(Ease.OutCubic);
        tweenList.Add(tween);
        tween.OnComplete(() =>
        {
            tweenList.Remove(tween);
            EndRotation = true;
        });
    }
}
