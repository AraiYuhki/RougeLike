using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class EnemyAI
{
    public Action<Unit, Vector2Int> OnMove { get; set; }
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

    public virtual void Move()
    {
    }
    public virtual IEnumerator Attack()
    {
        yield return null;
    }
}

public class DefaultAI : EnemyAI
{
    public DefaultAI(FloorManager floorInfo, Enemy enemy, Player player) : base(floorInfo, enemy, player) { }

    public override void Move()
    {
        var playerTile = floorInfo.GetTile(player.Position);
        var currentTile = floorInfo.GetTile(Enemy.Position);
        if (!Enemy.IsEncounted)
        {
            if (playerTile.IsRoom && currentTile.IsRoom)
            {
                Enemy.IsEncounted = playerTile.Id == currentTile.Id;
            }
        }

        var targetPosition = player.Position;
        if (!Enemy.IsEncounted)
        {
            if (Enemy.TargetTile == null || Enemy.TargetRoomId == currentTile.Id)
            {
                var count = 0;
                var targetRoomId = floorInfo.RoomIds.Random();
                while ((targetRoomId = floorInfo.RoomIds.Random()) == currentTile.Id && count < 10)
                    count++;
                Enemy.TargetTile = floorInfo.GetRoomTiles(targetRoomId).Random();
            }
            targetPosition = Enemy.TargetTile.Position;
        }

        var root = floorInfo.GetRoot(Enemy.Position, targetPosition);
        if (root == null) return;
        var nextTile = root.Skip(1).First();
        if (floorInfo.GetUnit(nextTile) != null)
        {
            return;
        }
        Enemy.MoveTo(nextTile);
        OnMove?.Invoke(Enemy, nextTile);
    }

    public override IEnumerator Attack()
    {
        var diff = player.Position - Enemy.Position;
        Enemy.SetDestAngle(diff);
        while (Enemy.EndRotation) yield return new WaitForEndOfFrame();
        var endAttackAnimation = false;
        var targetPosition = new Vector3(player.Position.x, 0.5f, player.Position.y);
        Enemy.Attack(targetPosition, () => endAttackAnimation = true);
        while(!endAttackAnimation) yield return new WaitForEndOfFrame();
    }
}
