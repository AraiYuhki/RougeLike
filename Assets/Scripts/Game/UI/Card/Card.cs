using Cysharp.Threading.Tasks;
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
    private FloorManager floorManager;
    private EnemyManager enemyManager;

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

    public CardInfo Data { get; private set; }
    public PassiveEffectInfo PassiveInfo { get; private set; }
    public Player Owner { get; private set; }

    public bool IsPassive => PassiveInfo != null;

    public void SetManager(FloorManager floorManager, EnemyManager enemyManager)
    {
        this.floorManager = floorManager;
        this.enemyManager = enemyManager;
    }

    public void SetInfo(CardInfo info, Player owner)
    {
        Data = info;
        if (info.IsPassive)
            PassiveInfo = DB.Instance.MPassiveEffect.GetById(info.PassiveEffectId).Clone();
        Owner = owner;
        label.text = info.Name;
    }

    public void Goto(Transform target, Action onComplete = null)
    {
        tween?.Kill();
        transform.SetParent(target);
        tween = DOTween.Sequence();
        tween.Append(transform.DOLocalMove(Vector3.zero, 0.3f));
        tween.Join(transform.DOScale(Vector3.one, 0.3f));
        tween.OnComplete(() => {
            tween = null;
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="onComplete"></param>
    /// <param name="enoughCost">Falseの場合、通常攻撃は威力半減、その他は不発扱いにする</param>
    /// <exception cref="NotImplementedException"></exception>
    public async void Use(Action onComplete = null, bool enoughCost = true)
    {
        if (IsPassive)
        {
            await MisFire(onComplete);
            return;
        }

        if (enoughCost)
        {
            switch (Data.Type)
            {
                case CardType.NormalAttack:
                    NormalAttack(onComplete, true, false);
                    break;
                case CardType.ResourceAttack:
                    NormalAttack(onComplete, true, true);
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
                case CardType.Charge:
                    Owner.Charge(Data.Param, onComplete);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return;
        }

        switch (Data.Type)
        {
            case CardType.NormalAttack:
                NormalAttack(onComplete, false, false);
                break;
            case CardType.ResourceAttack:
                NormalAttack(onComplete, false, true);
                break;
            default:
                await MisFire(onComplete);
                return;
        }

    }

    private void NormalAttack(Action onComplete, bool enoughCost, bool isResourceAttack)
    {
        var destPosition = Owner.Position + Owner.Angle;
        var target = floorManager.GetUnit(destPosition) as Enemy;
        var power = (int)(Data.Param * (Owner.ChargeStack + 1f));
        if (!enoughCost) power /= 2;
        if (target != null)
            Owner.Attack(power, target, () => onComplete?.Invoke(), isResourceAttack);
        else
            onComplete?.Invoke();
    }

    private void LongRangeAttack(Action onComplete = null)
    {
        Owner.Shoot((int)Data.Param, Data.Range, onComplete);
    }

    public bool CanUse()
    {
        if (IsPassive) return true;

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
            case CardType.Passive:
                return true;
            default:
                throw new NotImplementedException();
        }
    }

    private async UniTask MisFire(Action onComplete)
    {
        await UniTask.Yield();
        onComplete?.Invoke();

    }

    private bool CheckEnemyInAroundTile()
    {
        return floorManager.GetAroundTilesAt(Owner.Position).Select(tile => floorManager.GetUnit(tile.Position) != null).Any();
    }

    private bool CheckExistEnemySameRoom()
    {
        var currentTile = floorManager.GetTile(Owner.Position);
        if (!currentTile.IsRoom) return false;
        return enemyManager.Enemies
                .Select(enemy => floorManager.GetTile(enemy.Position))
                .Where(tile => tile.IsRoom).Any(tile => tile.Id == currentTile.Id);
    }

    private bool CheckEnemyInRange()
    {
        var target = floorManager.GetHitPosition(Owner.Position, Owner.Angle, Data.Range);
        return target.enemy != null;
    }

    private bool CheckEnemyInAttackArea()
    {
        foreach(var offset in Data.AttackAreaData.Data.Select(data => data.Offset))
        {
            var rotatedOffset = AttackAreaInfo.GetRotatedOffset(Owner.transform.localEulerAngles.y, offset);
            var position = Owner.Position + offset;
            var target = floorManager.GetUnit(position);
            if (target != null) return true;
        }
        return false;
    }

    private bool CheckEnemyInSameRoom()
    {
        var currenTile = floorManager.GetTile(Owner.Position);
        if (!currenTile.IsRoom) return false;
        return enemyManager.Enemies.Any(enemy =>
        {
            // 同じ部屋に敵が存在すればTrue
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
