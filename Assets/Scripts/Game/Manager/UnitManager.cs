using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager
{
    private Player player = null;
    private List<Enemy> enemies = new List<Enemy>();
    private Unit[,] unitList = null;
    private Floor floorInfo = null;

    private Unit GetUnit(int x, int y) => unitList[x, y];
    private Unit GetUnit(Vector2Int position) => GetUnit(position.x, position.y);
    private void SetUnit(Unit unit) => unitList[unit.Position.x, unit.Position.y] = unit;
    private Vector2Int GetUnitPosition(Unit unit)
    {
        var result = unitList.WithIndex().FirstOrDefault(info => info.Value == unit);
        return result != null ? result.position : Vector2Int.zero;
    }
    

    public UnitManager(Player player) => this.player = player;

    public void Initialize(Floor floorInfo)
    {
        this.floorInfo = floorInfo;
        unitList = new Unit[floorInfo.Size.x, floorInfo.Size.y];
        unitList[player.Position.x, player.Position.y] = player;
    }

    public IEnumerator EnemyControll()
    {
        foreach(var enemy in enemies)
        {
            yield break;
        }
    }

    private void EnemyControll(Enemy enemy)
    {
        var playerTile = floorInfo.GetTile(player.Position);
        var currentTile = floorInfo.GetTile(enemy.Position);
        unitList[enemy.Position.x, enemy.Position.y] = null;
        if (!enemy.IsEncounted)
        {
            if (playerTile.IsRoom && currentTile.IsRoom)
            {
                enemy.IsEncounted = playerTile.Id == currentTile.Id;
            }
        }
        if (enemy.IsEncounted)
        {
            var root = floorInfo.GetRoot(enemy.Position, player.Position);
            enemy.Move(root.Skip(1).First());
        }
        else
        {
            if (enemy.TargetTile == null || enemy.TargetRoomId == currentTile.Id)
            {
                var count = 0;
                var targetRoomId = floorInfo.RoomIds.Random();
                while ((targetRoomId = floorInfo.RoomIds.Random()) == currentTile.Id && count < 10)
                    count++;
                enemy.TargetTile = floorInfo.GetRoomTiles(targetRoomId).Random();
            }
            var root = floorInfo.GetRoot(enemy.Position, enemy.TargetTile.Position);
            enemy.Move(root.Skip(1).First());
        }
        
        unitList[enemy.Position.x, enemy.Position.y] = enemy;
    }

    private bool CanAttack(Enemy enemy)
        => Mathf.Abs(enemy.Position.x - player.Position.x) <= 1 && Mathf.Abs(enemy.Position.y - player.Position.y) <= 1;

}
