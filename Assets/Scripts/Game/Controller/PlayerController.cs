using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : IControllable
{
    protected GameController gameController;
    private static ItemManager itemManager => ServiceLocator.ItemManager;
    private static FloorManager floorManager => ServiceLocator.FloorManager;

    private Player player;
    private UIManager uiManager;
    private bool isTurnMode = false;
    private bool isExecuteCommand = false;

    private Vector2Int move = Vector2Int.zero;

    public PlayerController(Player player, UIManager uiManager, GameController gameController)
    {
        this.player = player;
        this.uiManager = uiManager;
        this.gameController = gameController;
    }

    private void OpenMenu()
    {
        uiManager.OpenMenu(() => gameController.SetStatus(GameStatus.UIControll));
    }

    private void Attack(Unit target, Vector2Int move)
    {
        player.SetDestAngle(move);
        move = Vector2Int.zero;
        player.Attack(target, () => gameController.SetStatus(GameStatus.EnemyControll));
    }

    public void Up() => move.y = 1;
    public void Down() => move.y = -1;
    public void Right() => move.x = 1;
    public void Left() => move.x = -1;
    public void Wait() => isExecuteCommand = true;
    public void TurnMode() => isTurnMode = true;


    public IEnumerator Controll()
    {
        if (player.IsLockInput) yield break;

        if (InputUtility.Menu.IsTriggerd())
        {
            OpenMenu();
            yield break;
        }
        if (InputUtility.Wait.IsPressed()) isExecuteCommand = true;
        if (InputUtility.Up.IsPressed()) Up();
        else if (InputUtility.Down.IsPressed()) Down();
        if (InputUtility.Right.IsPressed()) Right();
        else if (InputUtility.Left.IsPressed()) Left();
        isTurnMode |= InputUtility.TurnMode.IsPressed();

        if (move.x != 0 || move.y != 0)
        {
            var destPosition = player.Position + move;
            var destTile = floorManager.GetTile(destPosition);
            var enemy = floorManager.GetUnit(destPosition);
            if (enemy != null)
            {
                Attack(enemy, move);
                yield break;
            }
            if (destTile.IsWall || isTurnMode)
            {
                player.SetDestAngle(move);
            }
            else
            {
                player.Move(move);
                TakeItem();
                isExecuteCommand = true;
            }
        }
        isTurnMode = false;
        if (isExecuteCommand)
        {
            isExecuteCommand = false;
            gameController.SetStatus(GameStatus.EnemyControll);
        }
        move = Vector2Int.zero;
        yield return null;
    }

    public void TakeItem()
    {
        var item = floorManager.GetItem(player.Position);
        if (item == null) return;
        if (item.IsGem)
        {
            player.Data.Gems += item.GemCount;
            Debug.LogError($"ジェムを{item.GemCount}個拾った");
        }
        else
        {
            player.Data.TakeItem(item.Data);
            Debug.LogError($"{item.Data.Name}を拾った");
        }
        floorManager.RemoveItem(item.Position);
        itemManager.Despawn(item);
    }

    public void DropItem(ItemBase target)
    {
        itemManager.Drop(target, 0, player.Position);
    }

    public void ThrowItem(ItemBase target)
    {
        var item = itemManager.Drop(target, 0, player.Position);
        (int length, var position, var enemy) = floorManager.GetHitPosition(player.Position, player.Angle, 10);
        var tween = item.transform
            .DOLocalMove(new Vector3(position.x, 0, position.y), 0.1f * length)
            .SetEase(Ease.Linear)
            .Play();
        tween.onComplete += () => ThrownItem(target, item, position, enemy);
    }

    private void ThrownItem(ItemBase target, Item item, Vector2Int targetPosition, Enemy enemy)
    {
        floorManager.RemoveItem(item);
        // 当たる位置にドロップできない場合は周囲からドロップ可能な場所を探す
        if (enemy != null)
        {
            if (target is WeaponData weapon)
                enemy.Damage(DamageUtil.GetDamage(player, weapon.Atk), player);
            else if (target is ShieldData shield)
                enemy.Damage(DamageUtil.GetDamage(player, shield.Def), player);
            // 消費アイテムを投げつけた場合は、強制的にその効果を発動させる
            else if (target is UsableItemData usableItem)
                usableItem.Use(enemy);
            itemManager.Despawn(item);
            gameController.SetStatus(GameStatus.EnemyControll);
            return;
        }

        if (!floorManager.CanDrop(targetPosition))
        {
            // 周囲のドロップできる場所を検索
            var candidate = floorManager.GetCanDropTile(targetPosition);
            // 候補あり
            if (candidate != null)
            {
                var dropTween = item.transform
                .DOLocalMove(new Vector3(candidate.Position.X, 0f, candidate.Position.Y), 0.5f)
                .SetEase(Ease.OutExpo)
                .OnComplete(() =>
                {
                    floorManager.SetItem(item, candidate.Position);
                    gameController.SetStatus(GameStatus.EnemyControll);
                })
                .Play();
                return;
            }
            // 候補がないので消滅
            itemManager.Despawn(item);
            return;
        }
        floorManager.SetItem(item, targetPosition);
        gameController.SetStatus(GameStatus.EnemyControll);
    }
}
