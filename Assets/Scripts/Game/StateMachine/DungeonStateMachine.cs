using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum GameState
{
    Wait,
    PlayerTurn,
    EnemyTurn,
    UIControll,
    MainMenu,
    NextFloorLoad,
    TurnEnd,
}

public class DungeonStateMachine
{
    private GameState currentState = GameState.Wait;
    private IState current => states.ContainsKey(currentState) ? states[currentState] : null;
    private Dictionary<GameState, IState> states = new Dictionary<GameState, IState>();

    public void AddState(GameState state, IState instance) => states[state] = instance;

    public void Goto(GameState state)
    {
        current?.OnExit();
        currentState = state;
        current?.OnEnter();
    }

    public IEnumerator Update()
    {
        while (true)
        {
            if (current != null)
            {
                current.Update();
                yield return null;
            }
        }
    }
}

public class PlayerTurnState : IState
{
    private Player player;
    private DungeonStateMachine stateMachine;
    private FloorManager floorManager;
    private ItemManager itemManager;
    private DialogManager dialogManager;
    private NoticeGroup notice;

    public PlayerTurnState(
        DungeonStateMachine stateMachine,
        FloorManager floorManager,
        ItemManager itemManager,
        DialogManager dialogManager,
        NoticeGroup notice,
        Player player)
    {
        this.stateMachine = stateMachine;
        this.floorManager = floorManager;
        this.itemManager = itemManager;
        this.dialogManager = dialogManager;
        this.notice = notice;
        this.player = player;
    }


    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Update()
    {
        if (player.IsLockInput) return;
        if (InputUtility.Menu.IsTriggerd())
        {
            stateMachine.Goto(GameState.MainMenu);
            return;
        }
        var isExecuteCommand = false;
        var move = Vector2Int.zero;
        if (InputUtility.Wait.IsPressed()) isExecuteCommand = true;
        if (InputUtility.Up.IsPressed()) move.y = 1;
        else if (InputUtility.Down.IsPressed()) move.y = -1;
        if (InputUtility.Right.IsPressed()) move.x = 1;
        else if (InputUtility.Left.IsPressed()) move.x = -1;
        var isTurnMode = InputUtility.TurnMode.IsPressed();

        if (move.x != 0 || move.y != 0)
        {
            var currentPosition = player.Position;
            var destPosition = currentPosition + move;
            var destTile = floorManager.GetTile(destPosition);
            var enemy = floorManager.GetUnit(destPosition);
            if (enemy != null)
            {
                stateMachine.Goto(GameState.Wait);
                player.SetDestAngle(move);
                move = Vector2Int.zero;
                player.Attack(enemy, () => stateMachine.Goto(GameState.EnemyTurn));
                return;
            }
            if (destTile.IsWall || isTurnMode)
            {
                player.SetDestAngle(move);
            }
            else
            {
                player.Move(move);
                TakeItem();
                // 移動先が階段か確認する
                isExecuteCommand = !CheckStair();
            }
        }
        if (isExecuteCommand)
        {
            isExecuteCommand = false;
            stateMachine.Goto(GameState.EnemyTurn);
        }
    }

    private void TakeItem()
    {
        var item = floorManager.GetItem(player.Position);
        if (item == null) return;
        if (item.IsGem)
        {
            player.Data.Gems += item.GemCount;
            notice.Add($"ジェムを{item.GemCount}個拾った", Color.cyan);
        }
        else
        {
            player.Data.TakeItem(item.Data);
            notice.Add($"{item.Data.Name}を拾った", Color.cyan);
        }
        floorManager.RemoveItem(item.Position);
        itemManager.Despawn(item);
        stateMachine.Goto(GameState.EnemyTurn);
    }

    private bool CheckStair()
    {
        var stairPosition = floorManager.FloorData.StairPosition;
        if (stairPosition.X == player.Position.x && stairPosition.Y == player.Position.y)
        {
            var dialog = dialogManager.Open<CommonDialog>();
            dialog.Initialize("確認", "次の階へ進みますか？",
                ("はい", () =>
                {
                    dialogManager.Close(dialog);
                    stateMachine.Goto(GameState.Wait);
                    Fade.Instance.FadeOut(() =>
                    {
                        stateMachine.Goto(GameState.NextFloorLoad);
                    });
                }
            ),
                ("いいえ", () =>
                {
                    dialogManager.Close(dialog);
                    stateMachine.Goto(GameState.EnemyTurn);
                }
            ));
            return true;
        }
        return false;
    }
}

public class EnemyTurnState : IState
{
    private DungeonStateMachine stateMachine;
    private EnemyManager enemyManager;
    private UniTask task;

    public EnemyTurnState(DungeonStateMachine stateMachine, EnemyManager enemyManager)
    {
        this.stateMachine = stateMachine;
        this.enemyManager = enemyManager;
    }

    public void OnEnter()
    {
        task = enemyManager.Controll();
    }

    public void OnExit()
    {
    }

    public void Update()
    {
        if (task.Status == UniTaskStatus.Succeeded)
            stateMachine.Goto(GameState.PlayerTurn);
    }
}

public class WaitState : IState
{
    public void OnEnter()
    {
    }

    public void OnExit()
    {
    }

    public void Update()
    {
    }
}
