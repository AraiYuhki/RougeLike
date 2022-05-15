using UnityEngine;
using DG.Tweening;
using System;

public class Unit : MonoBehaviour
{
    public Vector2Int Position { get; set; }
    public float DestAngle { get; set; }

    public bool EndRotation { get; protected set; }

    public event Action<Unit, Vector2Int> OnMoved;
    public event Action<Unit, Unit> OnAttack;
    public event Action<Unit, int> OnDamage;
    public event Action OnDead;

    public virtual int Hp { get; set; }
    public virtual void AddExp(int exp) { }

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
        var targetPosition = target.transform.localPosition;
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
        Debug.LogError($"{this.name} ÇÕ {target.name} Ç… {damage}É_ÉÅÅ[ÉWÇó^Ç¶ÇΩ");
    }

    public void MoveTo(Vector2Int destPosition, TweenCallback onComplete = null)
    {
        var diff = destPosition - Position;
        SetPosition(destPosition, onComplete);
        SetDestAngle(diff);
        OnMoved?.Invoke(this, destPosition);
    }

    public void Move(Vector2Int move, TweenCallback onComplete = null)
    {
        var dest = Position + move;
        SetPosition(dest, onComplete);
        SetDestAngle(move);
        OnMoved?.Invoke(this, dest);
    }

    public virtual void Damage(int damage, Unit attacker)
    {
        Hp -= damage;
        OnDamage?.Invoke(attacker, damage);
        if (Hp <= 0)
            Dead(attacker);
    }

    public void Dead(Unit attacker)
    {
        if (this is Enemy enemy)
            attacker.AddExp(enemy.Data.Exp);
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
        var destAngle = Vector3.SignedAngle(Vector3.forward, new Vector3(move.x, 0f, move.y), Vector3.up);
        transform.DORotate(new Vector3(0f, destAngle, 0f), 0.1f).SetEase(Ease.OutCubic);
    }
}
