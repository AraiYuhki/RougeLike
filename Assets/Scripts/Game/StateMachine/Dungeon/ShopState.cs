using Cysharp.Threading.Tasks;

public class ShopState : IState
{
    private DungeonStateMachine stateMachine;
    private ShopWindow window;
    private FloorManager floorManager;

    public ShopState(DungeonStateMachine stateMachine, ShopWindow window, FloorManager floorManager)
    {
        this.stateMachine = stateMachine;
        this.floorManager = floorManager;
        this.window = window;
        this.window.Initialize(stateMachine);
    } 

    public void OnEnter()
    {
        var shopSetting = DB.Instance.MFloorShop.GetById(floorManager.FloorInfo.ShopId);
        if (!window.IsOpen)
        {
            window.InitializeShop(shopSetting);
            window.InitializeDeck();
            window.Open().Forget();
        }
    }

    public void OnExit()
    {
    }

    public void Update()
    {
        if (InputUtility.Right.IsTriggerd()) window.Right();
        else if (InputUtility.Left.IsTriggerd()) window.Left();
        else if (InputUtility.Up.IsTriggerd()) window.Up();
        else if (InputUtility.Down.IsTriggerd()) window.Down();
        else if (InputUtility.Submit.IsTriggerd()) window.Submit();
        else if (InputUtility.Cancel.IsTriggerd())
        {
            stateMachine.Goto(GameState.Wait);
            window.Close();
        }
    }
}
