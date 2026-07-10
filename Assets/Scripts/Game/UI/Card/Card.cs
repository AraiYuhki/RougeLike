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
    private TMP_Text titleLabel;
    [SerializeField]
    private TMP_Text detailLabel;
    [SerializeField]
    private Image illust;
    [SerializeField]
    private bool visibleFrontSide = false;
    [SerializeField]
    private Animator _animator;

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

    public async UniTask OpenAsync() => await _animator.PlayAsync(AnimatorHash.Open);

    public async UniTask CloseAsync() => await _animator.PlayAsync(AnimatorHash.Close);

    public void SwitchSide(AnimationEvent e) => VisibleFrontSide = e.intParameter == 1;

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
        titleLabel.text = info.Name;
        detailLabel.text = info.Description;
        illust.sprite = info.Illust;
    }

    public void Goto(bool isOpen, Transform target)
    {
        tween?.Kill();
        transform.SetParent(target);
        tween = DOTween.Sequence();
        tween.Append(transform.DOLocalMove(Vector3.zero, 0.3f));
        tween.Join(transform.DOScale(Vector3.one, 0.3f));
        tween.OnComplete(() => tween = null);
        if (isOpen)
            OpenAsync().Forget();
        else
            CloseAsync().Forget();
    }

    public void Goto(Transform target)
    {
        tween?.Kill();
        transform.SetParent(target);
        tween = DOTween.Sequence();
        tween.Append(transform.DOLocalMove(Vector3.zero, 0.3f));
        tween.Join(transform.DOScale(Vector3.one, 0.3f));
        tween.OnComplete(() => tween = null);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="onComplete"></param>
    /// <param name="enoughCost">Falseの場合、通常攻撃は威力半減、その他は不発扱いにする</param>
    /// <exception cref="NotImplementedException"></exception>
    public async void Use(Action onComplete = null)
    {
        if (IsPassive)
        {
            await MisFire(onComplete);
            return;
        }

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
                Owner.Attack((int)Data.Param1, Data.AttackAreaData, onComplete);
                break;
            case CardType.RoomAttack:
                Owner.RoomAttack((int)Data.Param1, onComplete);
                break;
            case CardType.Heal:
                Owner.Heal(Data.Param1);
                onComplete?.Invoke();
                break;
            case CardType.StaminaHeal:
                Owner.RecoveryStamina(Data.Param1);
                onComplete?.Invoke();
                break;
            case CardType.Charge:
                Owner.Charge(Data.Param1, onComplete);
                break;
            case CardType.DrawAndUse:
                Owner.DrawAndUse((int)Data.Param1, Data.TargetCategory, onComplete);
                break;
            case CardType.PenetrateAttack:
                Owner.PenetrateShoot((int)Data.Param1, Data.Range, onComplete);
                break;
            case CardType.AilmentAttack:
                AilmentAttack(onComplete);
                break;
            case CardType.CureAilment:
                Owner.RecoverAilment();
                onComplete?.Invoke();
                break;
            case CardType.Redraw:
                Owner.Redraw(onComplete);
                break;
            case CardType.Swap:
                SwapPosition(onComplete);
                break;
            case CardType.KnockbackAttack:
                KnockbackAttack(onComplete);
                break;
            case CardType.PullAttack:
                PullAttack(onComplete);
                break;
            case CardType.FollowupAttack:
                FollowupAttack(onComplete);
                break;
            case CardType.PlaceTrap:
                Owner.PlaceTrap(Data.Param2, onComplete);
                break;
            case CardType.LastCardAttack:
                NormalAttack(onComplete, true, false);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void NormalAttack(Action onComplete, bool enoughCost, bool isResourceAttack)
    {
        var destPosition = Owner.Position + Owner.Angle;
        var target = floorManager.GetUnit(destPosition) as Enemy;
        var power = (int)(Data.Param1 * (Owner.ChargeStack + 1f));
        if (!enoughCost) power /= 2;
        if (target != null)
            Owner.Attack(power, target, () => onComplete?.Invoke(), isResourceAttack);
        else
            onComplete?.Invoke();
    }

    private void LongRangeAttack(Action onComplete = null)
    {
        Owner.Shoot((int)Data.Param1, Data.Range, onComplete);
    }

    /// <summary>
    /// 前方の敵にダメージを与え、生き残った場合は状態異常を付与する
    /// </summary>
    private void AilmentAttack(Action onComplete)
    {
        var target = floorManager.GetUnit(Owner.Position + Owner.Angle) as Enemy;
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }
        var power = (int)(Data.Param1 * (Owner.ChargeStack + 1f));
        Owner.Attack(power, target, () =>
        {
            if (target != null && target.Hp > 0)
                target.AddAilment(Data.Ailment, Data.AilmentParam, Data.AilmentTurn);
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// 前方の敵にダメージを与えて吹き飛ばす。壁や他のユニットに激突した場合は追加ダメージ
    /// </summary>
    private void KnockbackAttack(Action onComplete)
    {
        var target = floorManager.GetUnit(Owner.Position + Owner.Angle) as Enemy;
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }
        var power = (int)(Data.Param1 * (Owner.ChargeStack + 1f));
        Owner.Attack(power, target, async () =>
        {
            if (target == null || target.Hp <= 0)
            {
                onComplete?.Invoke();
                return;
            }
            await KnockbackAsync(target);
            onComplete?.Invoke();
        });
    }

    private async UniTask KnockbackAsync(Enemy target)
    {
        var direction = Owner.Angle;
        var position = target.Position;
        var isCrashed = false;
        for (var count = 0; count < Data.Param2; count++)
        {
            var next = position + direction;
            var tile = floorManager.GetTile(next);
            if (tile == null || tile.IsWall || floorManager.GetUnit(next) != null)
            {
                isCrashed = true;
                break;
            }
            position = next;
        }
        if (position != target.Position)
        {
            floorManager.OnMoveUnit(target, position);
            await target.SetPositionAsync(position, target.GetCancellationTokenOnDestroy());
        }
        if (isCrashed && Data.Param3 > 0)
            target.Damage(Data.Param3, Owner);
    }

    /// <summary>
    /// 前方の敵にダメージを与える。状態異常の敵にはダメージ倍増
    /// </summary>
    private void FollowupAttack(Action onComplete)
    {
        var target = floorManager.GetUnit(Owner.Position + Owner.Angle) as Enemy;
        if (target == null)
        {
            onComplete?.Invoke();
            return;
        }
        var power = Data.Param1;
        if (target.HasAnyAilment)
            power *= Data.Param2;
        Owner.Attack((int)(power * (Owner.ChargeStack + 1f)), target, () => onComplete?.Invoke());
    }

    /// <summary>
    /// 前方の敵と位置を入れ替える
    /// </summary>
    private async void SwapPosition(Action onComplete)
    {
        var target = floorManager.GetUnit(Owner.Position + Owner.Angle) as Enemy;
        if (target == null)
        {
            await MisFire(onComplete);
            return;
        }
        await Owner.SwapAsync(target);
        onComplete?.Invoke();
    }

    /// <summary>
    /// 直線上の敵を目の前まで引き寄せてダメージを与える
    /// </summary>
    private async void PullAttack(Action onComplete)
    {
        var (_, _, target) = floorManager.GetHitPosition(Owner.Position, Owner.Angle, Data.Range);
        if (target == null)
        {
            await MisFire(onComplete);
            return;
        }
        var destPosition = Owner.Position + Owner.Angle;
        if (target.Position != destPosition)
        {
            floorManager.OnMoveUnit(target, destPosition);
            await target.SetPositionAsync(destPosition, target.GetCancellationTokenOnDestroy());
        }
        var power = (int)(Data.Param1 * (Owner.ChargeStack + 1f));
        Owner.Attack(power, target, () => onComplete?.Invoke());
    }

    public bool CanUse()
    {
        if (IsPassive) return true;

        switch (Data.Type)
        {
            case CardType.NormalAttack:
            case CardType.ResourceAttack:
                return CheckEnemyInAroundTile();
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
            case CardType.DrawAndUse:
                return true;
            case CardType.PenetrateAttack:
            case CardType.PullAttack:
                return CheckEnemyInRange() || CheckEnemyInAroundTile();
            case CardType.AilmentAttack:
            case CardType.KnockbackAttack:
            case CardType.Swap:
                return CheckEnemyInAroundTile();
            case CardType.CureAilment:
                return Owner.HasAnyAilment;
            case CardType.Redraw:
                return Owner.CanRedraw;
            case CardType.FollowupAttack:
                return CheckAilmentEnemyInAroundTile();
            case CardType.PlaceTrap:
                return CanPlaceTrap();
            case CardType.LastCardAttack:
                return Owner.HandCount == 1 && CheckEnemyInAroundTile();
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
        return floorManager.GetAroundTilesAt(Owner.Position).Where(tile => floorManager.GetUnit(tile.Position) != null).Any();
    }

    private bool CheckAilmentEnemyInAroundTile()
    {
        return floorManager.GetAroundTilesAt(Owner.Position)
            .Select(tile => floorManager.GetUnit(tile.Position) as Enemy)
            .Any(enemy => enemy != null && enemy.HasAnyAilment);
    }

    /// <summary>
    /// 足元に罠を設置できるか(階段と既存の罠の上には置けない)
    /// </summary>
    private bool CanPlaceTrap()
    {
        var position = Owner.Position;
        if (floorManager.FloorData.IsStair(position.x, position.y)) return false;
        return floorManager.GetTrap(position) == null;
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

    private void OnDestroy()
    {
        tween?.Kill();
        tween = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        frontSide?.SetActive(visibleFrontSide);
        backSide?.SetActive(!visibleFrontSide);
    }
#endif

}
