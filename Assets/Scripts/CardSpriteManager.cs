using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardSpriteManager : MonoBehaviour
{
    public static CardSpriteManager Instance;

    [Header("Card Sprites Folder Path")]
    [Tooltip("Path to card sprites folder in Assets")]
    public string cardSpritesPath = "Assets/Sprites/Playing Cards/Playing Cards/PNG-cards-1.3";

    private Dictionary<string, Sprite> cardSprites = new Dictionary<string, Sprite>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadAllCardSprites();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadAllCardSprites()
    {
#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { cardSpritesPath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object obj in sprites)
            {
                if (obj is Sprite sprite)
                {
                    cardSprites[sprite.name] = sprite;
                }
            }
        }
#endif
    }

    public Sprite GetCardSprite(Card.Suit suit, Card.Rank rank)
    {
        string rankName = GetRankName(rank);
        string suitName = GetSuitName(suit);
        string spriteName = $"{rankName}_of_{suitName}_0";

        if (cardSprites.ContainsKey(spriteName))
        {
            return cardSprites[spriteName];
        }

        return null;
    }

    string GetRankName(Card.Rank rank)
    {
        switch (rank)
        {
            case Card.Rank.Ace: return "ace";
            case Card.Rank.Two: return "2";
            case Card.Rank.Three: return "3";
            case Card.Rank.Four: return "4";
            case Card.Rank.Five: return "5";
            case Card.Rank.Six: return "6";
            case Card.Rank.Seven: return "7";
            case Card.Rank.Eight: return "8";
            case Card.Rank.Nine: return "9";
            case Card.Rank.Ten: return "10";
            case Card.Rank.Jack: return "jack";
            case Card.Rank.Queen: return "queen";
            case Card.Rank.King: return "king";
            default: return "";
        }
    }

    string GetSuitName(Card.Suit suit)
    {
        switch (suit)
        {
            case Card.Suit.Hearts: return "hearts";
            case Card.Suit.Diamonds: return "diamonds";
            case Card.Suit.Clubs: return "clubs";
            case Card.Suit.Spades: return "spades";
            default: return "";
        }
    }
}
