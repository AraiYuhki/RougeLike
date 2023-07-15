using Cysharp.Threading.Tasks;

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
