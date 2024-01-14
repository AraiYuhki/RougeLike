using TMPro;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    [SerializeField]
    private TMP_Text titleLogo;

    [SerializeField]
    private VerticalMenu mainMenu = null;

    [SerializeField]
    private TitleMenuItem menuItemPrefab;

    [SerializeField]
    private DialogManager dialogManager;

    private TitleStateMachine stateMachine;

    private void Awake()
    {
        stateMachine = new TitleStateMachine();
        stateMachine.AddState(TitleState.Wait, new WaitState());
        stateMachine.AddState(TitleState.MainMenu, new TitleMenuState(stateMachine, titleLogo, mainMenu, menuItemPrefab));
        stateMachine.AddState(TitleState.Config, new ConfigState(stateMachine, dialogManager));
        stateMachine.AddState(TitleState.Dialog, new DialogState());
        stateMachine.Goto(TitleState.MainMenu);
    }

    private void Update()
    {
        stateMachine.Update();
    }
}
