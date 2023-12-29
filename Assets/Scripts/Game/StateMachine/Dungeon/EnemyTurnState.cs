using Cysharp.Threading.Tasks;

public class EnemyTurnState : IState
{
    private DungeonStateMachine stateMachine;
    private EnemyManager enemyManager;

    public EnemyTurnState(DungeonStateMachine stateMachine, EnemyManager enemyManager)
    {
        this.stateMachine = stateMachine;
        this.enemyManager = enemyManager;
    }

    public void OnEnter()
    {
        enemyManager.Controll(stateMachine).Forget();
    }

    public void OnExit()
    {
    }

    public void Update()
    {
    }
}
