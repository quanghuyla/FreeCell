using System.Collections.Generic;
using UnityEngine;

public class Foundation : MonoBehaviour, ICardContainer
{
    public List<Card> cards = new List<Card>();
    public Card.Suit targetSuit;
    
    public void AddCard(Card card)
    {
        if (cards.Count == 0)
        {
            targetSuit = card.suit;
        }
        
        cards.Add(card);
        card.transform.SetParent(transform, false);
        
        RectTransform rectTransform = card.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
    }
    
    public bool CanPlaceCard(Card card)
    {
        if (cards.Count == 0)
        {
            return card.rank == Card.Rank.Ace;
        }

        Card topCard = cards[cards.Count - 1];
        return card.suit == topCard.suit && 
               card.GetRankValue() == topCard.GetRankValue() + 1;
    }
}
