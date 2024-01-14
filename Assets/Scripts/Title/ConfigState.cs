public class ConfigState : IState
{
    private TitleStateMachine stateMachine;
    private DialogManager dialogManager;

    private ConfigDialog dialog;

    public ConfigState(TitleStateMachine stateMachine, DialogManager dialogManager)
    {
        this.stateMachine = stateMachine;
        this.dialogManager = dialogManager;
    }

    public void OnEnter()
    {
        dialog = dialogManager.Create<ConfigDialog>();
        dialog.OnExit += () => stateMachine.Goto(TitleState.Wait);
        dialog.Open();
    }

    public void OnExit()
    {
        dialogManager.Close(dialog, () => stateMachine.Goto(TitleState.MainMenu));
        dialog = null;
    }

    public void Update()
    {
        dialog.Controll();
    }
}
