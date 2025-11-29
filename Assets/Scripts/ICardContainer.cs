public interface ICardContainer
{
    void AddCard(Card card);
    void RemoveCard(Card card);
    bool CanPlaceCard(Card card);
}