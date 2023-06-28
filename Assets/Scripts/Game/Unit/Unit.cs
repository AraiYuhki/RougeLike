using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.ProBuilder;
using UnityEditor;
using System.Drawing.Imaging;
using System.Linq;

public class Unit : MonoBehaviour
{
    public const float MaxChargeStack = 4f;
    public Vector2Int Position { get; set; }
    public float DestAngle { get; set; }

    public bool EndRotation { get; protected set; }
    public Vector2Int Angle { get; protected set; } = Vector2Int.up;

    public Action<Unit, Vector2Int> OnMoved{get;set;}
    public Action<Unit, Unit> OnAttack{get;set;}
    public Action<Unit, int> OnDamage{get;set;}
    public Action OnDead { get; set; }

    public virtual int Hp { get; set; }
    public virtual int MaxHp { get; set; }
    public virtual float ChargeStack { get; set; }
    public virtual void AddExp(int exp) { }
    public virtual void RecoveryStamina(float value) { }
    public virtual void PowerUp(int value) { }
    public virtual void Charge(float value) => ChargeStack = Mathf.Min(ChargeStack + value, MaxChargeStack);

    public void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
    }

    public virtual void Update()
    {
    }

    public void Move(int x, int z) => Move(new Vector2Int(x, z));
    public virtual void Move(Vector2Int move) => Move(move, null);

    public virtual void Attack(Unit target, TweenCallback onEndAttack = null)
    {
        var damage = DamageUtil.GetDamage(this, target);
        var targetPosition = new Vector3(target.Position.x, target.transform.localPosition.y, target.Position.y);
        var currentPosition = transform.localPosition;
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMove(targetPosition, 0.2f).SetEase(Ease.InCubic));
        sequence.Append(transform.DOLocalMove(currentPosition, 0.2f).SetEase(Ease.OutCubic));
        sequence.OnComplete(() =>
        {
            OnAttack?.Invoke(this, target);
            target.Damage(damage, this);
            onEndAttack?.Invoke();
        });
        sequence.SetAutoKill(true);
        sequence.Play();
        Debug.LogError($"{this.name} は {target.name} に {damage}ダメージを与えた");
        ChargeStack = 0;
    }

    public virtual void Attack(int weaponPower, AttackAreaData attackArea,  Action onComplete = null)
    {
        var floorManager = ServiceLocator.Instance.FloorManager;
        var tween = DOTween.Sequence();
        // 角度はアニメーション前に取得しておく
        var angle = transform.localEulerAngles.y;
        tween.Append(transform.DOLocalRotate(transform.localEulerAngles + Vector3.up * 360f, 0.6f, RotateMode.FastBeyond360));
        tween.AppendInterval(0.2f);
        tween.OnComplete(() =>
        {
            foreach (var offset in attackArea.Data.Select(data => data.Offset - attackArea.Center))
            {
                var rotatedOffset = AttackAreaData.GetRotatedOffset(angle, offset);
                var position = Position + offset;
                var target = floorManager.GetUnit(position);
                if (this is Player player && target is Enemy enemy)
                {
                    enemy.Damage(DamageUtil.GetDamage(player, weaponPower, enemy), this);
                }
            }
            onComplete?.Invoke();
        });
    }

    public virtual void Shoot(int weaponPower, int range, Action onComplete = null)
    {
        var target = ServiceLocator.Instance.FloorManager.GetHitPosition(Position, Angle, range);
        var bullet = ServiceLocator.Instance.GameController.CreateBullet(transform.localPosition, transform.rotation);
        var tween = bullet.transform
            .DOLocalMove(new Vector3(target.position.x, 0.5f, target.position.y), 0.1f * target.length)
            .SetEase(Ease.Linear)
            .Play();
        tween.OnComplete(() =>
        {
            Destroy(bullet);
            onComplete?.Invoke();
            if (this is Player player && target.enemy != null)
            {
                target.enemy.Damage(DamageUtil.GetDamage(player, weaponPower, target.enemy), this);
            }
        });
    }

    public void MoveTo(Vector2Int destPosition, TweenCallback onComplete = null)
    {
        var diff = destPosition - Position;
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

    public virtual void Heal(float value)
    {
        Hp += (int)value;
        Hp = Mathf.Min(Hp, MaxHp);
    }

    public virtual void Damage(int damage, Unit attacker)
    {
        Hp -= damage;
        OnDamage?.Invoke(attacker, damage);
        Debug.LogError($"{name}は{damage}ダメージを受けた");
        if (Hp <= 0)
            Dead(attacker);
    }

    public void Dead(Unit attacker)
    {
        if (this is Enemy enemy && attacker != null)
        {
            attacker.AddExp(enemy.Data.Exp);
            Debug.LogError($"{enemy.Data.Name}は倒れた");
        }
        OnDead?.Invoke();
    }

    public virtual void TurnEnd() 
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
        if (onComplete != null) tween.onComplete = onComplete;
    }

    public virtual void SetDestAngle(Vector2Int move)
    {
        Angle = move;
        var destAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(move.x, 0f, move.y), Vector3.up);
        transform.DORotate(new Vector3(0f, destAngle, 0f), 0.1f).SetEase(Ease.OutCubic);
    }
}
