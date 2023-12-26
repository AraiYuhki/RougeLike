﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public enum GameState
{
    Wait,
    PlayerTurn,
    EnemyTurn,
    UIControll,
    TurnEnd,
    MainMenu,
    NextFloorLoad,
    Dialog,
    Shop,
}

public class DungeonStateMachine : IDungeonStateMachine
{
    private GameState currentState = GameState.Wait;

    private GameState prevState = GameState.Wait;
    private IState current => states.ContainsKey(currentState) ? states[currentState] : null;
    private Dictionary<GameState, IState> states = new Dictionary<GameState, IState>();

    public void AddState(GameState state, IState instance) => states[state] = instance;

    public void Goto(GameState state)
    {
        current?.OnExit();
        currentState = state;
        prevState = currentState;
        current?.OnEnter();
    }

    public void GotoPrev()
    {
        Goto(prevState);
        prevState = GameState.Wait;
    }

    public void OpenCommonDialog(string title, string message, params (string label, Action onClick)[] data)
    {
        prevState = currentState;
        var dialog = DialogManager.Instance.Create<CommonDialog>();
        dialog.Initialize(title, message, data);
        dialog.Open(() =>
        {
            currentState = GameState.Dialog;
            if (current is DialogState dialogState)
                dialogState?.OnEnter(dialog);
            else
                throw new Exception("Can't enter dialog state");
        });
        currentState = GameState.Wait;
    }

    public IEnumerator Update()
    {
        while (true)
        {
            if (current != null)
            {
                current.Update();
            }
            yield return null;
        }
    }
}
