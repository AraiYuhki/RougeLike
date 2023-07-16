using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
    private FloorManager floorManager;
    [SerializeField]
    private EnemyManager enemyManager;
    [SerializeField]
    private ItemManager itemManager;
    [SerializeField]
    private DialogManager dialogManager;
    [SerializeField]
    private MenuUI menuUI;
    [SerializeField]
    private MainMenuUI mainMenuUI;
    [SerializeField]
    private GameObject uiController = null;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private NoticeGroup noticeGroup = null;
    [SerializeField]
    private Player player = null;

    private Vector2Int move = Vector2Int.zero;
    private GameStatus status = GameStatus.Wait;
    private bool isExecuteCommand = false;
    private bool isTurnMode = false;
    private Coroutine turnControll = null;

    private DungeonStateMachine stateMachine;

    private static readonly RuntimePlatform[] EnableUIControllerPlatforms = new RuntimePlatform[]
    {
        RuntimePlatform.Android,
        RuntimePlatform.IPhonePlayer,
        RuntimePlatform.WebGLPlayer
    };


    private void Awake()
    {
        status = GameStatus.Wait;
        stateMachine = new DungeonStateMachine();
        stateMachine.AddState(GameState.Wait, new WaitState());
        stateMachine.AddState(GameState.PlayerTurn, new PlayerTurnState(stateMachine, floorManager, itemManager, dialogManager, noticeGroup, player));
        stateMachine.AddState(GameState.EnemyTurn, new EnemyTurnState(stateMachine, enemyManager));
        stateMachine.AddState(GameState.MainMenu, new MainMenuState(stateMachine, mainMenuUI, menuUI));
    }

    private void Start()
    {
        uiController.gameObject.SetActive(EnableUIControllerPlatforms.Contains(Application.platform));

        uiManager.Initialize(floorManager, player,
            () => status = GameStatus.EnemyControll,
            TakeItem,
            ThrowItem,
            DropItem);
        uiManager.OnCloseMenu = () => status = GameStatus.PlayerControll;
        floorManager.Clear();
        floorManager.Create(40, 40, 10, false);
        player.Initialize();
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        player.OnMoved += floorManager.OnMoveUnit;

        //turnControll = StartCoroutine(TurnControll());
        turnControll = StartCoroutine(stateMachine.Update());
        stateMachine.Goto(GameState.PlayerTurn);

        enemyManager.Initialize(player);
        itemManager.Initialize();
        Fade.Instance.FadeIn(() => status = GameStatus.PlayerControll);
    }

    private void OnDestroy()
    {
        StopCoroutine(turnControll);
    }

    public IEnumerator TurnControll()
    {
        while (true)
        {
            if (dialogManager.Controll())
            {
                yield return null;
                continue;
            }
            switch (status)
            {
                case GameStatus.PlayerControll:
                    yield return PlayerControll();
                    break;
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

    public void Up() => move.y = 1;
    public void Down() => move.y = -1;
    public void Right() => move.x = 1;
    public void Left() => move.x = -1;
    public void TurnMode() => isTurnMode = true;
    public void Wait() => isExecuteCommand = true;

    public void SwitchMenu()
    {
        if (uiManager.IsMenuOpened) CloseMenu();
        else OpenMenu();
    }

    public void OpenMenu()
    {
        uiManager.OpenMenu(() => status = GameStatus.UIControll);
        status = GameStatus.Wait;
    }

    public void CloseMenu()
    {
        uiManager.CloseMenu(() => status = GameStatus.PlayerControll);
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
            var currentPosition = player.Position;
            var destPosition = currentPosition + move;
            var destTile = floorManager.GetTile(destPosition);
            var enemy = floorManager.GetUnit(destPosition);
            if (enemy != null)
            {
                status = GameStatus.Wait;
                player.SetDestAngle(move);
                move = Vector2Int.zero;
                player.Attack(enemy, () => status = GameStatus.EnemyControll);
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
                // �K�i���Ȃ���΂��̂܂܎���
                isExecuteCommand = !CheckStair();
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

    private void DropItem(ItemBase target)
    {
        itemManager.Drop(target, 0, player.Position);
        status = GameStatus.EnemyControll;
    }

    private void ThrowItem(ItemBase target)
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
                status = GameStatus.EnemyControll;
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
                        status = GameStatus.EnemyControll;
                    };
                    return;
                }
                // ドロップできる場所がないので消滅
                itemManager.Despawn(item);
                return;
            }
            floorManager.SetItem(item, targetPosition.position);
            status = GameStatus.EnemyControll;
        };
    }

    private void TakeItem()
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
        status = GameStatus.EnemyControll;
    }

    private bool CheckStair()
    {
        var stairPosition = floorManager.FloorData.StairPosition;
        if (stairPosition.X == player.Position.x && stairPosition.Y == player.Position.y)
        {
            var dialog = dialogManager.Open<CommonDialog>();
            dialog.Initialize("確認", "次の階へ進みますか？", ("はい", () =>
            {
                dialogManager.Close(dialog);
                Fade.Instance.FadeOut(OpenShop);
            }),
            ("いいえ", () =>
            {
                dialogManager.Close(dialog);
                status = GameStatus.EnemyControll;
            }));
            return true;
        }
        return false;
    }

    private void OpenShop()
    {

    }

    private IEnumerator UIControll()
    {
        //if (InputUtility.Menu.IsTriggerd())
        //{
        //    CloseMenu();
        //    yield return null;
        //}

        //yield return uiManager.UpdateUI();
        yield return null;
    }
}
