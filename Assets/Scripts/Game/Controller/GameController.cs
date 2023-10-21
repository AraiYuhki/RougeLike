using TMPro;
using UnityEngine;

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

    [SerializeField]
    private DungeonInfo dungeonData = null;

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

    public string DungeonName => dungeonData.Name;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        stateMachine = new DungeonStateMachine();
        stateMachine.AddState(GameState.Wait, new WaitState());
        stateMachine.AddState(GameState.PlayerTurn, new PlayerTurnState(stateMachine, floorManager, itemManager, dialogManager, noticeGroup, player, cardController));
        stateMachine.AddState(GameState.EnemyTurn, new EnemyTurnState(stateMachine, enemyManager));
        stateMachine.AddState(GameState.MainMenu, new MenuState(stateMachine, uiManager));
        stateMachine.AddState(GameState.NextFloorLoad, new LoadFloorState(player, stateMachine, this, floorManager, floorMoveView));
        stateMachine.AddState(GameState.Dialog, new DialogState());
        stateMachine.AddState(GameState.Shop, new ShopState(stateMachine, shopWindow, floorManager));
    }

    private void Start()
    {
        dungeonData = DB.Instance.MDungeon.GetById(1);
        CurrentFloor = 1;
        var floorInfo = dungeonData.GetFloor(CurrentFloor);
        floorManager.Clear();
        floorManager.Create(floorInfo, dungeonData.IsTower);
        player.Initialize();
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        player.OnMoved += floorManager.OnMoveUnit;
        cardController.Player = player;
        cardController.Initialize();

        turnControll = StartCoroutine(stateMachine.Update());

        enemyManager.Initialize(player, floorInfo);
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

        var floorInfo = dungeonData.GetFloor(CurrentFloor);
        floorManager.Create(floorInfo, dungeonData.IsTower);
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        enemyManager.SetFloorData(floorInfo);
        itemManager.Initialize(150, 1, 5);
        for (var count = 0; count < floorInfo.InitialSpawnEnemyCount; count++)
            enemyManager.Spawn();
        ForceUpdateMinimap();
    }

    public void StartEnemyTurn() => stateMachine.Goto(GameState.EnemyTurn);

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
        floorManager.RemoveItem(item.Position);
        itemManager.Despawn(item);
        StartEnemyTurn();
    }

    public void ForceUpdateMinimap() => uiManager.ForceUpdateMinimap();
}
