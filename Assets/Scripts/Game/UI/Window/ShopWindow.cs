using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopWindow : MonoBehaviour
{
    private enum Mode
    {
        Shop,
        Deck
    }
    [SerializeField]
    private CanvasGroup canvasGroup;

    [Header("ショップ関係")]
    [SerializeField]
    private CanvasGroup shopWindow;
    [SerializeField]
    private HorizontalMenu shopMenu;

    [Header("デッキ関係")]
    [SerializeField]
    private CanvasGroup deckWindow;
    [SerializeField]
    private GridScrollMenu deckMenu;

    [Header("その他")]
    [SerializeField]
    private ShopCard originalCard;
    [SerializeField]
    private SelectableCard originalSelectableCard;
    [SerializeField]
    private Player player;
    [SerializeField]
    private CardController cardController;

    private Mode mode = Mode.Shop;
    private IDungeonStateMachine stateMachine;
    private PlayerData playerData;

    private MenuBase current => mode == Mode.Shop ? shopMenu : deckMenu;

    public bool IsOpen { get; private set; }

    public void Initialize(IDungeonStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        if (player != null)
            playerData = player.Data;
        shopMenu.OnSubmit = BuyCard;
        deckMenu.OnSubmit = RemoveCard;
    }

    public async UniTask Open()
    {
        IsOpen = true;
        gameObject.SetActive(true);
        shopWindow.gameObject.SetActive(true);
        deckWindow.gameObject.SetActive(false);

        deckWindow.alpha = canvasGroup.alpha = 0;
        shopWindow.alpha = 1f;

        mode = Mode.Shop;
        await canvasGroup.DOFade(1f, 0.2f);

        shopMenu.Enable = true;
        deckMenu.Enable = false;
        shopMenu.ReselectCurrentItem();
    }

    public void Close()
    {
        canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            shopMenu.Enable = false;
            deckMenu.Enable = false;
            gameObject.SetActive(false);
            shopWindow.gameObject.SetActive(false);
            deckWindow.gameObject.SetActive(false);
            stateMachine.Goto(GameState.NextFloorLoad);
            IsOpen = false;
        });
    }

#if UNITY_EDITOR
    public void SetPlayerData(PlayerData playerData)
    {
        this.playerData = playerData;
    }
