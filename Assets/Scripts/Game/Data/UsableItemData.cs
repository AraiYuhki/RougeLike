using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    HPHeal,
    StaminaHeal,
    PowerUp,
}
[Serializable]
public class UsableItemData : ItemBase
{
    [SerializeField]
    private ItemType type;
    [SerializeField]
    private float parameter;
    [SerializeField]
    private bool isStackable = false;
    public ItemType Type { get => type; set => type = value; }
    public float Parameter { get => parameter; set => parameter = value; }
    public override bool IsStackable => isStackable;

    public UsableItemData(UsableItemData other) : base(other)
    {
        type = other.type;
        parameter = other.parameter;
        isStackable = other.isStackable;
    }

    public void Use(Unit user, Action onUsed = null)
    {
        switch(type)
        {
            case ItemType.HPHeal:
                user.Heal(parameter);
                break;
            case ItemType.StaminaHeal:
                user.RecoveryStamina(parameter);
                break;
            case ItemType.PowerUp:
                user.PowerUp((int)parameter);
                break;
            default:
                break;
        }
        onUsed?.Invoke();
    }

    public override object Clone() => new UsableItemData(this);
}
