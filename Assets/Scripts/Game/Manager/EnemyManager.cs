using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private NoticeGroup notice;
    [SerializeField]
    private DamagePopupManager damagePopupManager;
    [SerializeField]
    private Enemy[] prefabs = new Enemy[0];

    private Player player = null;
    private List<EnemyAI> enemies = new List<EnemyAI>();
    private int spawnedCount = 0;
    const int spawnIntervalTurn = 30;

    public List<Enemy> Enemies => enemies.Select(enemy => enemy.Enemy).ToList();

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
        instance.DamagePopupManager = damagePopupManager;
        instance.SetNoticeGroup(notice);
        instance.SetPosition(tiles.Random().Position);
        floorManager.SetUnit(instance, instance.Position);
        instance.OnMoved += floorManager.OnMoveUnit;
        instance.OnDead += () =>
        {
            enemies.Remove(ai);
            floorManager.RemoveUnit(instance.Position);
            Destroy(instance.gameObject);
        };
        enemies.Add(ai);
    }

    public async UniTask Controll()
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
        var completedCount = 0;
        foreach (var enemy in moveEnemies)
            await enemy.Move(() => completedCount++);
        while (moveEnemies.Count > completedCount) await UniTask.Yield();
        foreach (var enemy in attackEnemies)
            await enemy.Attack();
    }
}
