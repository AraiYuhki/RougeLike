using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardController : MonoBehaviour
{
    [SerializeField]
    private FloorManager floorManager;
    [SerializeField]
    private EnemyManager enemyManager;
    [SerializeField]
    private Card originalCard;

    [SerializeField]
    private Transform deckContainer;
    [SerializeField]
    private Transform cemetaryContainer;
    [SerializeField]
    private List<Transform> handContainers;

    [SerializeField]
    private TMP_Text deckCardCountLabel;

    private List<Card> deck = new List<Card>();
    private Card[] hands = new Card[4];
    private List<Card> cemetary = new List<Card>();

    public Player Player { get; set; }

    public Card GetHandCard(int handIndex) => hands[handIndex];

    public int AllCardsCount => deck.Count + cemetary.Count + hands.Count(hand => hand != null);
    public List<Card> AllCards => deck.Concat(cemetary).Concat(hands).Where(card => card != null).ToList();

    private void Update()
    {
        deckCardCountLabel.text = deck.Count.ToString();
    }

    public void Initialize()
    {
        foreach (var _ in Enumerable.Range(0, 6))
            AddToDeck(DB.Instance.MCard.GetById(1));
        foreach (var _ in Enumerable.Range(0, 2))
        {
            AddToDeck(DB.Instance.MCard.GetByType(CardType.Charge).First());
            AddToDeck(DB.Instance.MCard.GetByType(CardType.LongRangeAttack).First());
            AddToDeck(DB.Instance.MCard.GetByType(CardType.ResourceAttack).First());
        }
        AddToDeck(DB.Instance.MCard.GetByType(CardType.StaminaHeal).First());
        Shuffle();
        DrawAll();
        Player.Hp = Player.MaxHp;
    }

    public void Add(CardInfo data)
    {
        var card = CreateCard(data, new Vector3(0f, -500f, 0f), transform);
        deck.Add(card);

        var sequence = DOTween.Sequence();
        sequence.Append(card.transform.DOLocalMove(Vector3.zero, 0.2f));
        sequence.AppendInterval(0.4f);
        sequence.OnComplete(() =>
        {
            card.Goto(deckContainer);
            Shuffle();
            DrawAll();
        });
    }

    public void AddToDeck(CardInfo data)
    {
        var card = CreateCard(data, Vector3.zero, deckContainer);
        deck.Add(card);
    }

    private Card CreateCard(CardInfo data, Vector3 position, Transform parent)
    {
        var card = Instantiate(originalCard, parent);
        card.transform.localPosition = position;
        card.transform.localScale = Vector3.one;
        card.SetManager(floorManager, enemyManager);
        card.SetInfo(data, Player);
        card.VisibleFrontSide = false;
        return card;
    }

    public void Shuffle(bool isReset = false)
    {
        if (isReset)
        {
            var sequence = DOTween.Sequence();
            var delay = 0f;
            foreach(var card in hands.Concat(cemetary).Where(card => card != null))
            {
                deck.Add(card);
                card.transform.parent = deckContainer;
                sequence.Insert(delay, card.transform.DOLocalMove(Vector3.zero, 0.2f));
                sequence.Insert(delay, card.transform.DOScale(Vector3.one, 0.2f));
                card.VisibleFrontSide = false;
                delay += 0.05f;
            }
            cemetary.Clear();
            deck.Shuffle();
            for (var index = 0; index < hands.Length; index++) hands[index] = null;
            sequence.OnComplete(() =>
            {
                DrawAll();
            });
            return;
        }
        deck.Shuffle();
    }

    public void Reload()
    {
        var sequence = DOTween.Sequence();
        var delay = 0f;
        foreach (var card in cemetary)
        {
            deck.Add(card);
            card.transform.parent = deckContainer;
            sequence.Insert(delay, card.transform.DOLocalMove(Vector3.zero, 0.2f));
            sequence.Insert(delay, card.transform.DOScale(Vector3.one, 0.2f));
            card.VisibleFrontSide = false;
            delay += 0.05f;
        }
        cemetary.Clear();
        deck.Shuffle();
        sequence.OnComplete(DrawAll);
    }

    public void DrawAll()
    {
        for (var index = 0; index < hands.Length; index++)
            Draw(index);
    }

    public void Draw(int handIndex)
    {
        if (deck.Count <= 0) return;
        if (hands[handIndex] != null) return;
        var card = deck.First();
        deck.Remove(card);
        hands[handIndex] = card;
        card.Goto(handContainers[handIndex], () => card.VisibleFrontSide = true);
    }
    public void Use(int handIndex)
    {
        var card = hands[handIndex];
        if (card == null) return;
        for (var index = 0; index < hands.Length; index++)
        {
            if (hands[index] == card)
            {
                hands[index] = null;
                break;
            }
        }
        cemetary.Add(card);
        card.Goto(cemetaryContainer);
        Draw(handIndex);
        if (hands.All(hand => hand == null)) Reload();
    }

    public void Remove(Card card)
    {
        if (deck.Contains(card)) deck.Remove(card);
        else if (cemetary.Contains(card)) cemetary.Remove(card);
        else
        {
            for (var index = 0; index < hands.Length; index++)
            {
                if (hands[index] == card)
                {
                    hands[index] = null;
                    break;
                }
            }
        }
        Destroy(card.gameObject);
    }

    public List<PassiveEffectInfo> PassiveEffects()
    {
        return hands
            .Where(hand => hand != null && hand.IsPassive)
            .Select(hand => hand.PassiveInfo)
            .ToList();
    }
}
