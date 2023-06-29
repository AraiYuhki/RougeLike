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
    private GameObject uiController = null;
    [SerializeField]
    private UIManager uiManager;
    [SerializeField]
    private CardController cardController = null;
    [SerializeField]
    private Player player = null;

    [SerializeField]
    private GameObject bulletPrefab = null;

    public CardController CardController => cardController;

    public Player Player => player;
    private FloorManager floorManager => ServiceLocator.Instance.FloorManager;
    private EnemyManager enemyManager => ServiceLocator.Instance.EnemyManager;
    private ItemManager itemManager => ServiceLocator.Instance.ItemManager;

    private Vector2Int move = Vector2Int.zero;
    private GameStatus status = GameStatus.Wait;
    private bool isExecuteCommand = false;
    private bool isTurnMode = false;
    private Coroutine turnControll = null;

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

        uiManager.Initialize(floorManager, player,
            () => status = GameStatus.EnemyControll,
            TakeItem,
            ThrowItem,
            DropItem);
        uiManager.OnCloseMenu = () => status = GameStatus.PlayerControll;
        floorManager.Clear();
        floorManager.Create(20, 20, 4, false);

        turnControll = StartCoroutine(TurnControll());

        cardController.Player = player;
        cardController.Initialize();

        player.Initialize();
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        player.OnMoved += floorManager.OnMoveUnit;

        enemyManager.Initialize(player);
        itemManager.Initialize(150, 1, 5);
    }

    private int index = 0;
    private void Update()
    {
    }

    private void OpenDialog()
    {
        var dialog = ServiceLocator.Instance.DialogManager.Open<CommonDialog>();
        dialog.Initialize($"テスト{index}", $"テストダイアログ{index}", ("さらに開く", () => OpenDialog()), ("閉じる", () => ServiceLocator.Instance.DialogManager.Close(dialog)));
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
            if (ServiceLocator.Instance.DialogManager.Controll())
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

    private IEnumerator PlayerControll()
    {
        if (player.IsLockInput) yield break;

        if (InputUtility.Menu.IsTriggerd())
        {
            uiManager.OpenMenu(() => status = GameStatus.UIControll);
            status = GameStatus.Wait;
            yield break;
        }
        if (InputUtility.Wait.IsPressed()) isExecuteCommand = true;
        if (InputUtility.Up.IsPressed()) Up();
        else if (InputUtility.Down.IsPressed()) Down();
        if (InputUtility.Right.IsPressed()) Right();
        else if (InputUtility.Left.IsPressed()) Left();
        isTurnMode |= InputUtility.TurnMode.IsPressed();

        Card card = null;
        var handIndex = -1;
        if (InputUtility.One.IsPressed())
        {
            card = cardController.GetHandCard(0);
            handIndex = 0;
        }
        else if (InputUtility.Two.IsPressed())
        {
            card = cardController.GetHandCard(1);
            handIndex = 1;
        }
        else if (InputUtility.Three.IsPressed())
        {
            card = cardController.GetHandCard(2);
            handIndex = 2;
        }
        else if (InputUtility.Four.IsPressed())
        {
            card = cardController.GetHandCard(3);
            handIndex = 3;
        }

        if (card != null && card.CanUse())
        {
            status = GameStatus.Wait;
            card.Use(() =>
            {
                status = GameStatus.EnemyControll;
                cardController.Use(handIndex);
            });
            yield break;
        }

        if (move.x != 0 || move.y != 0)
        {
            var currentPosition = player.Position;
            var destPosition = currentPosition + move;
            var destTile = floorManager.GetTile(destPosition);
            var enemy = floorManager.GetUnit(destPosition);
            if (destTile.IsWall || isTurnMode || enemy != null)
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

    private void DropItem(ItemBase target)
    {
        itemManager.Drop(target, 0, Player.Position);
        status = GameStatus.EnemyControll;
    }

    public GameObject CreateBullet(Vector3 position, Quaternion rotation)
    {
        var bullet = Instantiate(bulletPrefab, floorManager.transform);
        bullet.transform.localScale = Vector3.one;
        bullet.transform.localPosition = position;
        bullet.transform.localRotation = rotation;
        return bullet;
    }

    private void Shoot(int baseAttack, int range, Unit attacker)
    {
        var target = floorManager.GetHitPosition(attacker.Position, attacker.Angle, range);
        var bullet = Instantiate(bulletPrefab, attacker.transform.parent);
        bullet.transform.localPosition = attacker.transform.localPosition;
        bullet.transform.localScale = Vector3.one;
        var tween = bullet.transform
            .DOLocalMove(new Vector3(target.position.x, 0.5f, target.position.y), 0.1f * target.length)
            .SetEase(Ease.Linear)
            .Play();
        tween.onComplete += () =>
        {
            Destroy(bullet);
            var enemy = target.enemy;
            status = GameStatus.EnemyControll;
            if (enemy == null) return;
            var damage = 0;
            if (attacker is Player player)
            {
                damage = DamageUtil.GetDamage(player, baseAttack, enemy);
                enemy.Damage(damage, attacker);
            }
        };
        status = GameStatus.Wait;
    }

    private void ThrowItem(ItemBase target)
    {
        var item = itemManager.Drop(target, 0, Player.Position);
        var targetPosition = floorManager.GetHitPosition(player.Position, player.Angle, 10);
        var tween = item.transform
            .DOLocalMove(new Vector3(targetPosition.position.x, 0, targetPosition.position.y), 0.1f * targetPosition.length)
            .SetEase(Ease.Linear)
            .Play();
        tween.onComplete += () =>
        {
            floorManager.RemoveItem(item.Position);
            // 当たる位置にドロップできない場合は周囲からドロップできる場所を探す
            if (targetPosition.enemy != null)
            {
                var enemy = targetPosition.enemy;
                if (target is WeaponData weapon)
                    enemy.Damage(DamageUtil.GetDamage(player, weapon.Atk), player);
                else if (target is ShieldData shield)
                    enemy.Damage(DamageUtil.GetDamage(player, shield.Def), player);
                // 消費アイテムを投げつけた場合は、強制的にその効果を発動させる
                else if (target is UsableItemData usableItem)
                    usableItem.Use(enemy);
                itemManager.Despawn(item);
                status = GameStatus.EnemyControll;
                return;
            }
            if (!floorManager.CanDrop(targetPosition.position))
            {
                // 周囲のドロップできる場所を検索
                var candidate = floorManager.GetCanDropTile(targetPosition.position);
                // 候補あり
                if (candidate != null)
                {
                    // ドロップアニメ
                    Debug.LogError(candidate.Position);
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
            Debug.LogError($"ジェムを{item.GemCount}個拾った");
        }
        else
        {
            player.Data.TakeItem(item.Data);
            Debug.LogError($"{item.Data.Name}を拾った");
        }
        floorManager.RemoveItem(item.Position);
        ServiceLocator.Instance.ItemManager.Despawn(item);
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
