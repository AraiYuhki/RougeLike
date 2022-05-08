using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private Enemy[] prefabs = new Enemy[0];

    private Player player = null;
    private List<EnemyAI> enemies = new List<EnemyAI>();
    private FloorManager floorManager => ServiceLocator.Instance.FloorManager;
    private int spawnedCount = 0;
    const int spawnIntervalTurn = 30;

    public void Initialize(Player player)
    {
        this.player = player;
        for (var count = 0; count < 4; count++)
            Spawn();
    }

    public void Spawn()
    {
        var instance = Instantiate(prefabs.First(), floorManager.transform);
        var ai = new DefaultAI(floorManager, instance, player);
        var playerTile = floorManager.GetTile(player.Position);
        var tiles = floorManager.GetEmptyRoomTiles(playerTile.Id);
        instance.SetPosition(tiles.Random().Position);
        floorManager.SetUnit(instance, instance.Position);
        ai.OnMove = floorManager.OnMoveUnit;
        enemies.Add(ai);
    }

    public IEnumerator EnemyControll()
    {
        if (spawnIntervalTurn < spawnedCount)
        {
            Spawn();
            spawnedCount = 0;
        }
        else
        {
            spawnedCount++;
        }
        var moveEnemies = enemies.Where(e => !e.CanAttack()).ToList();
        var attackEnemies = enemies.Where(e => e.CanAttack()).ToList();
        foreach(var enemy in moveEnemies)
            enemy.Move();
        foreach (var enemy in attackEnemies)
            yield return enemy.Attack();
    }
}
