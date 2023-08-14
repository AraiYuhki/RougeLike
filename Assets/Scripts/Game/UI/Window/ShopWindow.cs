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
    private CardController cardController;
    [SerializeField]
    private Player player;
    [SerializeField]
    private Image window;
    [SerializeField]
    private TabGroups tabGroups;
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private ShopCard originalCard;

    [SerializeField]
    private GridScrollMenu shopMenu;
    [SerializeField]
    private GridScrollMenu deckMenu;

    private Sequence tween;

    private GridScrollMenu currentMenu
    {
        get
        {
            if (tabGroups.SelectIndex == (int)TabType.Shop)
                return shopMenu;
            return deckMenu;
        }
    }

    public DungeonStateMachine StateMachine { get; set; }

    public Action OnClose { get; set; }
    public bool IsOpened { get; private set; }

    private void Start()
    {
        shopMenu.OnSubmit = selectedItem =>
        {
            if (selectedItem is ShopCard card)
            {
                shopMenu.Enable = false;
                var data = card.Data;
                StateMachine.OpenCommonDialog("確認", $"{data.Name}を{data.Price}Gで購入しますか？",
                    ("はい", () => {
                        BuyCard(data);
                        shopMenu.enabled = true;
                        StateMachine.Goto(GameState.Shop);
                    }
                ),
                    ("いいえ", () => {
                        // ショップステートに戻す
                        shopMenu.Enable = true;
                        StateMachine.Goto(GameState.Shop);
                    }
                )
                    );
            }
        };

        deckMenu.OnSubmit = selectedItem =>
        {
            if (selectedItem is ShopCard card)
            {
                deckMenu.Enable = false;
                var data = card.Data;
                StateMachine.OpenCommonDialog("確認", $"{card.Data.Name}を200Gで破棄しますか？",
                    ("はい", () =>
                    {
                        deckMenu.RemoveItem(selectedItem);
                        RemoveCard(card, selectedItem);
                        deckMenu.Enable = true;
                        StateMachine.Goto(GameState.Shop);
                    }
                ),
                    ("いいえ", () =>
                    {
                        // ショップステートに戻す
                        deckMenu.enabled = true;
                        StateMachine.Goto(GameState.Shop);
                    }
                ));
            }
        };
    }

    public void Open(Action onComplete = null)
    {
        if (IsOpened) return;
        IsOpened = true;
        tween?.Kill();

        tabGroups.SelectIndex = (int)TabType.Shop;
        shopMenu.Enable = true;
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
            if (tabGroups.SelectIndex != (int)TabType.Deck) return;
            InitializeDeck();
        };
    }

    public void Close()
    {
        if (!IsOpened) return;
        IsOpened = false;
        tween?.Kill();
        tween = DOTween.Sequence();
        tween.Append(canvasGroup.DOFade(0f, 0.2f));
        tween.Append(window.rectTransform.DOScale(0.5f, 0.2f));
        tween.OnComplete(() =>
        {
            tween = null;
            OnClose?.Invoke();
            gameObject.SetActive(false);
        });
    }

    public void Right() => currentMenu.Right();
    public void Left() => currentMenu.Left();
    public void Up() => currentMenu.Up();
    public void Down() => currentMenu.Down();
    public void RightTrigger() => tabGroups.SelectIndex++;
    public void LeftTrigger() => tabGroups.SelectIndex--;
    public void Submit() => currentMenu.Submit();

    public void InitializeShop()
    {
        shopMenu.Clear();
        var cards = DataBase.Instance.GetTable<MCard>().Data.Randmize().Take(3).ToList();
        foreach (var data in cards)
        {
            var card = Instantiate(originalCard);
            card.SetData(data, null);
            card.Enable = player.Data.Gems >= data.Price;
            shopMenu.AddItem(card);
        }
        shopMenu.Initialize();
    }

    public void InitializeDeck()
    {
        deckMenu.Clear();
        var canRemove = player.Data.Gems >= 200;
        foreach (var card in cardController.AllCards)
        {
            var obj = Instantiate(originalCard);
            obj.SetData(card.Data, card);
            obj.Enable = canRemove;
            deckMenu.AddItem(obj);
        }
        deckMenu.Initialize();
    }

    private void BuyCard(CardData data)
    {
        cardController.Add(data);
        player.Data.Gems -= data.Price;
        UpdateDeck();
        UpdateShop();
    }

    private void RemoveCard(ShopCard shopCard, SelectableItem item)
    {
        player.Data.Gems -= 200;
        cardController.Remove(shopCard.Card);
        deckMenu.RemoveItem(item);
        UpdateDeck();
        UpdateShop();
    }

    private void UpdateShop()
    {
        foreach (var card in shopMenu.Items.Select(item => item as ShopCard))
            card.Enable = card.Price <= player.Data.Gems;
    }

    private void UpdateDeck()
    {
        var canRemove = player.Data.Gems >= 200;
        foreach (var card in deckMenu.Items.Select(item => item as ShopCard))
            card.Enable = canRemove;
        deckMenu.ReselectCurrentItem();
    }
}
