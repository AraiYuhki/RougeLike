using UnityEngine;

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
