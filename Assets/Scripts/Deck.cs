using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public GameObject cardPrefab;
    public List<Card> cards = new List<Card>();
    
    public void CreateDeck()
    {
        Debug.Log("=== CreateDeck() START ===");
        cards.Clear();
        
        if (cardPrefab == null)
        {
            Debug.LogError("CardPrefab is NULL!");
            return;
        }
        
        Transform cardParent = GameObject.Find("CardParent")?.transform;
        
        if (cardParent == null)
        {
            Debug.LogError("CardParent not found!");
            return;
        }
        
        Debug.Log($"Creating cards under: {cardParent.name}");
        Debug.Log($"CardParent children before: {cardParent.childCount}");
        
        int cardIndex = 0;
        foreach (Card.Suit suit in System.Enum.GetValues(typeof(Card.Suit)))
        {
            foreach (Card.Rank rank in System.Enum.GetValues(typeof(Card.Rank)))
            {
                GameObject cardObj = Instantiate(cardPrefab, cardParent);
                
                if (cardObj == null)
                {
                    Debug.LogError($"Failed to instantiate card {rank} of {suit}");
                    continue;
                }
                
                // Set a visible position for debugging
                RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = Vector2.zero; // Center position
                }
                
                Card card = cardObj.GetComponent<Card>();
                
                if (card == null)
                {
                    Debug.LogError($"Card component missing on {cardObj.name}");
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
        
        Debug.Log($"CardParent children after: {cardParent.childCount}");
        Debug.Log($"Total cards created: {cards.Count}");
        Debug.Log("=== CreateDeck() END ===");
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