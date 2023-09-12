using UnityEngine;

public class PlayerTurnState : IState
{
    private Player player;
    private DungeonStateMachine stateMachine;
    private FloorManager floorManager;
    private ItemManager itemManager;
    private DialogManager dialogManager;
    private NoticeGroup notice;
    private CardController cardController;

    public PlayerTurnState(
        DungeonStateMachine stateMachine,
        FloorManager floorManager,
        ItemManager itemManager,
        DialogManager dialogManager,
        NoticeGroup notice,
        Player player,
        CardController cardController)
    {
        this.stateMachine = stateMachine;
        this.floorManager = floorManager;
        this.itemManager = itemManager;
        this.dialogManager = dialogManager;
        this.notice = notice;
        this.player = player;
        this.cardController = cardController;
    }


    public void OnEnter()
    {
    }

    public void OnExit()
    {
        player.TurnEnd();
    }

    public void Update()
    {
        if (player.IsLockInput) return;
        if (InputUtility.Menu.IsTriggerd())
        {
            stateMachine.Goto(GameState.MainMenu);
            return;
        }
        if (UseCard())
        {
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

        if (InputUtility.DiagonalMode.IsPressed())
        {
            if (move.x == 0 || move.y == 0)
                move = Vector2Int.zero;
        }

        if (move.x != 0 || move.y != 0)
        {
            var currentPosition = player.Position;
            var destPosition = currentPosition + move;
            var destTile = floorManager.GetTile(destPosition);
            var enemy = floorManager.GetUnit(destPosition);
            if (enemy != null || destTile.IsWall || isTurnMode)
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
        if (item.IsGem)
        {
            player.Data.Gems += item.GemCount;
            notice.Add($"ジェムを{item.GemCount}個拾った", Color.cyan);
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
            stateMachine.OpenCommonDialog("確認", "次の階へ進みますか？",
                ("はい", delegate () { stateMachine.Goto(GameState.Shop); }),
                ("いいえ", delegate () { stateMachine.Goto(GameState.EnemyTurn); })
                );
            return true;
        }
        return false;
    }
}
