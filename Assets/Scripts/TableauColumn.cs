using System.Collections.Generic;
using UnityEngine;

public class TableauColumn : MonoBehaviour, ICardContainer
{
    public List<Card> cards = new List<Card>();
    public float cardSpacing = 25f; // Spacing between cards
    
    public void AddCard(Card card)
    {
        cards.Add(card);
        card.transform.SetParent(transform);
        UpdateCardPositions();
    }
    
    public void RemoveCard(Card card)
    {
        cards.Remove(card);
        UpdateCardPositions();
    }
    
    void UpdateCardPositions()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            RectTransform rectTransform = cards[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Position cards vertically with spacing
                rectTransform.anchoredPosition = new Vector2(0, -i * cardSpacing);
                
                // Set proper layering (later cards appear on top)
                rectTransform.SetSiblingIndex(i);
            }
        }
    }
    
    public bool CanPlaceCard(Card card)
    {
        if (cards.Count == 0) return true; // Empty column accepts any card
        
        Card topCard = cards[cards.Count - 1];
        // Must be alternating colors and descending rank
        return topCard.IsRed() != card.IsRed() && 
               topCard.GetRankValue() == card.GetRankValue() + 1;
    }
}