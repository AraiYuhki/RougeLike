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
    private ShopCard originalCard;

    [SerializeField]
    private GridScrollMenu shopMenu;
    [SerializeField]
    private GridScrollMenu deckMenu;

    private Sequence tween;

    private CardController cardController => ServiceLocator.Instance.GameController.CardController;
    private GridScrollMenu currentMenu
    {
        get
        {
            if (tabGroups.SelectIndex == (int)TabType.Shop)
                return shopMenu;
            return deckMenu;
        }
    }

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
                var dialog = DialogManager.Instance.Open<CommonDialog>();
                dialog.Initialize("�m�F", $"{data.Name}��{data.Price}G�ōw�����܂����H", 
                    ("�͂�", () => {
                        BuyCard(dialog, data);
                        shopMenu.Enable = true;
                    }
                ),
                    ("������", () =>
                    {
                        DialogManager.Instance.Close(dialog);
                        shopMenu.Enable = true;
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
                var dialog = DialogManager.Instance.Open<CommonDialog>();
                dialog.Initialize("�m�F", $"{card.Data.Name}��200G�Ŕj�����܂����H", ("�͂�", () =>
                {
                    deckMenu.RemoveItem(selectedItem);
                    RemoveCard(dialog, card.Card, card);
                    deckMenu.Enable = true;
                }
                ),
                ("������", () =>
                {
                    DialogManager.Instance.Close(dialog);
                    deckMenu.Enable = true;
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
            currentMenu.Right();
        else if (InputUtility.Left.IsTriggerd())
            currentMenu.Left();
        if (InputUtility.Up.IsTriggerd())
            currentMenu.Down();
        else if (InputUtility.Down.IsTriggerd())
            currentMenu.Up();

        if (InputUtility.Submit.IsTriggerd())
        {
            currentMenu.Submit();
        }
        else if (InputUtility.Cancel.IsTriggerd())
        {
            Close();
            yield break;
        }

        yield return null;
    }

    public void InitializeShop()
    {
        shopMenu.Clear();
        foreach ((var data, var index) in DataBase.Instance.GetTable<MCard>().Data.Select((data, index) => (data, index)))
        {
            var card = Instantiate(originalCard);
            card.SetData(data, null);
            card.Enable = ServiceLocator.Instance.GameController.Player.Data.Gems >= data.Price;
            shopMenu.AddItem(card);
        }
        shopMenu.Initialize();
    }

    public void InitializeDeck()
    {
        deckMenu.Clear();
        var canRemove = ServiceLocator.Instance.GameController.Player.Data.Gems >= 200;
        foreach (var card in cardController.AllCards)
        {
            var obj = Instantiate(originalCard);
            obj.SetData(card.Data, card);
            obj.Enable = canRemove;
            deckMenu.AddItem(obj);
        }
        deckMenu.Initialize();
    }

    private void BuyCard(DialogBase dialog, CardData data)
    {
        DialogManager.Instance.Close(dialog);
        ServiceLocator.Instance.GameController.CardController.Add(data);
        ServiceLocator.Instance.GameController.Player.Data.Gems -= data.Price;
        UpdateDeck();
        UpdateShop();
    }

    private void RemoveCard(DialogBase dialog, Card card, ShopCard shopCard)
    {
        DialogManager.Instance.Close(dialog);
        ServiceLocator.Instance.GameController.Player.Data.Gems -= 200;
        cardController.Remove(card);
        deckMenu.RemoveItem(shopCard as SelectableItem);
        UpdateDeck();
        UpdateShop();
    }

    private void UpdateShop()
    {
        foreach (var card in shopMenu.Items.Select(item => item as ShopCard))
            card.Enable = card.Price <= ServiceLocator.Instance.GameController.Player.Data.Gems;
    }

    private void UpdateDeck()
    {
        var canRemove = ServiceLocator.Instance.GameController.Player.Data.Gems >= 200;
        foreach (var card in deckMenu.Items.Select(item => item as ShopCard))
            card.Enable = canRemove;
        deckMenu.ReselectCurrentItem();
    }
}
