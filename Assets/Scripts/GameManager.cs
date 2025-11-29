using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    private Deck deck;
    public GameObject cardPrefab;
    public Transform tableauParent;
    public Transform foundationParent;
    public Transform freeCellParent;
    
    public List<TableauColumn> tableauColumns = new List<TableauColumn>();
    public List<Foundation> foundations = new List<Foundation>();
    public List<FreeCell> freeCells = new List<FreeCell>();
    
    private int moveCount = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        Debug.Log("=== GameManager Start() ===");
        
        if (deck == null)
        {
            Debug.Log("Deck is null, creating new Deck...");
            GameObject deckObj = new GameObject("Deck");
            deck = deckObj.AddComponent<Deck>();
            deck.cardPrefab = cardPrefab;
            Debug.Log("Deck created. CardPrefab assigned: " + (cardPrefab != null ? cardPrefab.name : "NULL"));
        }
        else
        {
            Debug.Log("Deck already exists");
        }
        
        SetupGame();
        Debug.Log("=== GameManager Start() COMPLETE ===");
    }
    
    void SetupGame()
    {
        Debug.Log("=== SetupGame() START ===");
        
        // Create 8 tableau columns
        for (int i = 0; i < 8; i++)
        {
            GameObject colObj = new GameObject("TableauColumn_" + i);
            colObj.transform.SetParent(tableauParent);
            
            RectTransform rect = colObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 400);
            rect.anchoredPosition = new Vector2(-315 + (i * 90), 0);
            
            UnityEngine.UI.Image bgImage = colObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.2f, 0.4f, 0.2f, 0.3f);
            
            TableauColumn col = colObj.AddComponent<TableauColumn>();
            DropZone dropZone = colObj.AddComponent<DropZone>();
            dropZone.zoneType = DropZone.ZoneType.Tableau;
            tableauColumns.Add(col);
        }
        
        // Create 4 foundations
        for (int i = 0; i < 4; i++)
        {
            GameObject foundObj = new GameObject("Foundation_" + i);
            foundObj.transform.SetParent(foundationParent);
            
            RectTransform rect = foundObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 110);
            rect.anchoredPosition = new Vector2(200 + (i * 90), 250);
            
            UnityEngine.UI.Image bgImage = foundObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.3f, 0.3f, 0.5f, 0.5f);
            
            Foundation found = foundObj.AddComponent<Foundation>();
            DropZone dropZone = foundObj.AddComponent<DropZone>();
            dropZone.zoneType = DropZone.ZoneType.Foundation;
            foundations.Add(found);
        }
        
        // Create 4 free cells
        for (int i = 0; i < 4; i++)
        {
            GameObject cellObj = new GameObject("FreeCell_" + i);
            cellObj.transform.SetParent(freeCellParent);
            
            RectTransform rect = cellObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 110);
            rect.anchoredPosition = new Vector2(-500 + (i * 90), 250);
            
            UnityEngine.UI.Image bgImage = cellObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.5f, 0.3f, 0.3f, 0.5f);
            
            FreeCell cell = cellObj.AddComponent<FreeCell>();
            DropZone dropZone = cellObj.AddComponent<DropZone>();
            dropZone.zoneType = DropZone.ZoneType.FreeCell;
            freeCells.Add(cell);
        }
        
        Debug.Log("About to call DealCards()");
        DealCards();
        Debug.Log("=== SetupGame() END ===");
    }
    
    void DealCards()
    {
        Debug.Log("=== DealCards() called ===");
        
        if (deck == null)
        {
            Debug.LogError("DECK IS NULL in DealCards()!");
            return;
        }
        
        Debug.Log("Deck exists, calling CreateDeck()...");
        deck.CreateDeck();
        
        Debug.Log("Cards in deck after CreateDeck(): " + deck.cards.Count);
        
        if (deck.cards.Count == 0)
        {
            Debug.LogError("No cards were created!");
            return;
        }
        
        Debug.Log("Calling Shuffle()...");
        deck.Shuffle();
        
        Debug.Log("Starting to deal cards to tableau...");
        int cardIndex = 0;
        for (int col = 0; col < 8; col++)
        {
            int cardsInColumn = (col < 4) ? 7 : 6;
            Debug.Log($"Dealing {cardsInColumn} cards to column {col}");
            
            for (int row = 0; row < cardsInColumn; row++)
            {
                if (cardIndex >= deck.cards.Count)
                {
                    Debug.LogError($"Ran out of cards at index {cardIndex}!");
                    return;
                }
                
                Card card = deck.cards[cardIndex];
                Debug.Log($"  Dealing card {cardIndex}: {card.GetDisplayString()} to column {col}");
                tableauColumns[col].AddCard(card);
                cardIndex++;
            }
        }
        
        Debug.Log($"=== DealCards() complete. Dealt {cardIndex} cards ===");
    }
    
    // Phương thức cho kéo thả thủ công (single card)
    public void MoveCard(Card card, ICardContainer from, ICardContainer to)
    {
        from.RemoveCard(card);
        to.AddCard(card);
        
        IncrementMoveCount();
        CheckWinCondition();
    }
    
    // Phương thức tăng số nước đi
    public void IncrementMoveCount()
    {
        moveCount++;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoveCount(moveCount);
        }
    }
    
    // Kiểm tra thắng
    public void CheckWinCondition()
    {
        int totalCards = 0;
        foreach (Foundation foundation in foundations)
        {
            totalCards += foundation.cards.Count;
        }
        
        if (totalCards == 52)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWinScreen(moveCount);
            }
        }
    }
    
    public void NewGame()
    {
        // Clear all containers
        foreach (var col in tableauColumns) 
        {
            foreach (var card in col.cards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            col.cards.Clear();
        }
        
        foreach (var found in foundations)
        {
            foreach (var card in found.cards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            found.cards.Clear();
        }
        
        foreach (var cell in freeCells)
        {
            if (cell.card != null)
            {
                Destroy(cell.card.gameObject);
                cell.card = null;
            }
        }
        
        moveCount = 0;
        DealCards();
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoveCount(moveCount);
        }
    }
}
