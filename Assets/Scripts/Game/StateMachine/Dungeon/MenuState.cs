using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuState : IState
{
    private DungeonStateMachine stateMachine;
    private UIManager uiManager;
    public MenuState(DungeonStateMachine stateMachine, UIManager uiManager)
    {
        this.stateMachine = stateMachine;
        this.uiManager = uiManager;
        uiManager.Initialize();
    }
    public void OnEnter()
    {
        uiManager.OpenMainMenu();
    }

    public void OnExit()
    {
        if(uiManager.IsOpened) uiManager.CloseAll();
    }

    public void Update()
    {
        if (!uiManager.IsOpened)
        {
            stateMachine.Goto(GameState.PlayerTurn);
            return;
        }
        uiManager.UpdateUI();
        if (InputUtility.Menu.IsTriggerd())
            stateMachine.Goto(GameState.PlayerTurn);
    }
}
