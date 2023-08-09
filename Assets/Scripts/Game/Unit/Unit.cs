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

    public Action<Unit, Vector2Int> OnMoved{get;set;}
    public Action<Unit, Unit> OnAttack{get;set;}
    public Action<Unit, int> OnDamage{get;set;}
    public Action OnDead { get; set; }

    public virtual int Hp { get; set; }
    public virtual int MaxHp { get; set; }
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

    public void Move(int x, int z) => Move(new Vector2Int(x, z));
    public virtual void Move(Vector2Int move) => Move(move, null);

    public virtual void Attack(Unit target, TweenCallback onEndAttack = null, bool isResourceAttack = false)
    {
        var damage = DamageUtil.GetDamage(this, target);
        var currentPosition = transform.localPosition;
        var sequence = DOTween.Sequence();
        sequence.Append(unit.transform.DOLocalMove(Vector3.forward * 2f, 0.2f).SetEase(Ease.InCubic));
        sequence.Append(unit.transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
        sequence.OnComplete(() =>
        {
            OnAttack?.Invoke(this, target);
            target.Damage(damage, this, isResourceAttack);
            onEndAttack?.Invoke();
        });
        sequence.SetAutoKill(true);
        sequence.Play();
        notice.Add($"{this.name} は {target.name} に {damage}ダメージを与えた", Color.red);
        ChargeStack = 0;
    }

    public virtual void Attack(int weaponPower, AttackAreaData attackArea, Action onComplete = null, bool isResourceAttack = false)
    {
        var tween = DOTween.Sequence();
        // 角度はアニメーション前に取得しておく
        var angle = transform.localEulerAngles.y;
        tween.Append(unit.transform.DOLocalRotate(Vector3.up * 360f, 0.6f, RotateMode.FastBeyond360));
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
                    enemy.Damage(DamageUtil.GetDamage(player, weaponPower, enemy), this, isResourceAttack);
                }
            }
            onComplete?.Invoke();
        });
    }

    public virtual void RoomAttack(int weaponPower, Action onComplete = null)
    {
        var currentTile = floorManager.GetTile(Position);
        var tween = DOTween.Sequence();
        tween.Append(unit.transform.DOLocalRotate(Vector3.up * 360f * 3f, 0.6f, RotateMode.FastBeyond360));
        tween.AppendInterval(0.2f);
        tween.OnComplete(() =>
        {
            if (currentTile.IsRoom)
            {
                foreach (var enemy in enemyManager.Enemies.Where(enemy =>
                {
                    var tile = floorManager.GetTile(enemy.Position);
                    if (!tile.IsRoom) return false;
                    return tile.Id == currentTile.Id;
                }))
                {
                    enemy.Damage(DamageUtil.GetDamage(this as Player, weaponPower, enemy), this);
                }
            } 
            else
            {
                foreach(var enemy in floorManager.GetAroundTilesAt(Position)
                    .Select(tile => floorManager.GetUnit(tile.Position) as Enemy)
                    .Where(enemy => enemy != null))
                {
                    enemy.Damage(DamageUtil.GetDamage(this as Player, weaponPower, enemy), this);
                }
            }
            onComplete?.Invoke();
        });
    }

    public virtual void Shoot(int weaponPower, int range, Action onComplete = null)
    {
        var target = floorManager.GetHitPosition(Position, Angle, range);
        var bullet = gameController.CreateBullet(transform.localPosition, transform.rotation);
        var tween = bullet.transform
            .DOLocalMove(new Vector3(target.position.x, 0.5f, target.position.y), 0.1f * target.length)
            .SetEase(Ease.Linear)
            .Play();
        tween.OnComplete(() =>
        {
            Destroy(bullet);
            if (this is Player player && target.enemy != null)
            {
                target.enemy.Damage(DamageUtil.GetDamage(player, weaponPower, target.enemy), this);
            }
            onComplete?.Invoke();
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

    public virtual void Heal(float value, bool damagePopup = true)
    {
        if (damagePopup) DamagePopupManager.Create(this, Mathf.RoundToInt(value), Color.green);
        Hp += (int)value;
        Hp = Mathf.Min(Hp, MaxHp);
    }

    public virtual void Damage(int damage, Unit attacker, bool isResourceAttack = false, bool damagePopup = true)
    {
        if (damagePopup) DamagePopupManager.Create(this, damage, Color.red);
        Hp -= damage;
        OnDamage?.Invoke(attacker, damage);
        if (Hp <= 0)
            Dead(attacker, isResourceAttack);
    }

    public void Dead(Unit attacker, bool isResourceAttack = false)
    {
        if (this is Enemy enemy && attacker != null)
        {
            attacker.AddExp(enemy.Data.Exp);
            notice.Add($"{enemy.Data.Name}は倒れた", Color.yellow);
            if (isResourceAttack)
            {
                var dropPosition = floorManager.GetCanDropTile(Position);
                if (dropPosition != null)
                    itemManager.Drop(UnityEngine.Random.Range(1, 20), dropPosition.Position, true);
            }
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
