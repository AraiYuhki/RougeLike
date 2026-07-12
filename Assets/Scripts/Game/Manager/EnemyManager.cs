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

    private FloorInfo floorSetting;
    private Player player = null;
    private List<EnemyAI> enemies = new List<EnemyAI>();
    private int spawnedCount = 0;

    public List<Enemy> Enemies => enemies.Select(enemy => enemy.Enemy).ToList();
    public List<EnemyData> DataList => Enemies.Select(enemy => enemy.Data).ToList();

    public void Initialize(Player player, FloorInfo floorSetting)
    {
        this.floorSetting = floorSetting;
        this.player = player;
        for (var count = 0; count < floorSetting.InitialSpawnEnemyCount; count++)
            Spawn();
    }

    public void LoadFromJson(List<EnemyData> enemies)
    {
        foreach (var data in enemies)
        {
            var instance = Instantiate(data.Master.Prefab, floorManager.transform);
            instance.Initialize(
                data, data.Position,
                gameController, floorManager,
                this, itemManager,
                notice, damagePopupManager
                );
            Setup(instance);
            instance.SetAngle(data.Angle);
        }
    }

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
        var enemyId = floorSetting.Enemies.Random();
        var master = DB.Instance.MEnemy.GetById(enemyId);
        var instance = Instantiate(master.Prefab, floorManager.transform);
        var playerTile = floorManager.GetTile(player.Position);
        var tiles = floorManager.GetEmptyRoomTiles(playerTile.Id);

        instance.Initialize(
            enemyId, tiles.Random().Position,
            gameController, floorManager,
            this, itemManager,
            notice, damagePopupManager);

        Setup(instance);
    }

    private void Setup(Enemy instance)
    {
        var ai = new DefaultAI(floorManager, instance, player);

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
            // 麻痺中は行動不可、拘束中は移動のみ不可
            var actableEnemies = enemies.Where(e => !e.Enemy.HasAilment(AilmentType.Paralysis)).ToList();
            var moveEnemies = actableEnemies.Where(e => !e.CanAttack() && !e.Enemy.HasAilment(AilmentType.Bind)).ToList();
            var attackEnemies = actableEnemies.Where(e => e.CanAttack()).ToList();

            var tasks = new List<UniTask>();
            foreach (var enemy in moveEnemies)
                tasks.Add(enemy.MoveAsync(enemy.Enemy.destroyCancellationToken));
            minimap.UpdateView();
            await UniTask.WhenAll(tasks);

            foreach (var enemy in attackEnemies)
                await enemy.AttackAsync(enemy.Enemy.destroyCancellationToken);

            // 状態異常(毒ダメージなど)のターン経過処理
            foreach (var enemy in Enemies)
                enemy.TurnEnd();
            await UniTask.Yield();
            minimap.UpdateView();
            stateMachine.Goto(GameState.PlayerTurn);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
