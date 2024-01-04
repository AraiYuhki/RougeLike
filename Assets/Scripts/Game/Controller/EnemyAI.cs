using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
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

    public virtual bool CanAttack()
    {
        var diff = player.Position - Enemy.Position;
        return Mathf.Abs(diff.x) <= 1 && Mathf.Abs(diff.y) <= 1;
    }

    public virtual UniTask MoveAsync(TweenCallback onComplete = null)
    {
        onComplete?.Invoke();
        return new UniTask();
    }

    public virtual UniTask AttackAsync()
    {
        return new UniTask();
    }
}

public class DefaultAI : EnemyAI
{
    protected List<Vector2Int> checkPoints = new List<Vector2Int>();
    protected List<int> rootRooms = new List<int>();
    public DefaultAI(FloorManager floorInfo, Enemy enemy, Player player) : base(floorInfo, enemy, player) { }

    public override async UniTask MoveAsync(TweenCallback onComplete = null)
    {
        if (!Enemy.IsEncounted)
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
            await base.MoveAsync(onComplete);
            cantMoveTurns++;
            return;
        }
        // 次の移動先を探す(rootの0番目は現在地なのでスキップ)
        var nextTile = root.Skip(1).First();
        if (floorInfo.GetUnit(nextTile) != null)
        {
            // 移動できなかった
            await base.MoveAsync(onComplete);
            cantMoveTurns++;
            return;
        }
        cantMoveTurns = 0;
        Enemy.MoveTo(nextTile, () =>
        {
            CheckTrap();
            onComplete?.Invoke();
        });
    }

    public override async UniTask AttackAsync()
    {
        var diff = player.Position - Enemy.Position;
        await Enemy.RotateAsync(diff, default);
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

    private void CheckTrap()
    {
        var trap = floorInfo.GetTrap(Enemy.Position);
        if (trap == null) return;
        trap.ExecuteAsync(Enemy, Enemy.GetCancellationTokenOnDestroy()).Forget();
    }
}
