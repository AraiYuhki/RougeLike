using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class ItemRowController : SelectableItem
{
    [SerializeField]
    private TMP_Text nameLabel;
    [SerializeField]
    private TMP_Text stackLabel;
    [SerializeField]
    private Image equipIcon;

    public ItemBase ItemData { get; private set; }

    public void Initialize(ItemBase itemData, int count, bool isEquip, Action onSelect = null, Action onSubmit = null)
    {
        base.Initialize(onSelect, onSubmit);
        ItemData = itemData;
        nameLabel.text = itemData.Name;
        equipIcon.gameObject.SetActive(isEquip);
        stackLabel.gameObject.SetActive(itemData.IsStackable);
        stackLabel.text = $"x{count}";
    }

    public void UpdateStatus(bool isEquip, int count)
    {
        nameLabel.text = ItemData.Name;
        equipIcon.gameObject.SetActive(isEquip);
        stackLabel.text = $"x{count}";
    }
}
