using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum GameState
{
    Wait,
    PlayerTurn,
    EnemyTurn,
    UIControll,
    MainMenu,
    NextFloorLoad,
    TurnEnd,
}

public class DungeonStateMachine
{
    private GameState currentState = GameState.Wait;
    private IState current => states.ContainsKey(currentState) ? states[currentState] : null;
    private Dictionary<GameState, IState> states = new Dictionary<GameState, IState>();

    public void AddState(GameState state, IState instance) => states[state] = instance;

    public void Goto(GameState state)
    {
        current?.OnExit();
        currentState = state;
        current?.OnEnter();
    }

    public IEnumerator Update()
    {
        while (true)
        {
            if (current != null)
            {
                current.Update();
                yield return null;
            }
        }
    }
}
