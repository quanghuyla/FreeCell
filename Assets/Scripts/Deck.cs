using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public GameObject cardPrefab;
    public List<Card> cards = new List<Card>();
    
    public void CreateDeck()
    {
        cards.Clear();

        if (cardPrefab == null)
        {
            return;
        }
        
        Transform cardParent = GameObject.Find("CardParent")?.transform;
        
        if (cardParent == null)
        {
            return;
        }
        
        int cardIndex = 0;
        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            foreach (Card.Rank rank in System.Enum.GetValues(typeof(Card.Rank)))
            {
                GameObject cardObj = Instantiate(cardPrefab, cardParent);
                
                if (cardObj == null)
                {
                    continue;
                }
                
                RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                
                Card card = cardObj.GetComponent<Card>();
                
                if (card == null)
                {
                    Destroy(cardObj);
                    continue;
                }
                
                card.suit = suit;
                card.rank = rank;
                cardObj.name = card.GetDisplayString();
                cards.Add(card);
                cardIndex++;
            }
        }
        
    }
    
    public void Shuffle()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(i, cards.Count);
            Card temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }
}
