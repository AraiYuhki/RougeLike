using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
    private Action equipedCallback;

    public void Initialize(Player player, Action usedCallback)
    {
        inventoryUI.Initialize(player);
        statusUI.Initialize(player);
        this.usedCallback = usedCallback;
    }

    public void Open(TweenCallback onComplete = null)
    {
        inventoryUI.Initialize();
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
                menu.Add(("����", () => EquipWeapon(weapon)));
            }
            else if (inventoryUI.SelectedItem is ShieldData shield)
            {
                menu.Add(("����", () => EquipShield(shield)));
            }
            else if (inventoryUI.SelectedItem is UsableItemData data)
            {
                menu.Add(("�g��", () => { UseItem(data); }));
            }
            menu.Add(("������", () => { }));
            menu.Add(("�u��", () => { }));
            menu.Add(("�߂�", () =>
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
            CloseUseMenu();
    }

    private void CloseUseMenu()
    {
        useMenuUI.Close();
        type = ControllType.Inventory;
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
        Debug.LogError($"{data.Name}:���g�p����");
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

    private void EquipWeapon(WeaponData weapon)
    {
        Player.Data.EquipmentWeapon = weapon;
        Debug.LogError($"{weapon.Name}�𑕔�����");
        inventoryUI.UpdateStatus();
        CloseUseMenu();
    }

    private void EquipShield(ShieldData shield)
    {
        Player.Data.EquipmentShield = shield;
        Debug.LogError($"{shield.Name}�𑕔�����");
        inventoryUI.UpdateStatus();
        CloseUseMenu();
    }
}
