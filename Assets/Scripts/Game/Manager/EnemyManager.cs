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
        var ai = CreateAI(instance);

        floorManager.SetUnit(instance, instance.Position);
        instance.OnMoved += floorManager.OnMoveUnit;
        if (instance.Data.Master.AIType == EnemyAIType.Split)
            instance.OnDamage += (attacker, damage) => TrySplit(instance);
        instance.OnDead += () =>
        {
            // 盗んだジェムは倒された場所にドロップする
            if (instance.Data.StolenGems > 0)
            {
                var dropTile = floorManager.GetCanDropTile(instance.Position);
                if (dropTile != null)
                    itemManager.Drop(instance.Data.StolenGems, dropTile.Position, true);
            }
            enemies.Remove(ai);
            floorManager.RemoveUnit(instance.Position);
            minimap.RemoveSymbol(instance);
            Destroy(instance.gameObject);
        };
        enemies.Add(ai);
        minimap.AddSymbol(instance);
    }

    private EnemyAI CreateAI(Enemy instance)
    {
        return instance.Data.Master.AIType switch
        {
            EnemyAIType.Ranged => new RangedAI(floorManager, instance, player),
            EnemyAIType.AilmentThrower => new AilmentThrowerAI(floorManager, instance, player),
            EnemyAIType.Thief => new ThiefAI(floorManager, instance, player),
            EnemyAIType.DoubleSpeed => new DoubleSpeedAI(floorManager, instance, player),
            EnemyAIType.Sleeper => new SleeperAI(floorManager, instance, player),
            EnemyAIType.HandDiscarder => new HandDiscardAI(floorManager, instance, player),
            _ => new DefaultAI(floorManager, instance, player),
        };
    }

    /// <summary>
    /// ダメージを受けて生き残った場合、一定確率で空いている隣接タイルに分裂する
    /// </summary>
    private void TrySplit(Enemy original)
    {
        if (original.Hp <= 0) return;
        if (UnityEngine.Random.Range(0, 100) >= original.Data.Master.AIParam1) return;
        var candidates = floorManager.GetAroundTilesAt(original.Position)
            .Where(tile => !tile.IsWall && floorManager.GetUnit(tile.Position) == null)
            .ToList();
        if (candidates.Count <= 0) return;
        var instance = Instantiate(original.Data.Master.Prefab, floorManager.transform);
        instance.Initialize(
            original.Data, candidates.Random().Position,
            gameController, floorManager,
            this, itemManager,
            notice, damagePopupManager);
        Setup(instance);
        notice.Add($"{original.Name}は分裂した!", Color.magenta);
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
            // 倍速の敵は1ターンに複数回行動する
            var maxActionCount = enemies.Count > 0 ? enemies.Max(e => e.ActionCount) : 0;
            for (var action = 0; action < maxActionCount; action++)
            {
                // 麻痺中は行動不可、拘束中は移動のみ不可
                var actableEnemies = enemies.Where(e => e.ActionCount > action && !e.Enemy.HasAilment(AilmentType.Paralysis)).ToList();
                var moveEnemies = actableEnemies.Where(e => !e.CanAttack() && !e.Enemy.HasAilment(AilmentType.Bind)).ToList();
                var attackEnemies = actableEnemies.Where(e => e.CanAttack()).ToList();

                var tasks = new List<UniTask>();
                foreach (var enemy in moveEnemies)
                    tasks.Add(enemy.MoveAsync(enemy.Enemy.destroyCancellationToken));
                minimap.UpdateView();
                await UniTask.WhenAll(tasks);

                foreach (var enemy in attackEnemies)
                    await enemy.AttackAsync(enemy.Enemy.destroyCancellationToken);
            }

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