#endif

    private void BuyCard(SelectableItem selectedItem)
    {
        if (selectedItem is SelectableCard item)
        {
            if (item.Type == ShopCardType.GotoOther)
                SwitchMenu().Forget();
            else if (item.Type == ShopCardType.Exit)
                Close();
            return;
        }
        if (selectedItem is not ShopCard card) return;

        shopMenu.Enable = false;
        var data = card.Data;
        stateMachine.OpenCommonDialog("確認", $"{data.Name}を{data.Price}Gで購入しますか？",
            ("はい", () =>
            {
                shopMenu.Enable = true;
                BuyCard(data);
                stateMachine.Goto(GameState.Shop);
            }
        ),
            ("いいえ", () =>
            {
                shopMenu.Enable = true;
                stateMachine.Goto(GameState.Shop);
            }
        ));
    }


    private void BuyCard(CardInfo card)
    {
        cardController.Add(card);
        playerData.Gems -= card.Price;
        UpdateDeck();
        UpdateShop();
    }

    private void RemoveCard(SelectableItem selectedItem)
    {
        if (selectedItem is SelectableCard item)
        {
            if (item.Type == ShopCardType.GotoOther)
                SwitchMenu().Forget();
            else if (item.Type == ShopCardType.Exit)
                Close();
            return;
        }
        if (selectedItem is not ShopCard card) return;

        deckMenu.Enable = false;
        var data = card.Data;
        stateMachine.OpenCommonDialog("確認", $"{card.Data.Name}を200Gで破棄しますか？",
            ("はい", () =>
            {
                deckMenu.RemoveItem(selectedItem);
                RemoveCard(card, selectedItem);
                deckMenu.Enable = true;
                stateMachine.Goto(GameState.Shop);
            }
        ),
            ("いいえ", () =>
            {
                deckMenu.Enable = true;
                stateMachine.Goto(GameState.Shop);
            }
        ));
    }

    private void RemoveCard(ShopCard shopCard, SelectableItem item)
    {
        playerData.Gems -= 200;
        cardController.Remove(shopCard.Card);
        deckMenu.RemoveItem(item);
        UpdateDeck();
        UpdateShop();
    }

    public void InitializeShop(FloorShopInfo shopSetting)
    {
        var cards = new List<CardInfo>();
        var master = DB.Instance.MCard.All;
        if (shopSetting.isSellAll)
            cards = master.Randmize().Take(3).ToList();
        else
            cards = master.Where(card => shopSetting.Cards.Contains(card.Id)).Randmize().Take(3).ToList();
        shopMenu.Clear();
        foreach (var cardInfo in cards)
        {
            var product = Instantiate(originalCard);
            product.gameObject.SetActive(true);
            product.Enable = playerData.Gems >= cardInfo.Price;
            shopMenu.AddItem(product);
            product.SetData(cardInfo, null, false);
        }
        shopMenu.AddItem(CreateSelectable("破棄", ShopCardType.GotoOther));
        shopMenu.AddItem(CreateSelectable("店から出る", ShopCardType.Exit));
        shopMenu.Initialize();
    }

    public void InitializeDeck() => InitializeDeck(cardController.AllCards);

    public void InitializeDeck(List<Card> deck)
    {
        deckMenu.Clear();
        deckMenu.Initialize();
        var canRemove = player.Data.Gems >= 200;
        deckMenu.AddItem(CreateSelectable("購入", ShopCardType.GotoOther));
        deckMenu.AddItem(CreateSelectable("店から出る", ShopCardType.Exit));
        foreach (var card in deck.OrderBy(card => card.Data.Id))
        {
            var obj = Instantiate(originalCard);
            obj.SetData(card.Data, card, true);
            obj.Enable = canRemove;
            deckMenu.AddItem(obj);
        }
        deckMenu.Initialize();
    }

    private SelectableCard CreateSelectable(string label, ShopCardType type)
    {
        var card = Instantiate(originalSelectableCard);
        card.Setup(label, type);
        return card;
    }

    private void UpdateShop()
    {
        foreach (var card in shopMenu.Items.Select(item => item as ShopCard).Where(item => item != null))
            card.Enable = card.Price <= player.Data.Gems;
    }

    private void UpdateDeck()
    {
        var canRemove = player.Data.Gems >= 200;
        foreach (var card in deckMenu.Items.Select(item => item as ShopCard).Where(item => item != null))
            card.Enable = canRemove;
        deckMenu.ReselectCurrentItem();
    }

    private async UniTask SwitchMenu()
    {
        if (mode == Mode.Shop)
        {
            mode = Mode.Deck;
            deckWindow.gameObject.SetActive(true);
            shopMenu.Enable = false;

            await DOTween.Sequence()
                .Append(shopWindow.transform.DOScale(0.8f, 0.2f))
                .Join(shopWindow.DOFade(0f, 0.2f))
                .Join(deckWindow.transform.DOScale(1f, 0.2f))
                .Join(deckWindow.DOFade(1f, 0.2f));

            deckMenu.Enable = true;
            shopWindow.gameObject.SetActive(false);
        }
        else
        {
            mode = Mode.Shop;
            shopWindow.gameObject.SetActive(true);
            deckMenu.Enable = false;

            await DOTween.Sequence()
                .Append(deckWindow.transform.DOScale(0.8f, 0.2f))
                .Join(deckWindow.DOFade(0f, 0.2f))
                .Join(shopWindow.transform.DOScale(1f, 0.2f))
                .Join(shopWindow.DOFade(1f, 0.2f));

            shopMenu.Enable = true;
            deckWindow.gameObject.SetActive(false);
        }

    }

    public void Right() => current.Right();
    public void Left() => current.Left();
    public void Up() => current.Up();
    public void Down() => current.Down();
    public void Submit() => current.Submit();
}
