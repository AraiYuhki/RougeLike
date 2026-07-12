using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public abstract class EnemyAI
{
    protected int cantMoveTurns = 0;
    protected FloorManager floorInfo = null;
    protected Player player = null;
    public Enemy Enemy { get; private set; } = null;
    public EnemyAI(FloorManager floorInfo, Enemy enemy, Player player)
    {
        this.floorInfo = floorInfo;
        this.player = player;
        Enemy = enemy;
    }

    /// <summary>
    /// 1ターンに行動できる回数(倍速の敵は2)
    /// </summary>
    public virtual int ActionCount => 1;

    public virtual bool CanAttack()
    {
        var diff = player.Position - Enemy.Position;
        return Mathf.Abs(diff.x) <= 1 && Mathf.Abs(diff.y) <= 1;
    }

    public virtual UniTask MoveAsync(CancellationToken token)
    {
        return UniTask.CompletedTask;
    }

    public virtual UniTask AttackAsync(CancellationToken token)
    {
        return UniTask.CompletedTask;
    }
}

public class DefaultAI : EnemyAI
{
    protected List<Vector2Int> checkPoints = new List<Vector2Int>();
    protected List<int> rootRooms = new List<int>();
    public DefaultAI(FloorManager floorInfo, Enemy enemy, Player player) : base(floorInfo, enemy, player) { }

    public override async UniTask MoveAsync(CancellationToken token)
    {
        // 盲目中はプレイヤーを発見できない
        if (!Enemy.IsEncounted && !Enemy.HasAilment(AilmentType.Blind))
        {
            var playerTile = floorInfo.GetTile(player.Position);
            var currentTile = floorInfo.GetTile(Enemy.Position);
            if (playerTile.IsRoom && currentTile.IsRoom)
                Enemy.IsEncounted = playerTile.Id == currentTile.Id;
        }
        var root = FindRoot();
        // ルートが見つからないもしくは現在地点から動けない場合は何もしない
        if(root == null || root.Count < 2)
        {
            await base.MoveAsync(token);
            cantMoveTurns++;
            return;
        }
        // 次の移動先を探す(rootの0番目は現在地なのでスキップ)
        var nextTile = root.Skip(1).First();
        if (floorInfo.GetUnit(nextTile) != null)
        {
            // 移動できなかった
            await base.MoveAsync(token);
            cantMoveTurns++;
            return;
        }
        cantMoveTurns = 0;
        await Enemy.MoveToAsync(nextTile, token);
        await CheckTrapAsync(token);
    }

    public override async UniTask AttackAsync(CancellationToken token)
    {
        var diff = player.Position - Enemy.Position;
        await Enemy.RotateAsync(diff, token);
        await Enemy.AttackAsync(player, Enemy.Data.Atk);
    }

    protected virtual List<Vector2Int> FindRoot()
    {
        var currentTile = floorInfo.GetTile(Enemy.Position);
        // プレイヤーと接触していない
        if (!Enemy.IsEncounted)
        {
            if (Enemy.TargetTile == null || Enemy.TargetTile == currentTile || cantMoveTurns > 1)
            {
                var count = 0;
                while (count < 10)
                {
                    var targetRoomId = floorInfo.RoomIds.Random();
                    Enemy.TargetTile = floorInfo.GetRoomTiles(targetRoomId).Random();
                    checkPoints = floorInfo.GetCheckpoints(Enemy.Position, Enemy.TargetTile.Position);
                    if (targetRoomId != currentTile.Id && checkPoints.Count > 0) break;
                    count++;
                }
            }
            // 移動できない
            if (checkPoints.Count <= 0)
            {
                cantMoveTurns++;
                Debug.LogWarning("Can't move");
                return null;
            }
            // 今のチェックポイントに到達した
            if (Enemy.Position == checkPoints[0])
            {
                checkPoints.RemoveAt(0);
                return null;
            }
            return floorInfo.GetRoot(Enemy.Position, checkPoints.First());
        }
        return floorInfo.GetRoot(Enemy.Position, player.Position);
    }

    protected async UniTask CheckTrapAsync(CancellationToken token)
    {
        var trap = floorInfo.GetTrap(Enemy.Position);
        if (trap == null)
        {
            await UniTask.Yield();
            return;
        }
        await trap.ExecuteAsync(Enemy, token);
    }
}

/// <summary>
/// 倍速AI(1ターンに2回行動する)
/// </summary>
public class DoubleSpeedAI : DefaultAI
{
    public DoubleSpeedAI(FloorManager floorInfo, Enemy enemy, Player player) : base(floorInfo, enemy, player) { }

    public override int ActionCount => 2;
}

/// <summary>
/// 居眠りAI(プレイヤーが同じ部屋に入るか隣接するまで一切動かない)
/// </summary>
public class SleeperAI : DefaultAI
{
    public SleeperAI(FloorManager floorInfo, Enemy enemy, Player player) : base(floorInfo, enemy, player) { }

    public override async UniTask MoveAsync(CancellationToken token)
    {
        // 眠っている間は移動しない(発見済みになったら通常の追跡を行う)
        if (!Enemy.IsEncounted && !Enemy.HasAilment(AilmentType.Blind))
        {
            var diff = player.Position - Enemy.Position;
            var isAdjacent = Mathf.Abs(diff.x) <= 1 && Mathf.Abs(diff.y) <= 1;
            var playerTile = floorInfo.GetTile(player.Position);
            var currentTile = floorInfo.GetTile(Enemy.Position);
            var isSameRoom = playerTile.IsRoom && currentTile.IsRoom && playerTile.Id == currentTile.Id;
            Enemy.IsEncounted = isAdjacent || isSameRoom;
        }
        if (!Enemy.IsEncounted) return;
        await base.MoveAsync(token);
    }

