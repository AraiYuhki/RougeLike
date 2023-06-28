using DG.Tweening;
using System.Linq;
using UnityEngine;

public class Player : Unit
{
    public PlayerData Data { get; private set; } = new PlayerData(10);
    public override int Hp { get => Mathf.FloorToInt(Data.Hp); set => Data.Hp = value; }
    public override int MaxHp { get => Data.MaxHP; }
    public override void AddExp(int exp) => Data.AddExp(exp);
    public override void RecoveryStamina(float value) => Data.Stamina += value;
    public override void PowerUp(int value) => Data.Atk += value;

    public bool IsLockInput { get; set; }
    private int healInterval = 0;

    public void Initialize(int lv, int hp, int atk, int def)
    {
        Data = new PlayerData(hp)
        {
            Lv = lv,
            Atk = atk,
            Def = def,
        };
        for (var i = 0; i < 3; i++)
        {
            Data.Inventory.Add(DataBase.Instance.GetTable<MItem>().Data.First().Clone() as ItemBase, 1);
        }
    }

    public override void Damage(int damage, Unit attacker)
    {
        base.Damage(damage, attacker);
        healInterval = 10;
    }

    public override void Initialize()
    {
        Initialize(1, MaxHp, 1, 1);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Move(Vector2Int move)
    {
        IsLockInput = true;
        Move(move, () => IsLockInput = false);
    }

    public override void TurnEnd()
    {
        Data.Stamina -= 0.1f;
        if (Data.Stamina <= 0)
            Damage(1, null);
        else if (healInterval > 0)
            healInterval--;
        else
            Heal(Data.MaxHP * 0.095f);
    }

    public void Attack(int weaponAttack, Enemy target, TweenCallback onEndAttack = null)
    {
        var damage = DamageUtil.GetDamage(this, weaponAttack, target);
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
        Debug.LogError($"{this.name} ÇÕ {target.name} Ç… {damage}É_ÉÅÅ[ÉWÇó^Ç¶ÇΩ");
        ChargeStack = 0;
    }
}
