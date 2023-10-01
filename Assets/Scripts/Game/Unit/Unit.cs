using UnityEngine;
using DG.Tweening;
using System;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    [SerializeField]
    protected GameController gameController;
    [SerializeField]
    protected FloorManager floorManager;
    [SerializeField]
    protected EnemyManager enemyManager;
    [SerializeField]
    protected ItemManager itemManager;
    [SerializeField]
    protected NoticeGroup notice;
    [SerializeField]
    protected GameObject unit;

    public const float MaxChargeStack = 4f;
    public Vector2Int Position { get; set; }
    public float DestAngle { get; set; }

    public bool EndRotation { get; protected set; }
    public Vector2Int Angle { get; protected set; } = Vector2Int.up;

    protected List<Tween> tweenList = new List<Tween>();

    public Action<Unit, Vector2Int> OnMoved{get;set;}
    public Action<Unit, Unit> OnAttack{get;set;}
    public Action<Unit, int> OnDamage{get;set;}
    public Action OnDead { get; set; }

    public virtual int Hp { get; set; }
    public virtual int MaxHp { get; set; }
    public virtual string Name => "No Name";
    public virtual float ChargeStack { get; set; }
    public virtual void AddExp(int exp) { }
    public virtual void RecoveryStamina(float value) { }
    public virtual void PowerUp(int value, Action onComplete = null) { }
    public virtual void Charge(float value, Action onComplete = null)
    {
        transform.DOPunchScale(Vector3.one * 0.5f, 0.5f).OnComplete(() =>
        {
            ChargeStack = Mathf.Min(ChargeStack + value, MaxChargeStack);
            onComplete?.Invoke();
        });
    }

    public virtual void DrawAndUse(int count, CardCategory category, Action onComplete)
    {
        switch(category)
        {
            case CardCategory.Attack:
                ContinouseAttack(count, onComplete);
                break;
            case CardCategory.Utility:
                ContinouseUse(count, onComplete);
                break;
            default:
                throw new NotImplementedException($"{category}は未実装です");
        }
    }

    /// <summary>
    /// 指定した枚数カードを引いて、引いた攻擊カードを全て使用し、他のカードは捨てる
    /// </summary>
    /// <param name="count"></param>
    public virtual void ContinouseAttack(int count, Action onComplete) { }

    /// <summary>
    /// 指定した枚数のカードを引いて、引いた道具系カードをすべて使用し、他のカードは捨てる
    /// </summary>
    /// <param name="count"></param>
    public virtual void ContinouseUse(int count, Action onComplete) { }

    public virtual DamagePopupManager DamagePopupManager { protected get; set; }
    

    public void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
    }

    public void SetManagers(GameController gameController, FloorManager floorManager, EnemyManager enemyManager, ItemManager itemManager, NoticeGroup noticeGroup)
    {
        this.gameController = gameController;
        this.floorManager = floorManager;
        this.enemyManager = enemyManager;
        this.itemManager = itemManager;
        this.notice = noticeGroup;
    }

    public virtual void Update()
    {
    }

    protected virtual void OnDestroy()
    {
        foreach (var tween in tweenList)
            tween.Kill();
        tweenList.Clear();
    }

    public void Move(int x, int z) => Move(new Vector2Int(x, z));
    public virtual void Move(Vector2Int move) => Move(move, null);

    public virtual void Attack(Unit target, int damage, Action onComplete = null, bool isResourceAttack = false)
    {
        var currentPosition = transform.localPosition;
        var sequence = DOTween.Sequence();
        tweenList.Add(sequence);
        sequence.Append(unit.transform.DOLocalMove(Vector3.forward * 2f, 0.2f).SetEase(Ease.InCubic));
        sequence.Append(unit.transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
        sequence.OnComplete(() =>
        {
            tweenList.Remove(sequence);
            OnAttack?.Invoke(this, target);
            target.Damage(damage, this);
            onComplete?.Invoke();
        });
        sequence.SetAutoKill(true);
        sequence.Play();
        ChargeStack = 0;
    }

    public virtual void Attack(int damage, AttackAreaInfo attackArea, Action onComplete = null)
    {
        damage = (int)(damage * (ChargeStack + 1));
        var tween = DOTween.Sequence();
        // 角度はアニメーション前に取得しておく
        var angle = transform.localEulerAngles.y;
        tweenList.Add(tween);
        tween.Append(unit.transform.DOLocalRotate(Vector3.up * 360f, 0.6f, RotateMode.FastBeyond360));
        tween.AppendInterval(0.2f);
        tween.OnComplete(() =>
        {
            tweenList.Remove(tween);
            foreach (var offset in attackArea.Data.Select(data => data.Offset))
            {
                var rotatedOffset = AttackAreaInfo.GetRotatedOffset(angle, offset);
                var position = Position + offset;
                var target = floorManager.GetUnit(position);
                if (this is Player player && target is Enemy enemy)
                {
                    enemy.Damage(damage, this);
                }
            }
            onComplete?.Invoke();
        });
        ChargeStack = 0;
    }

    public virtual void RoomAttack(int damage, Action onComplete = null)
    {
        damage = (int)(damage * (ChargeStack + 1));
        var currentTile = floorManager.GetTile(Position);
        var tween = DOTween.Sequence();
        tweenList.Add(tween);
        tween.Append(unit.transform.DOLocalRotate(Vector3.up * 360f * 3f, 0.6f, RotateMode.FastBeyond360));
        tween.AppendInterval(0.2f);
        tween.OnComplete(() =>
        {
            tweenList.Remove(tween);
            if (currentTile.IsRoom)
            {
                foreach (var enemy in enemyManager.Enemies)
                {
                    var tile = floorManager.GetTile(enemy.Position);
                    if (!tile.IsRoom || tile.Id != currentTile.Id) continue;
                    enemy.Damage(damage, this as Player);
                }
            } 
            else
            {
                foreach(var enemy in floorManager.GetAroundTilesAt(Position)
                    .Select(tile => floorManager.GetUnit(tile.Position) as Enemy)
                    .Where(enemy => enemy != null))
                {
                    enemy.Damage(damage, this as Player);
                }
            }
            onComplete?.Invoke();
        });
        ChargeStack = 0;
    }

    public virtual void Shoot(int damage, int range, Action onComplete = null)
    {
        damage = (int)(damage * (ChargeStack + 1));
        var target = floorManager.GetHitPosition(Position, Angle, range);
        var bullet = gameController.CreateBullet(transform.localPosition, transform.rotation);
        var tween = bullet.transform
            .DOLocalMove(new Vector3(target.position.x, 0.5f, target.position.y), 0.1f * target.length)
            .SetEase(Ease.Linear)
            .Play();
        tweenList.Add(tween);
        tween.OnComplete(() =>
        {
            tweenList.Remove(tween);
            Destroy(bullet);
            if (this is Player player && target.enemy != null)
            {
                target.enemy.Damage(damage, player);
            }
            onComplete?.Invoke();
        });
        ChargeStack = 0;
    }

    public void MoveTo(Vector2Int destPosition, TweenCallback onComplete = null)
    {
        var diff = destPosition - Position;
        ChargeStack = 0;
        OnMoved?.Invoke(this, destPosition);
        SetPosition(destPosition, onComplete);
        SetDestAngle(diff);
    }

    public void Move(Vector2Int move, TweenCallback onComplete = null)
    {
        var dest = Position + move;
        ChargeStack = 0;
        OnMoved?.Invoke(this, dest);
        SetPosition(dest, onComplete);
        SetDestAngle(move);
    }

    public virtual void Heal(float value, bool damagePopup = true)
    {
        if (damagePopup) DamagePopupManager.Create(this, Mathf.RoundToInt(value), Color.green);
        Hp += (int)value;
        Hp = Mathf.Min(Hp, MaxHp);
    }

    public virtual void Damage(int damage, Unit attacker, bool damagePopup = true)
    {
        if (damagePopup) DamagePopupManager.Create(this, damage, Color.red);
        notice.Add($"{attacker.Name}は{Name}に{damage}ダメージ与えた", Color.red);
        Hp -= damage;
        OnDamage?.Invoke(attacker, damage);
        if (Hp <= 0)
            Dead(attacker);
    }

    public virtual void Damage(int damage)
    {
        DamagePopupManager.Create(this, damage, Color.magenta);
        notice.Add($"{Name}は{damage}ダメージ受けた", Color.red);
        Hp -= damage;
        OnDamage?.Invoke(null, damage);
        if (Hp <= 0)
            Dead(null);
    }

    public void Dead(Unit attacker)
    {
        if (this is Enemy enemy && attacker != null)
        {
            notice.Add($"{enemy.Name}は倒れた", Color.yellow);
        }
        OnDead?.Invoke();
    }

    public virtual void TurnEnd() 
    {
        ExecuteAilments();
    }

    protected virtual void ExecuteAilments()
    {

    }

    public virtual void SetPosition(Vector2Int position)
    {
        Position = position;
        transform.localPosition = new Vector3(position.x, 0.5f, position.y);
    }

    public virtual void SetPosition(Vector2Int position, TweenCallback onComplete)
    {
        Position = position;
        var tween = transform.DOLocalMove(new Vector3(position.x, 0.5f, position.y), 0.2f).SetEase(Ease.OutCubic);
        tweenList.Add(tween);
        tween.OnComplete(() =>
        {
            tweenList.Remove(tween);
            onComplete?.Invoke();
        });
    }

    public virtual void SetDestAngle(Vector2Int move)
    {
        Angle = move;
        var destAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(move.x, 0f, move.y), Vector3.up);
        var tween = transform.DORotate(new Vector3(0f, destAngle, 0f), 0.1f).SetEase(Ease.OutCubic);
        tweenList.Add(tween);
        tween.OnComplete(() => tweenList.Remove(tween));
    }
}
