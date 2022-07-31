using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    private ScrollRect scrollView;
    [SerializeField]
    private ItemRowController template;
    [SerializeField]
    private Transform baseObject;
    [SerializeField]
    private CanvasGroup group;

    private Player player;
    private PlayerData data => player.Data;
    private List<ItemRowController> items = new List<ItemRowController>();
    public CanvasGroup Group => group;

    private int selectedIndex = 0;
    private Tweener animationTween = null;
    private int itemNumInPage = 7;
    private int topIndex = 0;
    private int bottomIndex = 0;

    public ItemBase SelectedItem => items[selectedIndex].ItemData;

    public void Start()
    {
        var rectTransform = scrollView.GetComponent<RectTransform>();
        itemNumInPage = Mathf.FloorToInt(rectTransform.rect.height / 40);
        bottomIndex = itemNumInPage;
    }

    public void Up()
    {
        items[selectedIndex].Select(false);
        selectedIndex--;
        if (selectedIndex < 0) selectedIndex += items.Count;
        items[selectedIndex].Select(true);
        FixIndex();
        FixScroll();
    }

    public void Down()
    {
        items[selectedIndex].Select(false);
        selectedIndex++;
        if (selectedIndex >= items.Count) selectedIndex -= items.Count;
        items[selectedIndex].Select(true);
        FixIndex();
        FixScroll();
    }

    private void FixIndex()
    {
        if (selectedIndex < topIndex)
        {
            topIndex = selectedIndex;
            bottomIndex = selectedIndex + itemNumInPage;
        }
        else if(selectedIndex > bottomIndex)
        {
            bottomIndex = selectedIndex;
            topIndex = selectedIndex - itemNumInPage;
        }
    }

    private void FixScroll()
    {
        if (bottomIndex == items.Count - 1)
            scrollView.verticalNormalizedPosition = 0f;
        else
            scrollView.verticalNormalizedPosition = 1f - (float)topIndex / (items.Count - itemNumInPage);
    }

    private void SetSelectIndex(int index)
    {
        selectedIndex = index;
        FixIndex();
    }

    public void Update() => Debug.LogError(scrollView.verticalNormalizedPosition);

    public void Submit()
    {

    }
    public void Cancel()
    {

    }

    public void Initialize(Player player) => this.player = player;

    public void Open(TweenCallback onComplete = null)
    {
        if (animationTween != null)
        {
            animationTween.Complete();
            animationTween = null;
        }
        gameObject.SetActive(true);
        foreach (var item in items) Destroy(item.gameObject);
        items.Clear();
        foreach((var pair, var index) in data.Inventory.Select((v, index) => (v, index)))
        {
            var item = Instantiate(template, baseObject);
            item.gameObject.SetActive(true);
            item.Initialize(pair.Key, pair.Value, () => SetSelectIndex(index));
            items.Add(item);
        }
        group.alpha = 0;
        animationTween = group.DOFade(1f, 0.2f).OnComplete(() =>
        {
            animationTween = null;
            onComplete?.Invoke();
        });
    }
    public void Close(TweenCallback onComplete = null)
    {
        if (animationTween != null)
        {
            animationTween.Complete();
            animationTween = null;
        }
        group.alpha = 1f;
        animationTween = group.DOFade(0f, 0.2f).OnComplete(() =>
        {
            animationTween = null;
            onComplete?.Invoke();
            gameObject.SetActive(false);
        });
    }
}
