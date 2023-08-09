using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopState : IState
{
    private DungeonStateMachine stateMachine;
    private ShopWindow window;

    public ShopState(DungeonStateMachine stateMachine, ShopWindow window)
    {
        this.stateMachine = stateMachine;
        this.window = window;
        this.window.StateMachine = stateMachine;
    } 

    public void OnEnter()
    {
        window.Open();
    }

    public void OnExit()
    {
        window.Close();
    }

    public void Update()
    {
        if (InputUtility.RightTrigger.IsTriggerd())     window.RightTrigger();
        else if (InputUtility.LeftTrigger.IsTriggerd()) window.LeftTrigger();
        else if (InputUtility.Right.IsTriggerd())       window.Right();
        else if (InputUtility.Left.IsTriggerd())        window.Left();
        else if (InputUtility.Up.IsTriggerd())          window.Up();
        else if (InputUtility.Down.IsTriggerd())        window.Down();
        else if (InputUtility.Submit.IsTriggerd())      window.Submit();
        else if(InputUtility.Cancel.IsTriggerd())       stateMachine.Goto(GameState.NextFloorLoad);
    }
}
