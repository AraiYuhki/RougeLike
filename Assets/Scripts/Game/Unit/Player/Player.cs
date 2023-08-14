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
            Gems = 50,
        };
        for (var i = 0; i < 3; i++)
        {
            Data.Inventory.Add(DataBase.Instance.GetTable<MItem>().Data.First().Clone() as ItemBase, 1);
        }
    }

    public override void Damage(int damage, Unit attacker, bool isResourceAttack = false, bool damagePopup = true)
    {
        base.Damage(damage, attacker, isResourceAttack, damagePopup);
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
        Data.Stamina -= 0.1f;
        if (Data.Stamina <= 0)
            Damage(1, null, damagePopup: false);
        else if (HealInterval > 0)
            HealInterval--;
        else
            Heal(1f, false);
    }

    public void Attack(int damage, Enemy target, TweenCallback onEndAttack = null, bool isResourceAttack = false)
    {
        var sequence = DOTween.Sequence();
        sequence.Append(unit.transform.DOLocalMove(Vector3.forward * 2f, 0.2f).SetEase(Ease.InCubic));
        sequence.Append(unit.transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
        sequence.OnComplete(() =>
        {
            OnAttack?.Invoke(this, target);
            target.Damage((int)(damage * (ChargeStack + 1)), this, isResourceAttack);
            onEndAttack?.Invoke();
        });
        sequence.SetAutoKill(true);
        sequence.Play();
        notice.Add($"{this.name} は {target.name} に {damage}ダメージを与えた", Color.red);
        ChargeStack = 0;
    }
}
