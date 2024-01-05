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

    public void Initialize(
        EnemyData original,
        Vector2Int position,
        GameController gameController,
        FloorManager floorManager,
        EnemyManager enemyManager,
        ItemManager itemManager,
        NoticeGroup notice,
        DamagePopupManager damagePopupManager)
    {
        data = original.Clone();
        SetManagers(gameController, floorManager, enemyManager, itemManager, notice, damagePopupManager);
        SetPosition(position);
    }
}
