using UnityEngine;

public class FreeCell : MonoBehaviour, ICardContainer
{
    public Card card = null;
    
    public bool CanPlaceCard(Card newCard)
    {
        return card == null;
    }
    
    public void AddCard(Card newCard)
    {
        card = newCard;
        card.transform.SetParent(transform, false);
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
