using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CommonDialog : DialogBase
{
    [SerializeField]
    private TMP_Text message;
    [SerializeField]
    private SelectableItem original;
    [SerializeField]
    private Transform itemContainer;

    private List<SelectableItem> items = new List<SelectableItem>();

    public override DialogType Type => DialogType.Common;

    public string Message
    {
        get => message.text;
        set => message.text = value;
    }

    public void Initialize(string title, string message, params (string label, Action onClick)[] data)
    {
        foreach (var item in items) Destroy(item.gameObject);
        items.Clear();

        Title = title;
        Message = message;

        foreach((var label, var onClick) in data)
        {
            var instance = Instantiate(original, itemContainer);
            instance.gameObject.SetActive(true);
            instance.Label = label;
            instance.Initialize(null, onClick);
            items.Add(instance);
        }
        original.gameObject.SetActive(false);
        UpdateView();
    }

    private void UpdateView()
    {
        foreach((var item, var index) in items.Select((item, index) => (item, index)))
            item.Select(index == currentSelected);
    }

    public override void Left()
    {
        currentSelected--;
        if (currentSelected < 0) currentSelected += items.Count;
    }

    public override void Right()
    {
        currentSelected++;
        if (currentSelected >= items.Count) currentSelected -= items.Count;
    }

    public override void Controll()
    {
        var prevIndex = currentSelected;
        if (InputUtility.Left.IsTriggerd())
            Left();
        else if (InputUtility.Right.IsTriggerd())
            Right();
        else if (InputUtility.Submit.IsTriggerd())
            items[currentSelected].Submit();
        if (prevIndex != currentSelected) UpdateView();
    }
}
