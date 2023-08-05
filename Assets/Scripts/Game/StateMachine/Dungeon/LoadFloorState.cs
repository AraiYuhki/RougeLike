using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LoadFloorState : IState
{
    private Player player;
    private DungeonStateMachine stateMachine;
    private GameController gameController;
    private FloorManager floorManager;
    private FloorMoveView floorMoveView;

    private bool generatedFloor = false;

    public LoadFloorState(
        Player player,
        DungeonStateMachine stateMachine,
        GameController gameController,
        FloorManager floorManager,
        FloorMoveView floorMoveView)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.gameController = gameController;
        this.floorManager = floorManager;
        this.floorMoveView = floorMoveView;
    }
    public void OnEnter()
    {
        generatedFloor = false;

        floorMoveView.StartFadeOut(
            gameController.CurrentFloor,
            gameController.CurrentFloor + 1,
            floorManager.IsTower,
            OnFadeComplete,
            OnAnimationComplete
            );
    }

    private void OnFadeComplete()
    {
        floorManager.Clear();
        floorManager.Create(40, 40, 10, floorManager.IsTower);
        player.SetPosition(floorManager.FloorData.SpawnPoint);
        gameController.ForceUpdateMinimap();
        generatedFloor = true;
    }

    private async void OnAnimationComplete()
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        await UniTask.WaitUntil(() => stopWatch.Elapsed.TotalSeconds > 0.5f && generatedFloor);
        floorMoveView.StartFadeIn(() => stateMachine.Goto(GameState.PlayerTurn));
    }

    public void OnExit()
    {

    }

    public void Update()
    {

    }
}