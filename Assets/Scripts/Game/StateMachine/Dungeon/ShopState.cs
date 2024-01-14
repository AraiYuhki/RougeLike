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
        if (InputUtility.Right.IsTrigger()) window.Right();
        else if (InputUtility.Left.IsTrigger()) window.Left();
        else if (InputUtility.Up.IsTrigger()) window.Up();
        else if (InputUtility.Down.IsTrigger()) window.Down();
        else if (InputUtility.Submit.IsTrigger()) window.Submit();
        else if (InputUtility.Cancel.IsTrigger())
        {
            stateMachine.Goto(GameState.Wait);
            window.Close();
        }
    }
}
