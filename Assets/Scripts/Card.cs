using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    public enum Suit { Hearts, Diamonds, Clubs, Spades }
    public enum Rank { Ace = 1, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
    
    public Suit suit;
    public Rank rank;
    public bool isFaceUp = true;
    
    void Start()
    {
        UpdateVisuals();
    }
    
    public Color GetColor()
    {
        return (suit == Suit.Hearts || suit == Suit.Diamonds) ? Color.red : Color.black;
    }
    
    public bool IsRed()
    {
        return suit == Suit.Hearts || suit == Suit.Diamonds;
    }
    
    public int GetRankValue()
    {
        return (int)rank;
    }
    
    public string GetDisplayString()
    {
        string rankStr = rank.ToString();
        if (rank == Rank.Ace) rankStr = "A";
        else if (rank == Rank.Jack) rankStr = "J";
        else if (rank == Rank.Queen) rankStr = "Q";
        else if (rank == Rank.King) rankStr = "K";
        else rankStr = ((int)rank).ToString();
        
        string suitSymbol = "";
        switch (suit)
        {
            case Suit.Hearts: suitSymbol = "♥"; break;
            case Suit.Diamonds: suitSymbol = "♦"; break;
            case Suit.Clubs: suitSymbol = "♣"; break;
            case Suit.Spades: suitSymbol = "♠"; break;
        }
        
        return rankStr + suitSymbol;
    }
    
    public void UpdateVisuals()
    {
        Image cardImage = GetComponent<Image>();
        if (cardImage != null && CardSpriteManager.Instance != null)
        {
            Sprite cardSprite = CardSpriteManager.Instance.GetCardSprite(suit, rank);
            if (cardSprite != null)
            {
                cardImage.sprite = cardSprite;
                cardImage.color = Color.white;
            }
        }

        Outline outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        outline.effectDistance = new Vector2(2, -2);
        outline.useGraphicAlpha = true;

        TextMeshProUGUI tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
        {
            Destroy(tmpText.gameObject);
        }
    }
}
