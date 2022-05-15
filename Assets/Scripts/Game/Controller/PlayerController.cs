using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Player player;

    /// <summary>
    /// 方向転換のみ行う？
    /// </summary>
    private bool isTurnMode = false;
    /// <summary>
    /// コマンドが実行されたか
    /// </summary>
    private bool isExecuteCommand = false;
    private FloorManager floor;
    public void SetFloor(FloorManager floor) => this.floor = floor;
    public void Spawn(Vector2Int position) => player.SetPosition(position);
    public Player Player => player;

    private Vector2Int move = Vector2Int.zero;

    public void Up() => move.y = 1;
    public void Down() => move.y = -1;
    public void Right() => move.x = 1;
    public void Left() => move.x = -1;
    public void TurnMode() => isTurnMode = true;
    public void Wait() => isExecuteCommand = true;

    public IEnumerator Controll()
    {
        while (true)
        {
            if (player.IsLockInput)
            {
                yield return null;
                continue;
            }
            if (Input.GetKey(KeyCode.Space)) isExecuteCommand = true;
            if (Input.GetKey(KeyCode.W)) Up();
            else if (Input.GetKey(KeyCode.S)) Down();
            if (Input.GetKey(KeyCode.D)) Right();
            else if (Input.GetKey(KeyCode.A)) Left();
            isTurnMode = Input.GetKey(KeyCode.LeftShift);

            if (move.x != 0 || move.y != 0)
            {
                var currentPosition = player.Position;
                var destPosition = currentPosition + move;
                var destTile = floor.GetTile(destPosition);
                var enemy = floor.GetUnit(destPosition);
                if (enemy != null)
                {
                    var completeAttackAnimation = false;
                    player.SetDestAngle(move);
                    player.Attack(enemy, () => completeAttackAnimation = true);
                    while (!completeAttackAnimation) yield return new WaitForEndOfFrame();
                    move = Vector2Int.zero;
                    yield break;
                }
                if (destTile.IsWall || isTurnMode)
                {
                    player.SetDestAngle(move);
                }
                else
                {
                    player.Move(move);
                    isExecuteCommand = true;
                }
            }
            move = Vector2Int.zero;
            if (isExecuteCommand)
            {
                isExecuteCommand = false;
                yield break;
            }
            yield return null;
        }
    }
}
