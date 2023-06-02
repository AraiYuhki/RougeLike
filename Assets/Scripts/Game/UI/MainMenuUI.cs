using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MainMenuUI : ScrollMenu, IControllable
{
    [SerializeField]
    private SelectableItem inventoryItem;
    [SerializeField]
    private SelectableItem floorItem;
    [SerializeField]
    private SelectableItem pauseItem;
    [SerializeField]
    private SelectableItem retireItem;

    public void Initialize(UnityAction onSelectInventory, UnityAction onSelectFloor, UnityAction onSelectPause, UnityAction onSelectRetire)
    {
        items = new List<SelectableItem>()
        {
            inventoryItem,
            floorItem,
            pauseItem,
            retireItem
        };
        group.alpha = 0f;
        gameObject.SetActive(false);
        inventoryItem.Initialize(() => SetSelectIndex(0), onSelectInventory);
        floorItem.Initialize(() => SetSelectIndex(1), onSelectFloor);
        pauseItem.Initialize(() => SetSelectIndex(2), onSelectPause);
        retireItem.Initialize(() => SetSelectIndex(3), onSelectRetire);
    }

    public IEnumerator Controll()
    {
        yield return null;
    }
}
