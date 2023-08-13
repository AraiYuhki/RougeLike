using DG.Tweening;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameStatus
{
    Wait = 0,
    PlayerControll,
    UIControll,
    EnemyControll,
    DialogControll,
    TurnEnd,
}

public class GameController : MonoBehaviour
{
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private EnemyManager enemyManager;
    [SerializeField]
    private ItemManager itemManager;
    [SerializeField]
    private DialogManager dialogManager;
    [SerializeField]
    private GameObject controllerUI = null;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private NoticeGroup noticeGroup = null;
    [SerializeField]
    private CardController cardController = null;
    [SerializeField]
    private FloorMoveView floorMoveView = null;
    [SerializeField]
    private ShopWindow shopWindow = null;
    [SerializeField]
    private Player player = null;

    [SerializeField]
    private TMP_Text floorLabel;

    [SerializeField]
    private GameObject bulletPrefab = null;

    private Coroutine turnControll = null;

    private DungeonStateMachine stateMachine;

    private int currentFloor = 1;

    public int CurrentFloor
    {
        get => currentFloor;
        set
        {
            currentFloor = value;
            floorLabel.text = floorManager.IsTower ? $"{currentFloor}F" : $"B{currentFloor}F";
        }
    }

    private static readonly RuntimePlatform[] EnableUIControllerPlatforms = new RuntimePlatform[]
    {
        RuntimePlatform.Android,
        RuntimePlatform.IPhonePlayer,
        RuntimePlatform.WebGLPlayer
    };


    private void Awake()
    {
        stateMachine = new DungeonStateMachine();
        stateMachine.AddState(GameState.Wait, new WaitState());
        stateMachine.AddState(GameState.PlayerTurn, new PlayerTurnState(stateMachine, floorManager, itemManager, dialogManager, noticeGroup, player, cardController));
        stateMachine.AddState(GameState.EnemyTurn, new EnemyTurnState(stateMachine, enemyManager));
        stateMachine.AddState(GameState.MainMenu, new MenuState(stateMachine, uiManager));
        stateMachine.AddState(GameState.NextFloorLoad, new LoadFloorState(player, stateMachine, this, floorManager, floorMoveView));
        stateMachine.AddState(GameState.Dialog, new DialogState());
        stateMachine.AddState(GameState.Shop, new ShopState(stateMachine, shopWindow));
    }

    private void Start()
    {
        controllerUI.gameObject.SetActive(EnableUIControllerPlatforms.Contains(Application.platform));

        CurrentFloor = 1;
        floorManager.Clear();
        floorManager.Create(20, 20, 4, false);
        player.Initialize();
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        player.OnMoved += floorManager.OnMoveUnit;
        cardController.Player = player;
        cardController.Initialize();

        turnControll = StartCoroutine(stateMachine.Update());

        enemyManager.Initialize(player);
        itemManager.Initialize(150, 1, 5);
        Fade.Instance.FadeIn(() => stateMachine.Goto(GameState.PlayerTurn));
    }

    private void OnDestroy()
    {
        StopCoroutine(turnControll);
    }

    public void LoadNextFloor()
    {
        CurrentFloor++;
        enemyManager.Clear();
        itemManager.Clear();
        floorManager.Clear();
        floorManager.Create(40, 40, 10, floorManager.IsTower);
        for (var count = 0; count < 4; count++)
            enemyManager.Spawn();
        itemManager.Initialize(150, 1, 5);
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        ForceUpdateMinimap();
    }

    public void StartEnemyTurn() => stateMachine.Goto(GameState.EnemyTurn);

    /// <summary>
    /// インスペクター上から使用
    /// アイテムをその場に置く
    /// </summary>
    /// <param name="target"></param>
    public void DropItem(ItemBase target)
    {
        itemManager.Drop(target, 0, player.Position);
        StartEnemyTurn();
    }

    /// <summary>
    /// インスペクター上から使用
    /// アイテムを投げる
    /// </summary>
    /// <param name="target"></param>
    public void ThrowItem(ItemBase target)
    {
        var item = itemManager.Drop(target, 0, player.Position);
        var targetPosition = floorManager.GetHitPosition(player.Position, player.Angle, 10);
        var tween = item.transform
            .DOLocalMove(new Vector3(targetPosition.position.x, 0, targetPosition.position.y), 0.1f * targetPosition.length)
            .SetEase(Ease.Linear)
            .Play();
        tween.onComplete += () =>
        {
            floorManager.RemoveItem(item.Position);
            // 飛んでいった先に敵がいるか？
            if (targetPosition.enemy != null)
            {
                var enemy = targetPosition.enemy;
                if (target is WeaponData weapon)
                    enemy.Damage(DamageUtil.GetDamage(player, weapon.Atk), player);
                else if (target is ShieldData shield)
                    enemy.Damage(DamageUtil.GetDamage(player, shield.Def), player);
                // ぶつけたアイテムを強制的に使用させる
                else if (target is UsableItemData usableItem)
                    usableItem.Use(enemy);
                itemManager.Despawn(item);
                StartEnemyTurn();
                return;
            }
            if (!floorManager.CanDrop(targetPosition.position))
            {
                // ドロップ可能タイルを検索
                var candidate = floorManager.GetCanDropTile(targetPosition.position);
                // ドロップ可能
                if (candidate != null)
                {
                    var dropTween = item.transform
                    .DOLocalMove(new Vector3(candidate.Position.X, 0f, candidate.Position.Y), 0.5f)
                    .SetEase(Ease.OutExpo)
                    .Play();
                    dropTween.onComplete += () =>
                    {
                        floorManager.SetItem(item, candidate.Position);
                        StartEnemyTurn();
                    };
                    return;
                }
                // ドロップできる場所がないので消滅
                itemManager.Despawn(item);
                return;
            }
            floorManager.SetItem(item, targetPosition.position);
            StartEnemyTurn();
        };
    }

    public GameObject CreateBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = Instantiate(bulletPrefab, floorManager.transform);
        bullet.transform.localScale = Vector3.one;
        bullet.transform.localPosition = position;
        bullet.transform.localRotation = rotation;
        return bullet;
    }

    /// <summary>
    /// インスペクター上から使用
    /// アイテムを拾う
    /// </summary>
    public void TakeItem()
    {
        var item = floorManager.GetItem(player.Position);
        if (item == null) return;
        if (item.IsGem)
        {
            player.Data.Gems += item.GemCount;
            noticeGroup.Add($"ジェムを{item.GemCount}個拾った", Color.cyan);
        }
        else
        {
            player.Data.TakeItem(item.Data);
            noticeGroup.Add($"{item.Data.Name}を拾った", Color.cyan);
        }
        floorManager.RemoveItem(item.Position);
        itemManager.Despawn(item);
        StartEnemyTurn();
    }

    public void ForceUpdateMinimap() => uiManager.ForceUpdateMinimap();
}
