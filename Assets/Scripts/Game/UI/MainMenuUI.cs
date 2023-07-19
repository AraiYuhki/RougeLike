using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MainMenuUI : ScrollMenu
{
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private SelectableItem inventoryItem;
    [SerializeField]
    private SelectableItem floorItem;
    [SerializeField]
    private SelectableItem pauseItem;
    [SerializeField]
    private SelectableItem retireItem;
    [SerializeField]
    private SelectableItem closeItem;

    public void Initialize(Action onSelectInventory, Action onSelectFloor, Action onSelectPause, Action onSelectRetire, Action onSelectClose)
    {
        items = new List<SelectableItem>()
        {
            inventoryItem,
            floorItem,
            pauseItem,
            retireItem,
            closeItem
        };
        group.alpha = 0f;
        gameObject.SetActive(false);
        inventoryItem.Initialize(() => SetSelectIndex(0), onSelectInventory);
        floorItem.Initialize(() => SetSelectIndex(1), onSelectFloor);
        pauseItem.Initialize(() => SetSelectIndex(2), onSelectPause);
        retireItem.Initialize(() => SetSelectIndex(3), onSelectRetire);
        closeItem.Initialize(() => SetSelectIndex(4), onSelectClose);
    }

    public override void Open(Action onComplete = null)
    {
        canvas.enabled = true;
        base.Open(onComplete);
    }

    public override void Close(Action onComplete = null)
    {
        base.Close(() =>
        {
            onComplete?.Invoke();
            canvas.enabled = false;
        });
    }
}
