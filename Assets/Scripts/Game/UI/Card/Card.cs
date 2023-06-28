using DG.Tweening;
using System;
using System.Linq;
using TMPro;
using UnityEditor;
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
            case CardType.LongRangeAttack:
                LongRangeAttack(onComplete);
                break;
            case CardType.RangeAttack:
                Owner.Attack((int)Data.Param, Data.AttackAreaData, onComplete);
                break;
            case CardType.RoomAttack:
                Owner.RoomAttack((int)Data.Param, onComplete);
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

    private void LongRangeAttack(Action onComplete = null)
    {
        Owner.Shoot((int)Data.Param, Data.Range, onComplete);
    }

    public bool CanUse()
    {
        switch (Data.Type)
        {
            case CardType.NormalAttack:
            case CardType.ResourceAttack:
                return CheckEnemyInAroundTile() || CheckExistEnemySameRoom();
            case CardType.LongRangeAttack:
                return CheckEnemyInRange() || CheckEnemyInAroundTile();
            case CardType.RangeAttack:
                return CheckEnemyInAroundTile() || CheckEnemyInAttackArea();
            case CardType.RoomAttack:
                return CheckEnemyInAroundTile() || CheckEnemyInSameRoom();
            case CardType.Heal:
                return Owner.Hp < Owner.MaxHp;
            case CardType.StaminaHeal:
                if (Owner is Player player)
                    return player.Data.Stamina < player.Data.MaxStamina;
                return false;
            case CardType.Charge:
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    private bool CheckEnemyInAroundTile()
    {
        var floorManager = ServiceLocator.Instance.FloorManager;
        return floorManager.GetAroundTilesAt(Owner.Position).Select(tile => floorManager.GetUnit(tile.Position) != null).Any();
    }

    private bool CheckExistEnemySameRoom()
    {
        var floorManager = ServiceLocator.Instance.FloorManager;
        var currentTile = floorManager.GetTile(Owner.Position);
        if (!currentTile.IsRoom) return false;
        return ServiceLocator.Instance.EnemyManager.Enemies
                .Select(enemy => floorManager.GetTile(enemy.Position))
                .Where(tile => tile.IsRoom).Any(tile => tile.Id == currentTile.Id);
    }

    private bool CheckEnemyInRange()
    {
        var target = ServiceLocator.Instance.FloorManager.GetHitPosition(Owner.Position, Owner.Angle, Data.Range);
        return target.enemy != null;
    }

    private bool CheckEnemyInAttackArea()
    {
        foreach(var offset in Data.AttackAreaData.Data.Select(data => data.Offset - Data.AttackAreaData.Center))
        {
            var rotatedOffset = AttackAreaData.GetRotatedOffset(Owner.transform.localEulerAngles.y, offset);
            var position = Owner.Position + offset;
            var target = ServiceLocator.Instance.FloorManager.GetUnit(position);
            if (target != null) return true;
        }
        return false;
    }

    private bool CheckEnemyInSameRoom()
    {
        var floorManager = ServiceLocator.Instance.FloorManager;
        var currenTile = floorManager.GetTile(Owner.Position);
        if (!currenTile.IsRoom) return false;
        return ServiceLocator.Instance.EnemyManager.Enemies.Any(enemy =>
        {
            // ìØÇ∂ïîâÆÇ…ìGÇ™ë∂ç›Ç∑ÇÍÇŒTrue
            var tile = floorManager.GetTile(enemy.Position);
            if (!tile.IsRoom) return false;
            return tile.Id == currenTile.Id;
        });
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        frontSide?.SetActive(visibleFrontSide);
        backSide?.SetActive(!visibleFrontSide);
    }
#endif

}
