using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.ProBuilder;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class Unit : MonoBehaviour
{
    [SerializeField]
    protected NoticeGroup notice;
    [SerializeField]
    protected GameObject unit;

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
    public virtual DamagePopupManager DamagePopupManager { protected get; set; }
    

    public void Awake()
    {
        Initialize();
    }
    public void SetNoticeGroup(NoticeGroup noticeGroup) => this.notice = noticeGroup;

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
        var currentPosition = transform.localPosition;
        var sequence = DOTween.Sequence();
        sequence.Append(unit.transform.DOLocalMove(Vector3.forward * 2f, 0.2f).SetEase(Ease.InCubic));
        sequence.Append(unit.transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
        sequence.OnComplete(() =>
        {
            OnAttack?.Invoke(this, target);
            target.Damage(damage, this);
            onEndAttack?.Invoke();
        });
        sequence.SetAutoKill(true);
        sequence.Play();
        notice.Add($"{this.name} は {target.name} に {damage}ダメージを与えた", Color.red);
        ChargeStack = 0;
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
        DamagePopupManager.Create(this, Mathf.RoundToInt(value), Color.green);
        Hp += (int)value;
        Hp = Mathf.Min(Hp, MaxHp);
    }

    public virtual void Damage(int damage, Unit attacker)
    {
        DamagePopupManager.Create(this, damage, Color.red);
        Hp -= damage;
        OnDamage?.Invoke(attacker, damage);
        if (Hp <= 0)
            Dead(attacker);
    }

    public void Dead(Unit attacker)
    {
        if (this is Enemy enemy && attacker != null)
        {
            attacker.AddExp(enemy.Data.Exp);
            notice.Add($"{enemy.Data.Name}は倒れた", Color.yellow);
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
