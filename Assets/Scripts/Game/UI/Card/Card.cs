using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField]
    private GameObject frontSide;
    [SerializeField]
    private GameObject backSide;
    [SerializeField]
    private TMP_Text label;
    [SerializeField]
    private Image illust;
    [SerializeField]
    private bool visibleFrontSide = false;

    private Sequence tween = null;

    public bool VisibleFrontSide
    {
        get => visibleFrontSide;
        set
        {
            visibleFrontSide = value;
            frontSide.SetActive(value);
            backSide.SetActive(!value);
        }
    }

    public CardData Data { get; private set; }
    public Player Owner { get; private set; }

    public void SetData(CardData data, Player owner)
    {
        Data = data;
        Owner = owner;
        label.text = data.Name;
    }

    public void Goto(Transform target, Action onComplete = null)
    {
        tween?.Kill();
        transform.parent = target;
        tween = DOTween.Sequence();
        tween.Append(transform.DOLocalMove(Vector3.zero, 0.3f));
        tween.Join(transform.DOScale(Vector3.one, 0.3f));
        tween.OnComplete(() => {
            tween = null;
            onComplete?.Invoke();
        });
    }

    public void Use(Action onComplete = null)
    {
        switch (Data.Type)
        {
            case CardType.NormalAttack:
                NormalAttack(onComplete);
                break;
            case CardType.Heal:
                Owner.Heal(Data.Param);
                onComplete?.Invoke();
                break;
            case CardType.StaminaHeal:
                Owner.RecoveryStamina(Data.Param);
                onComplete?.Invoke();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void NormalAttack(Action onComplete = null)
    {
        var destPosition = Owner.Position + Owner.Angle;
        var target = ServiceLocator.Instance.FloorManager.GetUnit(destPosition) as Enemy;
        if (target != null)
            Owner.Attack((int)Data.Param, target, () => onComplete?.Invoke());
        else
            onComplete?.Invoke();
    }

    public bool CanUse()
    {
        switch (Data.Type)
        {
            case CardType.NormalAttack:
                return CheckExistEnemySameRoom();
            case CardType.Heal:
                return Owner.Hp < Owner.MaxHp;
            case CardType.StaminaHeal:
                if (Owner is Player player)
                    return player.Data.Stamina < player.Data.MaxStamina;
                return false;
            default:
                throw new NotImplementedException();
        }
    }

    private bool CheckExistEnemySameRoom()
    {
        var floorManager = ServiceLocator.Instance.FloorManager;
        var currentTile = floorManager.GetTile(Owner.Position);
        if (currentTile.IsRoom)
        {
            return ServiceLocator.Instance.EnemyManager.Enemies
                .Select(enemy => floorManager.GetTile(enemy.Position))
                .Where(tile => tile.IsRoom).Any(tile => tile.Id == currentTile.Id);
        }
        var aroundTiles = floorManager.GetAroundTilesAt(Owner.Position);
        return aroundTiles.Select(tile => floorManager.GetUnit(tile.Position) != null).Any();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        frontSide?.SetActive(visibleFrontSide);
        backSide?.SetActive(!visibleFrontSide);
    }
#endif

}
