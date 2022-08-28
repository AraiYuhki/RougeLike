using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MenuUI : MonoBehaviour
{
    enum ControllType
    {
        Inventory,
        UseMenu,
    }

    [SerializeField]
    private InventoryUI inventoryUI;
    [SerializeField]
    private StatusUI statusUI;
    [SerializeField]
    private UseMenuUI useMenuUI;

    public InventoryUI InventoryUI => inventoryUI;
    public StatusUI StatusUI => statusUI;
    private Player Player => ServiceLocator.Instance.GameController.Player;
    private PlayerData Data => Player.Data;
    private bool isOpened = false;
    private ControllType type = ControllType.Inventory;
    private Action usedCallback;

    public void Initialize(Player player, Action usedCallback)
    {
        inventoryUI.Initialize(player);
        statusUI.Initialize(player);
        this.usedCallback = usedCallback;
    }

    public void Open(TweenCallback onComplete = null)
    {
        inventoryUI.Open();
        statusUI.Open(() =>
        {
            type = ControllType.Inventory;
            onComplete?.Invoke();
            isOpened = true;
        });
    }

    public void Close(TweenCallback onComplete = null)
    {
        inventoryUI.Close();
        useMenuUI.Close();
        statusUI.Close(() => {
            isOpened = false;
            onComplete?.Invoke();
        });
    }

    public void SwitchOpenMenu(TweenCallback onComplete)
    {
        if (!isOpened)
            Open(onComplete);
        else
            Close(onComplete);
    }

    public IEnumerator Controll()
    {
        switch(type)
        {
            case ControllType.Inventory:
                InventoryControll();
                break;
            case ControllType.UseMenu:
                UseMenuControll();
                break;
        }
        yield return null;
    }

    private void InventoryControll()
    {
        if (InputUtility.Up.IsTriggerd())
            inventoryUI.Up();
        else if (InputUtility.Down.IsTriggerd())
            inventoryUI.Down();

        if (InputUtility.Submit.IsTriggerd())
        {
            var menu = new List<(string, UnityAction)>();
            if (inventoryUI.SelectedItem is WeaponData weapon)
            {
                menu.Add(("装備", () => Player.Data.EquipmentWeapon = weapon));
            }
            else if (inventoryUI.SelectedItem is ShieldData shield)
            {
                menu.Add(("装備", () => Player.Data.EquipmentShield = shield));
            }
            else if (inventoryUI.SelectedItem is UsableItemData data)
            {
                menu.Add(("使う", () => { UseItem(data); }));
            }
            menu.Add(("投げる", () => { }));
            menu.Add(("置く", () => { }));
            menu.Add(("戻る", () =>
            {
                useMenuUI.Close();
                type = ControllType.Inventory;
            }
            ));
            useMenuUI.Initialize(menu);
            useMenuUI.Open();
            type = ControllType.UseMenu;
        }
    }

    private void UseMenuControll()
    {
        if (InputUtility.Up.IsTriggerd())
            useMenuUI.Up();
        else if (InputUtility.Down.IsTriggerd())
            useMenuUI.Down();
        if (InputUtility.Submit.IsTriggerd())
            useMenuUI.Submit();
        else if (InputUtility.Cancel.IsTriggerd())
        {
            useMenuUI.Close();
            type = ControllType.Inventory;
        }
    }

    private void UseItem(UsableItemData data)
    {
        switch(data.Type)
        {
            case ItemType.HPHeal:
                Player.Heal(data.Parameter);
                break;
            case ItemType.PowerUp:
                Player.PowerUp((int)data.Parameter);
                break;
            case ItemType.StaminaHeal:
                Player.RecoveryStamina(data.Parameter);
                break;
            default:
                throw new NotImplementedException();
        }
        Debug.LogError($"{data.Name}:を使用した");
        if (!data.IsStackable)
        {
            Data.Inventory.Remove(data);
            Close(() => usedCallback?.Invoke());
            return;
        }

        Data.Inventory[data]--;
        if (Data.Inventory[data] <= 0)
            Player.Data.Inventory.Remove(data);
        Close(() => usedCallback?.Invoke());
    }
}