    public override async UniTask AttackAsync(CancellationToken token)
    {
        // 攻撃されるか隣接されたら目を覚ます
        Enemy.IsEncounted = true;
        await base.AttackAsync(token);
    }
}

/// <summary>
/// 遠距離攻撃AI(直線上にプレイヤーがいれば射程内から射撃する)
/// </summary>
public class RangedAI : DefaultAI
{
    public RangedAI(FloorManager floorInfo, Enemy enemy, Player player) : base(floorInfo, enemy, player) { }

    private int Range => Mathf.Max(Enemy.Data.Master.AIParam1, 2);

    public override bool CanAttack()
    {
        if (base.CanAttack()) return true;
        return FindShootDirection().HasValue;
    }

    public override async UniTask AttackAsync(CancellationToken token)
    {
        // 隣接時は通常攻撃
        if (base.CanAttack())
        {
            await base.AttackAsync(token);
            return;
        }
        var direction = FindShootDirection();
        if (!direction.HasValue) return;
        Enemy.IsEncounted = true;
        await Enemy.RotateAsync(direction.Value, token);
        await Enemy.ShootAsync(player, Enemy.Data.Atk, token);
    }

    /// <summary>
    /// プレイヤーが射程内の直線上(8方向)にいて、間に壁や他ユニットがなければその方向を返す
    /// </summary>
    private Vector2Int? FindShootDirection()
    {
        var diff = player.Position - Enemy.Position;
        if (diff.x != 0 && diff.y != 0 && Mathf.Abs(diff.x) != Mathf.Abs(diff.y)) return null;
        var distance = Mathf.Max(Mathf.Abs(diff.x), Mathf.Abs(diff.y));
        if (distance <= 0 || distance > Range) return null;
        var direction = new Vector2Int(System.Math.Sign(diff.x), System.Math.Sign(diff.y));
        for (var count = 1; count < distance; count++)
        {
            var position = Enemy.Position + direction * count;
            var tile = floorInfo.GetTile(position);
            if (tile == null || tile.IsWall) return null;
            if (floorInfo.GetUnit(position) != null) return null;
        }
        return direction;
    }
}

/// <summary>
/// 状態異常投擲AI(同じ部屋にいるプレイヤーへ毒を投げる)
/// </summary>
public class AilmentThrowerAI : DefaultAI
{
    public AilmentThrowerAI(FloorManager floorInfo, Enemy enemy, Player player) : base(floorInfo, enemy, player) { }

    public override bool CanAttack()
    {
        if (base.CanAttack()) return true;
        return CanThrow();
    }

    public override async UniTask AttackAsync(CancellationToken token)
    {
        // 隣接時は通常攻撃
        if (base.CanAttack())
        {
            await base.AttackAsync(token);
            return;
        }
        if (!CanThrow()) return;
        Enemy.IsEncounted = true;
        var diff = player.Position - Enemy.Position;
        await Enemy.RotateAsync(diff, token);
        var master = Enemy.Data.Master;
        await Enemy.ThrowAilmentAsync(player, AilmentType.Poison, master.AIParam1, master.AIParam2, token);
    }

    private bool CanThrow()
    {
        // すでに毒状態なら投げずに接近する
        if (player.Data.Ailments.ContainsKey(AilmentType.Poison)) return false;
        var playerTile = floorInfo.GetTile(player.Position);
        var currentTile = floorInfo.GetTile(Enemy.Position);
        return playerTile.IsRoom && currentTile.IsRoom && playerTile.Id == currentTile.Id;
    }
}

/// <summary>
/// 盗みAI(隣接時にジェムを盗み、盗んだ後はプレイヤーから逃げ回る)
/// </summary>
public class ThiefAI : DefaultAI
{
    public ThiefAI(FloorManager floorInfo, Enemy enemy, Player player) : base(floorInfo, enemy, player) { }

    private bool HasStolen => Enemy.Data.StolenGems > 0;

    public override bool CanAttack()
    {
        // 盗んだ後は攻撃せず逃げに徹する
        if (HasStolen) return false;
        return base.CanAttack();
    }

    public override async UniTask AttackAsync(CancellationToken token)
    {
        // ジェムを持っていない相手には通常攻撃
        if (player.Data.Gems <= 0)
        {
            await base.AttackAsync(token);
            return;
        }
        var diff = player.Position - Enemy.Position;
        await Enemy.RotateAsync(diff, token);
        Enemy.StealGems(player, Mathf.Max(Enemy.Data.Master.AIParam1, 1));
    }

    public override async UniTask MoveAsync(CancellationToken token)
    {
        if (!HasStolen)
        {
            await base.MoveAsync(token);
            return;
        }
        // プレイヤーから最も遠ざかる隣接タイルへ逃げる
        var candidates = floorInfo.GetAroundTilesAt(Enemy.Position)
            .Where(tile => !tile.IsWall && floorInfo.GetUnit(tile.Position) == null)
            .ToList();
        if (candidates.Count <= 0)
        {
            cantMoveTurns++;
            return;
        }
        var currentDistance = (player.Position - Enemy.Position).sqrMagnitude;
        var destTile = candidates.OrderByDescending(tile => (player.Position - (Vector2Int)tile.Position).sqrMagnitude).First();
        // これ以上逃げられない場合はその場に留まる
        if ((player.Position - (Vector2Int)destTile.Position).sqrMagnitude <= currentDistance)
        {
            cantMoveTurns++;
            return;
        }
        cantMoveTurns = 0;
        await Enemy.MoveToAsync(destTile.Position, token);
        await CheckTrapAsync(token);
    }
}
