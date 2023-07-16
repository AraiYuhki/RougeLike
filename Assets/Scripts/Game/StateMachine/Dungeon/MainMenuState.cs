using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuState : IState
{
    private DungeonStateMachine stateMachine;
    private MenuUI menuUI;
    private MainMenuUI mainMenuUI;

    private bool isOpened = false;

    public MainMenuState(DungeonStateMachine stateMachine, MainMenuUI mainMenuUI, MenuUI menuUI)
    {
        this.stateMachine = stateMachine;
        this.mainMenuUI = mainMenuUI;
        this.menuUI = menuUI;
    }

    public void OnEnter()
    {
        menuUI.Open(() => isOpened = true);
    }

    public void OnExit()
    {
        isOpened = false;
        menuUI.Close();
    }

    public void Update()
    {
        if (!isOpened) return;
        if (InputUtility.Menu.IsTriggerd() || InputUtility.Cancel.IsTriggerd())
        {
            stateMachine.Goto(GameState.PlayerTurn);
            return;
        }
        if (InputUtility.Up.IsTriggerd())
            mainMenuUI.Up();
        else if (InputUtility.Down.IsTriggerd())
            mainMenuUI.Down();
        else if (InputUtility.Submit.IsTriggerd())
            mainMenuUI.Submit();
    }

    private void OpenInventory()
    {

    }

    private void CheckStep()
    {

    }

    private void Suspend()
    {

    }

    private void Retire()
    {

    }
}
