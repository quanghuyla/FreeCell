using UnityEngine;

public class FreeCell : MonoBehaviour, ICardContainer
{
    public Card card = null;
    
    public bool CanPlaceCard(Card newCard)  // Keep the parameter for interface consistency
    {
        return card == null;  // We just check if the cell is empty
    }
    
    public void AddCard(Card newCard)
    {
        card = newCard;
        card.transform.SetParent(transform);
        card.transform.localPosition = Vector3.zero;
    }
    
    public void RemoveCard(Card cardToRemove)
    {
        if (card == cardToRemove)
        {
            card = null;
        }
    }
    
    public Card TakeCard()
    {
        Card temp = card;
        card = null;
        return temp;
    }
}