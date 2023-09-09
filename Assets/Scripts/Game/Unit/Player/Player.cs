using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;

public class Player : Unit
{
    [SerializeField]
    private CardController cardController;
    [SerializeField]
    private GameObject pointLight;
    [SerializeField]
    private DamagePopupManager damagePopupManager;

    public PlayerData Data { get; private set; } = new PlayerData(10);
    public override int Hp { get => Mathf.FloorToInt(Data.Hp); set => Data.Hp = value; }
    public override int MaxHp => cardController.AllCardsCount * 5;
    public override string Name => "Player";
    public override void RecoveryStamina(float value) => Data.Stamina += value;
    public override void PowerUp(int value, Action onComplete = null) => Data.Atk += value;
    public override DamagePopupManager DamagePopupManager 
    {
        protected get => damagePopupManager;
        set => damagePopupManager = value;
    }

    public bool IsLockInput { get; set; }
    public int HealInterval { get; set; } = 0;

    public void Initialize(int lv, int hp, int atk, int def)
    {
        Data = new PlayerData(hp)
        {
            Lv = lv,
            Atk = atk,
            Def = def,
            Gems = 0,
        };
    }

    public override void Damage(int damage, Unit attacker, bool damagePopup = true)
    {
        var passiveEffects = cardController.PassiveEffects();
        var lastDamage = damage;
        var defenseRate = 1f;
        foreach(var effect in passiveEffects)
        {
            if (effect.EffectType == PassiveEffectType.DefenceUp)
            {
                lastDamage -= effect.Param1;
                notice.Add($"{effect.Param1}ダメージ軽減!", Color.green);
            }
            else if (effect.EffectType == PassiveEffectType.DeffenceUpRate)
                defenseRate -= effect.Param1 * 0.01f;
        }
        lastDamage = Mathf.Max(0, lastDamage);
        lastDamage = Mathf.CeilToInt(lastDamage * defenseRate);
        if(defenseRate < 1f)
            notice.Add($"ダメージ{(1.0f - defenseRate) * 100}%軽減! ({damage} > {lastDamage})", Color.green);
        base.Damage(lastDamage, attacker, damagePopup);
        foreach(var effect in passiveEffects)
        {
            if (effect.EffectType == PassiveEffectType.Counter)
            {
                notice.Add("カウンター発動！");
                attacker.Damage(effect.Param1, this);
            }
            else if (effect.EffectType == PassiveEffectType.Refrect)
            {
                notice.Add("カウンター発動!");
                var counterDamage = Mathf.CeilToInt(lastDamage * effect.Param1 * 0.01f);
                attacker.Damage(counterDamage, this);
            }
        }
        HealInterval = 10;
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
        Move(move, () =>
        {
            IsLockInput = false;
            var flag = floorManager.GetTile(Position).IsRoom;
            pointLight.SetActive(flag);
        });
        ChargeStack = 0;
    }

    public override void SetPosition(Vector2Int position)
    {
        base.SetPosition(position);
        var flag = floorManager.GetTile(position).IsRoom;
        pointLight.SetActive(flag);
    }

    public override void TurnEnd()
    {
        var rate = 1f - cardController.PassiveEffects()
            .Where(effect => effect.EffectType == PassiveEffectType.Satiated)
            .Sum(effect => effect.Param1 * 0.01f);

        Data.Stamina -= 0.1f * Mathf.Max(rate, 0f);
        if (Data.Stamina <= 0)
            Damage(1, null, damagePopup: false);
        else if (HealInterval > 0)
            HealInterval--;
        else
            Heal(1f, false);
    }

    public void Attack(int damage, Enemy target, TweenCallback onEndAttack = null, bool isResourceAttack = false)
    {
        var lastDamage = damage;
        var rate = 1f;
        var passiveEffects = cardController.PassiveEffects();
        foreach(var effect in passiveEffects)
        {
            if (effect.EffectType == PassiveEffectType.AttackUp)
            {
                notice.Add($"{effect.Param1}ダメージ増加！", Color.green);
                lastDamage += effect.Param1;
            }
            else if (effect.EffectType == PassiveEffectType.AttackUpRate)
                rate += effect.Param1 * 0.01f;
        }
        if (rate > 1f)
            notice.Add($"ダメージ{(rate - 1f) * 100}%増加! ({damage} > {lastDamage})", Color.green);
        lastDamage = Mathf.CeilToInt(lastDamage * rate);

        var dropUpRate = passiveEffects.Where(effect => effect.EffectType == PassiveEffectType.DropUp).Sum(effect => effect.Param1 * 0.01f);
        if (UnityEngine.Random.value < dropUpRate)
        {
            isResourceAttack = true;
            notice.Add("アイテムドロップ！");
        }

        var sequence = DOTween.Sequence();
        sequence.Append(unit.transform.DOLocalMove(Vector3.forward * 2f, 0.2f).SetEase(Ease.InCubic));
        sequence.Append(unit.transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
        sequence.OnComplete(() =>
        {
            OnAttack?.Invoke(this, target);
            var resultDamage = (int)(lastDamage * (ChargeStack + 1));
            if (isResourceAttack)
            {
                var dropTile = floorManager.GetCanDropTile(target.Position);
                if (dropTile != null)
                    itemManager.Drop(resultDamage, dropTile.Position, true);
            }
            target.Damage(resultDamage, this);
            onEndAttack?.Invoke();
        });
        sequence.SetAutoKill(true);
        sequence.Play();
        ChargeStack = 0;
    }
}
