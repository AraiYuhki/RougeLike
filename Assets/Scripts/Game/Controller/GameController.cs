using NUnit.Framework;
using System;
using System.Collections.Generic;
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
    private TrapManager trapManager;
    [SerializeField]
    private DialogManager dialogManager;
    [SerializeField]
    private Minimap minimap;
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
        stateMachine.AddState(GameState.PlayerTurn, new PlayerTurnState(stateMachine, floorManager, itemManager, dialogManager, minimap, noticeGroup, player, cardController));
        stateMachine.AddState(GameState.EnemyTurn, new EnemyTurnState(stateMachine, enemyManager));
        stateMachine.AddState(GameState.MainMenu, new MenuState(stateMachine, uiManager));
        stateMachine.AddState(GameState.NextFloorLoad, new LoadFloorState(player, stateMachine, this, floorManager, floorMoveView));
        stateMachine.AddState(GameState.Dialog, new DialogState());
        stateMachine.AddState(GameState.Shop, new ShopState(stateMachine, shopWindow, floorManager));
    }

    private async void Start()
    {
        dungeonData = DB.Instance.MDungeon.GetById(1);
        CurrentFloor = 1;

        player.Initialize();
        player.OnMoved += floorManager.OnMoveUnit;
        cardController.Player = player;
        cardController.Initialize();

        turnControll = StartCoroutine(stateMachine.Update());

        var floorInfo = floorManager.Create(dungeonData, CurrentFloor);
        InitializeFloor(floorInfo);
        await Fade.Instance.FadeInAsync();
        stateMachine.Goto(GameState.PlayerTurn);
    }

    private void OnDestroy()
    {
        StopCoroutine(turnControll);
    }

    public void LoadNextFloor()
    {
        CurrentFloor++;
        Clear();

        var floorInfo = floorManager.Create(dungeonData, CurrentFloor);
        InitializeFloor(floorInfo);
    }

    private void InitializeFloor(FloorInfo floorInfo)
    {
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        enemyManager.Initialize(player, floorInfo);
        itemManager.Initialize(150, 1, 5);
        trapManager.Initialize(floorInfo);
        floorManager.CreateMesh();
        minimap.UpdateView();

        Save();
    }

    private void Clear()
    {
        enemyManager.Clear();
        itemManager.Clear();
        trapManager.Clear();
        floorManager.Clear();
    }

    public void Save()
    {
        var saveData = new SaveData(
            dungeonData.Id,
            CurrentFloor,
            player.Data,
            itemManager.ItemList,
            trapManager.TrapList,
            enemyManager.DataList,
            floorManager.FloorData,
            cardController,
            minimap.VisibleMap.ToArray()
            );
        DataBank.IsEncrypt = false;
        DataBank.Instance.Store("save", saveData);
        DataBank.Instance.SaveAll();
    }

    public void Load()
    {
        DataBank.IsEncrypt = false;
        if (!DataBank.Instance.Load<SaveData>("save"))
            throw new Exception("セーブデータのロードに失敗しました");

        Clear();
        var saveData = DataBank.Instance.Get<SaveData>("save");
        cardController.LoadFromJson(saveData);
        player.LoadFromJson(saveData.PlayerData);

        dungeonData = DB.Instance.MDungeon.GetById(saveData.DungeonId);
        CurrentFloor = saveData.CurrentFloor;

        floorManager.LoadFromJson(dungeonData.GetFloor(CurrentFloor), saveData.FloorData, dungeonData.IsTower);

        itemManager.LoadFromJson(saveData.Items);
        trapManager.LoadFromJson(saveData.Traps, floorManager.FloorInfo);
        enemyManager.LoadFromJson(saveData.Enemies);

        floorManager.CreateMesh();
        minimap.LoadFromJson(saveData.VisibleTiles);
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
        player.Data.Gems += item.GemCount;
        noticeGroup.Add($"ジェムを{item.GemCount}個拾った", Color.cyan);
        floorManager.RemoveItem(item.Position);
        itemManager.Despawn(item);
        StartEnemyTurn();
    }

#if DEBUG
    public void Goto(GameState state) => stateMachine.Goto(state);
#endif
}
