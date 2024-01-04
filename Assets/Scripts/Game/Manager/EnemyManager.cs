using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private GameController gameController;
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private ItemManager itemManager;
    [SerializeField]
    private Minimap minimap;
    [SerializeField]
    private NoticeGroup notice;
    [SerializeField]
    private DamagePopupManager damagePopupManager;
    [SerializeField]
    private Enemy[] prefabs = new Enemy[0];

    private FloorInfo floorSetting;
    private Player player = null;
    private List<EnemyAI> enemies = new List<EnemyAI>();
    private int spawnedCount = 0;

    public List<Enemy> Enemies => enemies.Select(enemy => enemy.Enemy).ToList();

    public void Initialize(Player player, FloorInfo floorSetting)
    {
        this.floorSetting = floorSetting;
        this.player = player;
        for (var count = 0; count < floorSetting.InitialSpawnEnemyCount; count++)
            Spawn();
    }

    public void SetFloorData(FloorInfo floorSetting) => this.floorSetting = floorSetting;

    public void Clear()
    {
        foreach (var enemy in Enemies)
        {
            floorManager.RemoveUnit(enemy.Position);
            Destroy(enemy.gameObject);
        }
        enemies.Clear();
    }

    public void Spawn()
    {
        var instance = Instantiate(prefabs.First(), floorManager.transform);
        var enemyId = floorSetting.Enemies.Random();
        instance.Initialize(DB.Instance.MEnemy.GetById(enemyId));
        var ai = new DefaultAI(floorManager, instance, player);
        var playerTile = floorManager.GetTile(player.Position);
        var tiles = floorManager.GetEmptyRoomTiles(playerTile.Id);
        instance.DamagePopupManager = damagePopupManager;
        instance.SetManagers(gameController, floorManager, this, itemManager, notice);
        instance.SetPosition(tiles.Random().Position);
        floorManager.SetUnit(instance, instance.Position);
        instance.OnMoved += floorManager.OnMoveUnit;
        instance.OnDead += () =>
        {
            enemies.Remove(ai);
            floorManager.RemoveUnit(instance.Position);
            minimap.RemoveSymbol(instance);
            Destroy(instance.gameObject);
        };
        enemies.Add(ai);
        minimap.AddSymbol(instance);
    }

    public async UniTask Controll(DungeonStateMachine stateMachine)
    {
        if (floorSetting.SpawnEnemyIntervalTurn < spawnedCount)
        {
            Spawn();
            spawnedCount = 0;
        }
        else
        {
            spawnedCount++;
        }
        try
        {
            var moveEnemies = enemies.Where(e => !e.CanAttack()).ToList();
            var attackEnemies = enemies.Where(e => e.CanAttack()).ToList();
            var completedCount = 0;
            foreach (var enemy in moveEnemies)
                await enemy.Move(() => completedCount++);
            while (moveEnemies.Count > completedCount) await UniTask.Yield();
            foreach (var enemy in attackEnemies)
                await enemy.AttackAsync();
            await UniTask.Yield();
            stateMachine.Goto(GameState.PlayerTurn);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
