using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ShopWindow : MonoBehaviour
{
    private enum TabType
    {
        Shop = 0,
        Deck = 1,
    }

    [SerializeField]
    private Image window;
    [SerializeField]
    private TabGroups tabGroups;
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private ScrollRect scrollView;
    [SerializeField]
    private Transform shopContainer;
    [SerializeField]
    private Transform deckContainer;
    [SerializeField]
    private ShopCard originalCard;

    private List<ShopCard> shopCards = new List<ShopCard>();
    private List<ShopCard> deckCards = new List<ShopCard>();
    private Sequence tween;

    private int columnCount = 0;
    private int GetRowCount(int elementCounts) => Mathf.CeilToInt(elementCounts / columnCount) + 1;
    private int GetIndex(Vector2Int index) => columnCount * index.y + index.x;

    private Vector2Int shopSelectCardIndex = Vector2Int.zero;
    private Vector2Int deckSelectCardIndex = Vector2Int.zero;

    public Action OnClose { get; set; }
    public bool IsOpened { get; private set; }

    private void Start()
    {
        // 列数は事前に計算しておく
        columnCount = Mathf.FloorToInt(window.rectTransform.sizeDelta.x / 200f);
    }

    public void Open(Action onComplete = null)
    {
        if (IsOpened) return;
        IsOpened = true;
        tween?.Kill();
        gameObject.SetActive(true);
        tween = DOTween.Sequence();
        tween.Append(canvasGroup.DOFade(1f, 0.2f));
        tween.Join(window.rectTransform.DOScale(1f, 0.2f));
        tween.OnComplete(() =>
        {
            tween = null;
            onComplete?.Invoke();
        });
        InitializeShop();
        tabGroups.OnChangeTab = () =>
        {
            if (tabGroups.SelectIndex != 1) return;
            InitializeDeck();
        };
    }

    public void Close()
    {
        if (!IsOpened) return;
        IsOpened = false;
        tween?.Kill();
        gameObject.SetActive(false);
        tween = DOTween.Sequence();
        tween.Append(canvasGroup.DOFade(0f, 0.2f));
        tween.Append(window.rectTransform.DOScale(0.5f, 0.2f));
        tween.OnComplete(() =>
        {
            tween = null;
            OnClose?.Invoke();
        });
    }

    public IEnumerator Controll()
    {
        if (InputUtility.RightTrigger.IsTriggerd())
            tabGroups.SelectIndex++;
        else if (InputUtility.LeftTrigger.IsTriggerd())
            tabGroups.SelectIndex--;

        var move = Vector2Int.zero;
        if (InputUtility.Right.IsTriggerd())
            move.x = 1;
        else if (InputUtility.Left.IsTriggerd())
            move.x = -1;
        if (InputUtility.Up.IsTriggerd())
            move.y = -1;
        else if (InputUtility.Down.IsTriggerd())
            move.y = 1;

        if (tabGroups.SelectIndex == (int)TabType.Shop)
        {
            var prevIndex = shopSelectCardIndex;
            shopSelectCardIndex = FixIndex(shopSelectCardIndex + move, shopCards.Count);
            if (prevIndex.x != shopSelectCardIndex.x || prevIndex.y != shopSelectCardIndex.y)
            {
                shopCards[GetIndex(prevIndex)].Select(false);
                shopCards[GetIndex(shopSelectCardIndex)].Select(true);
            }
        }
        else if (tabGroups.SelectIndex == (int)TabType.Deck)
        {
            var prevIndex = deckSelectCardIndex;
            deckSelectCardIndex = FixIndex(deckSelectCardIndex + move, deckCards.Count);
            if (prevIndex.x != deckSelectCardIndex.x || prevIndex.y != deckSelectCardIndex.y)
            {
                deckCards[GetIndex(prevIndex)].Select(false);
                deckCards[GetIndex(deckSelectCardIndex)].Select(true);
            }
        }

        yield return null;
    }

    private Vector2Int FixIndex(Vector2Int index, int elementCount)
    {
        // 行数を取得
        var rowCount = GetRowCount(elementCount);
        Debug.LogError(index);
        // Y軸のインデックスを補正する
        if (index.y < 0) index.y += rowCount;
        else if (index.y >= rowCount) index.y -= rowCount;

        // 現在の行の列数を算出する
        var currentLineColumnCount = columnCount;
        var lastIndex = (index.y + 1) * columnCount - 1; // 現在の行の末尾のインデックスを算出
        // 現在の行末のインデックスが要素数より大きい場合は、列数が足りていないので差分だけ減らす
        if (lastIndex > elementCount)
        {
            var diff = lastIndex - elementCount;
            currentLineColumnCount = columnCount - (diff + 1);
            index.x = Mathf.Min(index.x, currentLineColumnCount);
        }
        // X軸のインデックスを補正する
        if (index.x < 0) index.x += currentLineColumnCount;
        else if (index.x >= currentLineColumnCount) index.x -= currentLineColumnCount;

        return index;
    }

    public void InitializeShop()
    {
        foreach (var card in shopCards)
            Destroy(card.gameObject);
        shopCards.Clear();

        foreach(var data in DataBase.Instance.GetTable<MCard>().Data)
        {
            var card = Instantiate(originalCard, shopContainer);
            card.SetData(data, () =>
            {
                var dialog = DialogManager.Instance.Open<CommonDialog>();
                dialog.Initialize("確認", $"{data.Name}を{data.Price}Gで購入しますか？", ("はい", () =>
                {
                    DialogManager.Instance.Close(dialog);
                    ServiceLocator.Instance.GameController.CardController.Add(data);
                    ServiceLocator.Instance.GameController.Player.Data.Gems -= data.Price;
                    UpdateDeck();
                    UpdateShop();
                }
                ),
                ("いいえ", () =>
                {
                    DialogManager.Instance.Close(dialog);
                }));
            });
            card.Enable = card.Price <= ServiceLocator.Instance.GameController.Player.Data.Gems;
            shopCards.Add(card);
        }
        shopCards[GetIndex(shopSelectCardIndex)].Select(true);
    }

    public void InitializeDeck()
    {
        foreach (var card in deckCards)
            Destroy(card.gameObject);
        deckCards.Clear();

        var cardController = ServiceLocator.Instance.GameController.CardController;
        var canRemove = ServiceLocator.Instance.GameController.Player.Data.Gems >= 200;

        foreach (var card in ServiceLocator.Instance.GameController.CardController.AllCards)
        {
            var obj = Instantiate(originalCard, deckContainer);
            obj.SetData(card.Data, () =>
            {
                var dialog = DialogManager.Instance.Open<CommonDialog>();
                dialog.Initialize("確認", $"{card.Data.Name}を200Gで破棄しますか？", ("はい", () =>
                {
                    DialogManager.Instance.Close(dialog);
                    ServiceLocator.Instance.GameController.Player.Data.Gems -= 200;
                    cardController.Remove(card);
                    Destroy(obj.gameObject);
                    UpdateDeck();
                    UpdateShop();
                }
                ),
                ("いいえ", () =>
                {
                    DialogManager.Instance.Close(dialog);
                }));
            });
            obj.Enable = canRemove;
        }
        deckCards[GetIndex(deckSelectCardIndex)].Select(true);
    }

    private void UpdateShop()
    {
        foreach (var card in shopCards)
            card.Enable = card.Price <= ServiceLocator.Instance.GameController.Player.Data.Gems;
    }

    private void UpdateDeck()
    {
        var canRemove = ServiceLocator.Instance.GameController.Player.Data.Gems >= 200;
        foreach (var card in deckCards)
            card.Enable = canRemove;
    }
}
