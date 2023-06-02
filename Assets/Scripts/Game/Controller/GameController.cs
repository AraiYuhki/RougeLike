using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameStatus
{
    Wait = 0,
    PlayerControll,
    UIControll,
    EnemyControll,
    TurnEnd,
}

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject uiController = null;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private Player player = null;

    public Player Player => player;
    private FloorManager floorManager => ServiceLocator.FloorManager;
    private EnemyManager enemyManager => ServiceLocator.EnemyManager;
    private ItemManager itemManager => ServiceLocator.ItemManager;

    private Vector2Int move = Vector2Int.zero;
    private GameStatus status = GameStatus.Wait;
    private bool isExecuteCommand = false;
    private bool isTurnMode = false;
    private Coroutine turnControll = null;

    private IControllable currentControllObject = null;

    private Dictionary<GameStatus, IControllable> controllers = new Dictionary<GameStatus, IControllable>();

    private static readonly RuntimePlatform[] EnableUIControllerPlatforms = new RuntimePlatform[]
    {
        RuntimePlatform.Android,
        RuntimePlatform.IPhonePlayer,
        RuntimePlatform.WebGLPlayer
    };


    private void Awake()
    {
        status = GameStatus.PlayerControll;
    }

    private void Start()
    {
        uiController.gameObject.SetActive(EnableUIControllerPlatforms.Contains(Application.platform));

        var playerController = new PlayerController(player, uiManager, this);

        uiManager.Initialize(floorManager, player,
            () => status = GameStatus.EnemyControll,
            playerController.TakeItem,
            playerController.ThrowItem,
            playerController.DropItem);
        uiManager.OnCloseMenu = () => status = GameStatus.PlayerControll;
        floorManager.Clear();
        floorManager.Create(100, 100, 40, false);
        player.Initialize();
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        player.OnMoved += floorManager.OnMoveUnit;

        turnControll = StartCoroutine(TurnControll());

        enemyManager.Initialize(player);
        itemManager.Initialize();
        controllers = new Dictionary<GameStatus, IControllable>()
        {
            { GameStatus.PlayerControll, playerController},
        };
    }

    private int index = 0;
    private void Update()
    {
        // 現在の操作対象が存在し、ルーチンを進めた結果が処理終了ならそこで処理を止める
        if (currentControllObject != null && !currentControllObject.Controll().MoveNext())
            currentControllObject = null;
        ChangeStatus();
    }

    public void SetStatus(GameStatus newStatus) => status = newStatus;

    private void ChangeStatus()
    {
        if (!controllers.ContainsKey(status)) throw new System.NotImplementedException($"{status} is not implemeted");
        currentControllObject = controllers[status];
        currentControllObject.Controll().Reset();
    }

    private void OpenDialog()
    {
        var dialog = ServiceLocator.DialogManager.Open<CommonDialog>();
        dialog.Initialize($"テスト{index}", $"テストダイアログ{index}", ("さらに開く", () => OpenDialog()), ("閉じる", () => ServiceLocator.DialogManager.Close(dialog)));
        index++;
    }

    private void OnDestroy()
    {
        StopCoroutine(turnControll);
    }

    public IEnumerator TurnControll()
    {
        while (true)
        {
            if (ServiceLocator.DialogManager.Controll())
            {
                yield return null;
                continue;
            }
            switch (status)
            {
                case GameStatus.UIControll:
                    yield return UIControll();
                    break;
                case GameStatus.EnemyControll:
                    yield return enemyManager.Controll();
                    status = GameStatus.TurnEnd;
                    break;
                case GameStatus.TurnEnd:
                    foreach (var unit in FindObjectsOfType<Unit>())
                        unit.TurnEnd();
                    status = GameStatus.PlayerControll;
                    break;
                case GameStatus.Wait:
                default:
                    yield return null;
                    break;

            }
        }
    }

    public void Up() => currentControllObject?.Up();
    public void Down() => currentControllObject?.Down();
    public void Right() => currentControllObject?.Right();
    public void Left() => currentControllObject?.Left();
    public void TurnMode() => currentControllObject?.TurnMode();
    public void Wait() => currentControllObject?.Wait();

    public void OpenMenu()
    {
        uiManager.OpenMenu(() => status = GameStatus.UIControll);
        status = GameStatus.Wait;
    }

    private IEnumerator PlayerControll()
    {
        if (player.IsLockInput) yield break;

        if (InputUtility.Menu.IsTriggerd())
        {
            OpenMenu();
            yield break;
        }
        if (InputUtility.Wait.IsPressed()) isExecuteCommand = true;
        if (InputUtility.Up.IsPressed()) Up();
        else if (InputUtility.Down.IsPressed()) Down();
        if (InputUtility.Right.IsPressed()) Right();
        else if (InputUtility.Left.IsPressed()) Left();
        isTurnMode |= InputUtility.TurnMode.IsPressed();

        if (move.x != 0 || move.y != 0)
        {
            var destPosition = player.Position + move;
            var destTile = floorManager.GetTile(destPosition);
            var enemy = floorManager.GetUnit(destPosition);
            if (enemy != null)
            {
                Attack(enemy);
                yield break;
            }
            if (destTile.IsWall || isTurnMode)
            {
                player.SetDestAngle(move);
            }
            else
            {
                player.Move(move);
                TakeItem();
                isExecuteCommand = true;
            }
        }
        move = Vector2Int.zero;
        isTurnMode = false;
        if (isExecuteCommand)
        {
            isExecuteCommand = false;
            status = GameStatus.EnemyControll;
        }
        yield return null;
    }

    private void Attack(Unit target)
    {
        status = GameStatus.Wait;
        player.SetDestAngle(move);
        move = Vector2Int.zero;
        player.Attack(target, () => status = GameStatus.EnemyControll);
    }

    private void DropItem(ItemBase target)
    {
        itemManager.Drop(target, 0, Player.Position);
        status = GameStatus.EnemyControll;
    }

    private void ThrowItem(ItemBase target)
    {
        var item = itemManager.Drop(target, 0, Player.Position);
        (int length, var position, var enemy) = floorManager.GetHitPosition(player.Position, player.Angle, 10);
        var tween = item.transform
            .DOLocalMove(new Vector3(position.x, 0, position.y), 0.1f * length)
            .SetEase(Ease.Linear)
            .Play();
        tween.onComplete += () => ThrownItem(target, item, position, enemy);
    }

    private void ThrownItem(ItemBase target, Item item, Vector2Int targetPosition, Enemy enemy)
    {
        floorManager.RemoveItem(item.Position);
        // 当たる位置にドロップできない場合は周囲からドロップできる場所を探す
        if (enemy != null)
        {
            if (target is WeaponData weapon)
                enemy.Damage(DamageUtil.GetDamage(player, weapon.Atk), player);
            else if (target is ShieldData shield)
                enemy.Damage(DamageUtil.GetDamage(player, shield.Def), player);
            else if (target is UsableItemData usableItem)
                usableItem.Use(enemy);
            itemManager.Despawn(item);
            status = GameStatus.EnemyControll;
            return;
        }
        if (!floorManager.CanDrop(targetPosition))
        {
            // 周囲のドロップできる場所を検索
            var candidate = floorManager.GetCanDropTile(targetPosition);
            // 候補あり
            if (candidate != null)
            {
                var dropTween = item.transform
                    .DOLocalMove(new Vector3(candidate.Position.X, 0f, candidate.Position.Y), 0.5f)
                    .SetEase(Ease.OutExpo)
                    .Play();
                dropTween.onComplete += () =>
                {
                    floorManager.SetItem(item, candidate.Position);
                    status = GameStatus.EnemyControll;
                };
                return;
            }
            // 候補がないので消滅
            itemManager.Despawn(item);
            return;
        }
        floorManager.SetItem(item, targetPosition);
        status = GameStatus.EnemyControll;
    }

    private void TakeItem()
    {
        var item = floorManager.GetItem(player.Position);
        if (item == null) return;
        if (item.IsGem)
        {
            player.Data.Gems += item.GemCount;
            Debug.LogError($"ジェムを{item.GemCount}個拾った");
        }
        else
        {
            player.Data.TakeItem(item.Data);
            Debug.LogError($"{item.Data.Name}を拾った");
        }
        floorManager.RemoveItem(item.Position);
        ServiceLocator.ItemManager.Despawn(item);
        status = GameStatus.EnemyControll;
    }

    private IEnumerator UIControll()
    {
        if (InputUtility.Menu.IsTriggerd())
        {
            uiManager.CloseMenu();
            status = GameStatus.Wait;
            yield return null;
            yield break;
        }

        yield return uiManager.UpdateUI();
    }
}
