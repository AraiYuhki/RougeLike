using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScrollMenu : MonoBehaviour, IControllable
{
    [SerializeField]
    protected ScrollRect scrollView;
    [SerializeField]
    protected SelectableItem template;
    [SerializeField]
    protected Transform baseObject;
    [SerializeField]
    protected CanvasGroup group;
    protected List<string> menuItems = new List<string>();
    protected List<SelectableItem> items = new List<SelectableItem>();

    public bool UseScrollView => scrollView != null;

    protected int selectedIndex = 0;
    protected int itemNumInPage;
    protected int topIndex = 0;
    protected int bottomIndex = 0;

    private Tween tween;

    protected void Start()
    {
        if (!UseScrollView)
            return;
        var rectTransform = scrollView.GetComponent<RectTransform>();
        itemNumInPage = Mathf.FloorToInt(rectTransform.rect.height / 40);
        bottomIndex = itemNumInPage;
    }
    protected void OnDestroy()
    {
        tween?.Kill();
        tween = null;
    }

    protected void Clean()
    {
        foreach (var item in this.items) Destroy(item.gameObject);
        this.items.Clear();
    }

    public virtual void Initialize(IEnumerable<(string, Action)> items)
    {
        Clean();
        foreach ((var label, var callback, var index) in items.Select((pair, index) => (pair.Item1, pair.Item2, index)))
        {
            var item = Instantiate(template, baseObject);
            item.gameObject.SetActive(true);
            item.Initialize(() => SetSelectIndex(index), () => callback());
            item.GetComponentInChildren<TMP_Text>().text = label;
            this.items.Add(item);
        }
    }

    public virtual void Open(Action onComplete = null)
    {
        if (tween != null)
        {
            tween.Kill();
            tween = null;
        }
        selectedIndex = 0;
        group.alpha = 0f;
        gameObject.SetActive(true);
        tween = group.DOFade(1.0f, 0.2f);
        tween.OnComplete(() =>
        {
            tween = null;
            onComplete?.Invoke();
            items[selectedIndex].Select(true);
        });
    }

    public virtual void Close(Action onComplete = null)
    {
        if (tween != null)
        {
            tween.Kill();
            tween = null;
        }
        group.alpha = 1f;
        tween = group.DOFade(0f, 0.2f);
        if (items.Count != 0 && items.Count > selectedIndex)
            items[selectedIndex].Select(false);
        tween.OnComplete(() =>
        {
            tween = null;
            onComplete?.Invoke();
            gameObject.SetActive(false);
        });
    }

    public virtual void Submit() => items[selectedIndex].Submit();
    public virtual void Cancel()
    {
    }

    public void Up()
    {
        items[selectedIndex].Select(false);
        selectedIndex--;
        FixIndex();
        FixScroll();
        items[selectedIndex].Select(true);
    }

    public void Down()
    {
        items[selectedIndex].Select(false);
        selectedIndex++;
        FixIndex();
        FixScroll();
        items[selectedIndex].Select(true);
    }

    public virtual void Left() { }
    public virtual void Right() { }

    protected void SetSelectIndex(int index)
    {
        var prevIndex = selectedIndex;
        selectedIndex = index;
        FixIndex();
        if (prevIndex != selectedIndex)
        {
            items[selectedIndex].Select(true);
            items[prevIndex].Select(false);
        }
    }

    protected virtual void FixIndex()
    {
        if (selectedIndex >= items.Count)
            selectedIndex -= items.Count;
        else if (selectedIndex < 0)
            selectedIndex += items.Count;

        if (selectedIndex < topIndex)
        {
            topIndex = selectedIndex;
            bottomIndex = selectedIndex + itemNumInPage;
        }
        else if (selectedIndex > bottomIndex)
        {
            bottomIndex = selectedIndex;
            topIndex = selectedIndex - itemNumInPage;
        }
    }

    protected void FixScroll()
    {
        if (!UseScrollView)
            return;
        if (bottomIndex == items.Count - 1)
            scrollView.verticalNormalizedPosition = 0f;
        else
            scrollView.verticalNormalizedPosition = 1f - (float)topIndex / (items.Count - itemNumInPage);
    }
}
