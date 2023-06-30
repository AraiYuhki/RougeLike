using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopWindow : MonoBehaviour
{
    [SerializeField]
    private Image window;
    [SerializeField]
    private CanvasGroup canvasGroup;
    [SerializeField]
    private ScrollRect scrollView;
    [SerializeField]
    private Transform container;
    [SerializeField]
    private ShopCard originalCard;

    private List<ShopCard> cards;
    private Sequence tween;

    public void Open(Action onComplete = null)
    {
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
    }

    public void Close(Action onComplete = null)
    {
        tween?.Kill();
        gameObject.SetActive(false);
        tween = DOTween.Sequence();
        tween.Append(canvasGroup.DOFade(0f, 0.2f));
        tween.Append(window.rectTransform.DOScale(0.5f, 0.2f));
        tween.OnComplete(() =>
        {
            tween = null;
            onComplete?.Invoke();
        });
    }

    public void Initialize()
    {
        foreach (var card in cards)
            Destroy(card.gameObject);
        cards.Clear();

        foreach(var data in DataBase.Instance.GetTable<MCard>().Data)
        {
            var card = Instantiate(originalCard, container);
            card.SetData(data, () =>
            {
                var dialog = DialogManager.Instance.Open<CommonDialog>();
                dialog.Initialize("Šm”F", $"{data.Name}‚ð{data.Price}G‚Åw“ü‚µ‚Ü‚·‚©H", ("‚Í‚¢", () =>
                {
                    DialogManager.Instance.Close(dialog);
                    ServiceLocator.Instance.GameController.CardController.Add(data);
                    ServiceLocator.Instance.GameController.Player.Data.Gems -= data.Price;
                }),
                ("‚¢‚¢‚¦", () =>
                {
                    DialogManager.Instance.Close(dialog);
                }));
            });
            cards.Add(card);
        }
    }
}
