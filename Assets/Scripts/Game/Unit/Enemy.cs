using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
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

    public bool HasAilment(AilmentType type) => Data.Ailments.ContainsKey(type);
    public bool HasAnyAilment => Data.Ailments.Count > 0;

    public override void AddAilment(AilmentType type, int param, int turn)
    {
        data.AddAilment(type, param, turn);
        notice.Add($"{Name}は{type.ToLabel()}状態になった", Color.magenta);
    }

    protected override void ExecuteAilments()
    {
        if (data.Ailments.ContainsKey(AilmentType.Poison))
            Damage(data.Ailments[AilmentType.Poison].Param);
        data.DecrementAilmentTurns();
    }

    /// <summary>
    /// 直線上のプレイヤーへ弾を飛ばしてダメージを与える
    /// </summary>
    public async UniTask ShootAsync(Player target, int damage, CancellationToken token)
    {
        var diff = target.Position - Position;
        var distance = Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y), 1);
        var bullet = gameController.CreateBullet(transform.localPosition, transform.rotation);
        await bullet.transform
            .DOLocalMove(new Vector3(target.Position.x, 0.5f, target.Position.y), 0.1f * distance)
            .SetEase(Ease.Linear)
            .ToUniTask(cancellationToken: token);
        Destroy(bullet);
        OnAttack?.Invoke(this, target);
        target.Damage(damage, this);
    }

    /// <summary>
    /// プレイヤーへ状態異常を投げつける
    /// </summary>
    public async UniTask ThrowAilmentAsync(Player target, AilmentType type, int param, int turn, CancellationToken token)
    {
        var diff = target.Position - Position;
        var distance = Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y), 1);
        var bullet = gameController.CreateBullet(transform.localPosition, transform.rotation);
        await bullet.transform
            .DOLocalJump(new Vector3(target.Position.x, 0.5f, target.Position.y), 1f, 1, 0.15f * distance)
            .SetEase(Ease.Linear)
            .ToUniTask(cancellationToken: token);
        Destroy(bullet);
        target.AddAilment(type, param, turn);
    }

    /// <summary>
    /// プレイヤーからジェムを盗む
    /// </summary>
    public void StealGems(Player target, int amount)
    {
        var stolen = Mathf.Min(target.Data.Gems, amount);
        if (stolen <= 0) return;
        target.Data.Gems -= stolen;
        Data.StolenGems += stolen;
        notice.Add($"{Name}は{target.Name}からジェムを{stolen}個盗んだ!", Color.magenta);
    }

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
