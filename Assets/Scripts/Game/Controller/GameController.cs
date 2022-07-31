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
    private MenuUI menuUI = null;
    [SerializeField]
    private Player player = null;

    public InGameInput GameInput { get; private set; }
    public Player Player => player;
    private FloorManager floorManager => ServiceLocator.Instance.FloorManager;
    private EnemyManager enemyManager => ServiceLocator.Instance.EnemyManager;
    private InventoryUI inventoryUI => menuUI.InventoryUI;
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
        GameInput = new InGameInput();
        GameInput.Enable();
        status = GameStatus.PlayerControll;
    }

    private void Start()
    {
        uiController.gameObject.SetActive(EnableUIControllerPlatforms.Contains(Application.platform));
        
        floorManager.Clear();
        floorManager.Create(20, 20, 4, false);
        player.Initialize();
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        player.OnMoved += floorManager.OnMoveUnit;

        turnControll = StartCoroutine(TurnControll());

        enemyManager.Initialize(player);
        menuUI.Initialize(player, GameInput);
    }

    private void OnDestroy()
    {
        GameInput.Disable();
        StopCoroutine(turnControll);
    }

    public IEnumerator TurnControll()
    {
        while (true)
        {
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

        if (GameInput.Player.Menu.WasPressedThisFrame())
        {
            menuUI.SwitchOpenMenu(() => status = GameStatus.UIControll);
            status = GameStatus.Wait;
            yield break;
        }
        if (GameInput.Player.Wait.IsPressed()) isExecuteCommand = true;
        if (GameInput.Player.Up.IsPressed()) Up();
        else if (GameInput.Player.Down.IsPressed()) Down();
        if (GameInput.Player.Right.IsPressed()) Right();
        else if (GameInput.Player.Left.IsPressed()) Left();
        isTurnMode |= GameInput.Player.TurnMode.IsPressed();

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

    private IEnumerator UIControll()
    {
        if (GameInput.Player.Menu.WasPressedThisFrame())
        {
            menuUI.SwitchOpenMenu(() => status = GameStatus.PlayerControll);
            status = GameStatus.Wait;
            yield return null;
            yield break;
        }

        yield return menuUI.Controll();
    }
}
