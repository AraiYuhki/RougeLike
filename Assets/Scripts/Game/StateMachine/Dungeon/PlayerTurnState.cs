using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class PlayerTurnState : IState
{
    private Player player;
    private DungeonStateMachine stateMachine;
    private FloorManager floorManager;
    private ItemManager itemManager;
    private DialogManager dialogManager;
    private Minimap minimap;
    private NoticeGroup notice;
    private CardController cardController;

    public PlayerTurnState(
        DungeonStateMachine stateMachine,
        FloorManager floorManager,
        ItemManager itemManager,
        DialogManager dialogManager,
        Minimap minimap,
        NoticeGroup notice,
        Player player,
        CardController cardController)
    {
        this.stateMachine = stateMachine;
        this.floorManager = floorManager;
        this.itemManager = itemManager;
        this.dialogManager = dialogManager;
        this.minimap = minimap;
        this.notice = notice;
        this.player = player;
        this.cardController = cardController;
    }


    public void OnEnter()
    {
        minimap.UpdateView();
    }

    public void OnExit()
    {
        player.TurnEnd();
    }

    public async void Update()
    {
        if (player.IsLockInput) return;
        if (InputUtility.Menu.IsTriggerd())
        {
            stateMachine.Goto(GameState.MainMenu);
            return;
        }

        if (UseCard())
        {
            minimap.UpdateView();
            return;
        }
        if (InputUtility.Wait.IsPressed())
        {
            stateMachine.Goto(GameState.EnemyTurn);
            return;
        }
        var move = Vector2Int.zero;
        if (InputUtility.Up.IsPressed()) move.y = 1;
        else if (InputUtility.Down.IsPressed()) move.y = -1;
        if (InputUtility.Right.IsPressed()) move.x = 1;
        else if (InputUtility.Left.IsPressed()) move.x = -1;
        var isTurnMode = InputUtility.TurnMode.IsPressed();
        if (InputUtility.DiagonalMode.IsPressed() && (move.x == 0 || move.y == 0))
            move = Vector2Int.zero;
        if (move == Vector2Int.zero) return;

        var currentPosition = player.Position;
        var destPosition = currentPosition + move;
        var destTile = floorManager.GetTile(destPosition);
        var enemy = floorManager.GetUnit(destPosition);
        if (enemy != null || destTile.IsWall || isTurnMode)
        {
            player.SetDestAngle(move);
            return;
        }
        stateMachine.Goto(GameState.Wait);
        player.MoveAsync(move, player.GetCancellationTokenOnDestroy()).Forget();
        TakeItem();
        await CheckTrapAsync();
        if (!CheckStair())
        {
            stateMachine.Goto(GameState.EnemyTurn);
        }
        minimap.UpdateView();
    }

    private bool UseCard()
    {
        Card card = null;
        var handIndex = -1;
        if (InputUtility.One.IsTriggerd())
        {
            card = cardController.GetHandCard(0);
            handIndex = 0;
        }
        else if (InputUtility.Two.IsTriggerd())
        {
            card = cardController.GetHandCard(1);
            handIndex = 1;
        }
        else if (InputUtility.Three.IsTriggerd())
        {
            card = cardController.GetHandCard(2);
            handIndex = 2;
        }
        else if (InputUtility.Four.IsTriggerd())
        {
            card = cardController.GetHandCard(3);
            handIndex = 3;
        }
        if (card != null && card.CanUse())
        {
            stateMachine.Goto(GameState.Wait);
            card.Use(() =>
            {
                cardController.Use(handIndex);
                stateMachine.Goto(GameState.EnemyTurn);
            });
            return true;
        }
        return false;
    }

    private void TakeItem()
    {
        var item = floorManager.GetItem(player.Position);
        if (item == null) return;
        player.PlayerData.Gems += item.GemCount;
        notice.Add($"ジェムを{item.GemCount}個拾った", Color.cyan);
        floorManager.RemoveItem(item.Position);
        itemManager.Despawn(item);
        stateMachine.Goto(GameState.EnemyTurn);
    }

    private async UniTask CheckTrapAsync()
    {
        var trap = floorManager.GetTrap(player.Position);
        if (trap == null)
        {
            await UniTask.Yield();
            return;
        }
        stateMachine.Goto(GameState.Wait);
        await trap.ExecuteAsync(player, player.GetCancellationTokenOnDestroy());
        await UniTask.Yield();
    }

    private bool CheckStair()
    {
        var stairPosition = floorManager.FloorData.StairPosition;
        if (stairPosition.X != player.Position.x || stairPosition.Y != player.Position.y) return false;
        stateMachine.OpenCommonDialog("確認", "次の階へ進みますか？",
            ("はい", () => stateMachine.Goto(GameState.Shop)),
            ("いいえ", () => stateMachine.Goto(GameState.EnemyTurn))
            );
        return true;
    }
}
