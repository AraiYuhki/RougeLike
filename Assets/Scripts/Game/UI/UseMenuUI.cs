using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UseMenuUI : MonoBehaviour
{
    [SerializeField]
    private SelectableItem template = null;
    [SerializeField]
    private List<string> menuItems = new List<string>();
    [SerializeField]
    private List<SelectableItem> items = new List<SelectableItem>();

    private int selectedIndex = 0;
    private Tween tween;

    private void OnDestroy()
    {
        tween?.Complete();
        tween = null;
    }

    public void Initialize(IEnumerable<(string, UnityAction)> items)
    {
        foreach (var item in this.items) Destroy(item.gameObject);
        this.items.Clear();

        foreach ((var label, var callback, var index) in items.Select((pair, index) => (pair.Item1, pair.Item2, index)))
        {
            var item = Instantiate(template, transform);
            item.gameObject.SetActive(true);
            item.Initialize(() => SetSelectIndex(index), () => callback());
            item.GetComponentInChildren<Text>().text = label;
            this.items.Add(item);
        }
    }

    public void Open()
    {
        if (tween != null)
        {
            tween.Kill();
            tween = null;
        }
        tween = GetComponent<CanvasGroup>().DOFade(1.0f, 0.2f);
        tween.OnComplete(() => tween = null);
    }

    public void Close()
    {
        if (tween != null)
        {
            tween.Kill();
            tween = null;
        }
        tween = GetComponent<CanvasGroup>().DOFade(0f, 0.2f);
        tween.OnComplete(() => tween = null);
    }

    private void SetSelectIndex(int index)
    {
        selectedIndex = index;
        FixIndex();
    }

    private void FixIndex()
    {
        if (selectedIndex >= items.Count) selectedIndex -= items.Count;
        else if (selectedIndex < 0) selectedIndex -= items.Count;
    }

    public void Up()
    {
        items[selectedIndex].Select(false);
        selectedIndex++;
        FixIndex();
        items[selectedIndex].Select(true);
    }


    public void Down()
    {
        items[selectedIndex].Select(false);
        selectedIndex--;
        FixIndex();
        items[selectedIndex].Select(true);
    }

    public void Submit()
    {

    }

    public void Cancel()
    {

    }
}
