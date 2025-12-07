using System.Collections.Generic;
using UnityEngine;

public class TableauColumn : MonoBehaviour, ICardContainer
{
    public List<Card> cards = new List<Card>();
    public float cardSpacing = 35f;
    
    public void AddCard(Card card)
    {
        cards.Add(card);
        card.transform.SetParent(transform, false);

        card.transform.SetAsLastSibling();

        UpdateCardPositions();
    }
    
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
        UpdateCardPositions();
    }
    
    public void UpdateCardPositions()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform rectTransform = cards[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(0, -i * cardSpacing);
                rectTransform.SetSiblingIndex(i);
            }
        }
    }
    
    public bool CanPlaceCard(Card card)
    {
        if (cards.Count == 0) return true;

        Card topCard = cards[cards.Count - 1];
        return topCard.IsRed() != card.IsRed() && 
               topCard.GetRankValue() == card.GetRankValue() + 1;
    }
}
