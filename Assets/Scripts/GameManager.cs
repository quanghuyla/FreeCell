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

    private struct ThemePalette
    {
        public Color backgroundColor;
        public Color tableauColor;
        public Color freeCellColor;
        public Color foundationColor;
    }

    private ThemePalette activeTheme;

    private class InitialState
    {
        public List<List<Card>> tableauCards = new List<List<Card>>();
        public List<Card> freeCellCards = new List<Card>();
        public List<List<Card>> foundationCards = new List<List<Card>>();
    }

    private class MoveRecord
    {
        public List<Card> cards;
        public ICardContainer fromContainer;
        public ICardContainer toContainer;
    }

    private readonly Stack<MoveRecord> undoStack = new Stack<MoveRecord>();
    private readonly Stack<MoveRecord> redoStack = new Stack<MoveRecord>();
    private readonly InitialState initialState = new InitialState();
    private bool recordInitialStateOnDeal = true;
    private bool hasInitialState = false;
    private WinCelebration winCelebration;
    
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
        string themeName = PlayerPrefs.GetString("Theme", "Green");
        activeTheme = GetThemePalette(themeName);
        ApplyThemeBackground();
        winCelebration = GetComponent<WinCelebration>();

        if (deck == null)
        {
            GameObject deckObj = new GameObject("Deck");
            deck = deckObj.AddComponent<Deck>();
            deck.cardPrefab = cardPrefab;
        }
        SetupGame();
    }

    ThemePalette GetThemePalette(string themeName)
    {
        ThemePalette palette = new ThemePalette
        {
            backgroundColor = new Color(0.05f, 0.2f, 0.05f, 1f),
            tableauColor = new Color(0.2f, 0.4f, 0.2f, 0.35f),
            freeCellColor = new Color(0.5f, 0.3f, 0.3f, 0.5f),
            foundationColor = new Color(0.3f, 0.3f, 0.5f, 0.5f)
        };

        switch (themeName.ToLowerInvariant())
        {
            case "blue":
                palette.backgroundColor = new Color(0.03f, 0.08f, 0.2f, 1f);
                palette.tableauColor = new Color(0.2f, 0.3f, 0.5f, 0.45f);
                palette.freeCellColor = new Color(0.25f, 0.45f, 0.7f, 0.55f);
                palette.foundationColor = new Color(0.45f, 0.65f, 0.85f, 0.55f);
                break;
            case "red":
                palette.backgroundColor = new Color(0.2f, 0.05f, 0.05f, 1f);
                palette.tableauColor = new Color(0.5f, 0.2f, 0.2f, 0.45f);
                palette.freeCellColor = new Color(0.6f, 0.3f, 0.3f, 0.55f);
                palette.foundationColor = new Color(0.75f, 0.4f, 0.4f, 0.55f);
                break;
        }

        return palette;
    }

    void ApplyThemeBackground()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = activeTheme.backgroundColor;
        }
    }

    public void RegisterMove(List<Card> movedCards, ICardContainer from, ICardContainer to)
    {
        if (movedCards == null || movedCards.Count == 0 || from == null || to == null)
        {
            return;
        }

        MoveRecord record = new MoveRecord
        {
            cards = new List<Card>(movedCards),
            fromContainer = from,
            toContainer = to
        };

        undoStack.Push(record);
        redoStack.Clear();

        IncrementMoveCount();
        CheckWinCondition();
    }

    private void ApplyMoveRecord(MoveRecord record, bool forward)
    {
        if (record == null || record.cards == null || record.cards.Count == 0)
        {
            return;
        }

        ICardContainer source = forward ? record.fromContainer : record.toContainer;
        ICardContainer target = forward ? record.toContainer : record.fromContainer;

        if (source == null || target == null)
        {
            return;
        }

        foreach (Card card in record.cards)
        {
            source.RemoveCard(card);
            target.AddCard(card);
        }
    }

    public void UndoMove()
    {
        if (undoStack.Count == 0)
        {
            return;
        }

        MoveRecord record = undoStack.Pop();
        ApplyMoveRecord(record, false);
        redoStack.Push(record);

        if (moveCount > 0)
        {
            moveCount--;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoveCount(moveCount);
            UIManager.Instance.HideWinPanel();
        }

        if (winCelebration != null)
        {
            winCelebration.Stop();
        }
    }

    public void RedoMove()
    {
        if (redoStack.Count == 0)
        {
            return;
        }

        MoveRecord record = redoStack.Pop();
        ApplyMoveRecord(record, true);
        undoStack.Push(record);

        IncrementMoveCount();
        CheckWinCondition();
    }
    
    void SetupGame()
    {
        float cardWidth = 140f;
        float cardHeight = 200f;

        if (cardPrefab != null)
        {
            RectTransform prefabRect = cardPrefab.GetComponent<RectTransform>();
            if (prefabRect != null)
            {
                Vector2 prefabSize = prefabRect.sizeDelta;
                if (prefabSize.x > 0f)
                {
                    cardWidth = prefabSize.x;
                }
                if (prefabSize.y > 0f)
                {
                    cardHeight = prefabSize.y;
                }
            }
        }

        EnsureParentAnchors();

        float highlightPadding = 12f;
        float highlightWidth = cardWidth + highlightPadding;
        float highlightHeight = cardHeight + highlightPadding;

        float tableauSpacing = highlightWidth + 35f;
        float tableauZoneHeight = highlightHeight;
        float tableauZoneWidth = highlightWidth;

        float totalTableauWidth = (8 * tableauSpacing) - (tableauSpacing - cardWidth);
        float tableauStartX = -totalTableauWidth / 2f + cardWidth / 2f;

        float topRowY = 300f;
        float foundationStartX = 200f;
        float foundationSpacing = 200f;
        float freeCellStartX = -200f;
        float freeCellSpacing = 200f;
        float tableauY = -70f;

        for (int i = 0; i < 8; i++)
        {
            GameObject colObj = new GameObject("TableauColumn_" + i);
            colObj.transform.SetParent(tableauParent, false);

            RectTransform rect = colObj.AddComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.sizeDelta = new Vector2(tableauZoneWidth, tableauZoneHeight);
            rect.anchoredPosition = new Vector2(tableauStartX + (i * tableauSpacing), tableauY);

            UnityEngine.UI.Image bgImage = colObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = activeTheme.tableauColor;

            TableauColumn col = colObj.AddComponent<TableauColumn>();
            DropZone dropZone = colObj.AddComponent<DropZone>();
            dropZone.zoneType = DropZone.ZoneType.Tableau;
            tableauColumns.Add(col);
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject foundObj = new GameObject("Foundation_" + i);
            foundObj.transform.SetParent(foundationParent, false);

            RectTransform rect = foundObj.AddComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.sizeDelta = new Vector2(highlightWidth, highlightHeight);
            float xPos = foundationStartX + (i * foundationSpacing);
            rect.anchoredPosition = new Vector2(xPos, topRowY);

            UnityEngine.UI.Image bgImage = foundObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = activeTheme.foundationColor;

            Foundation found = foundObj.AddComponent<Foundation>();
            DropZone dropZone = foundObj.AddComponent<DropZone>();
            dropZone.zoneType = DropZone.ZoneType.Foundation;
            foundations.Add(found);
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject cellObj = new GameObject("FreeCell_" + i);
            cellObj.transform.SetParent(freeCellParent, false);

            RectTransform rect = cellObj.AddComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.sizeDelta = new Vector2(highlightWidth, highlightHeight);
            float xPos = freeCellStartX - (i * freeCellSpacing);
            rect.anchoredPosition = new Vector2(xPos, topRowY);

            UnityEngine.UI.Image bgImage = cellObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = activeTheme.freeCellColor;

            FreeCell cell = cellObj.AddComponent<FreeCell>();
            DropZone dropZone = cellObj.AddComponent<DropZone>();
            dropZone.zoneType = DropZone.ZoneType.FreeCell;
            freeCells.Add(cell);
        }
        
        DealCards();
    }

    void EnsureParentAnchors()
    {
        ConfigureRectTransform(tableauParent as RectTransform);
        ConfigureRectTransform(foundationParent as RectTransform);
        ConfigureRectTransform(freeCellParent as RectTransform);
    }

    void ConfigureRectTransform(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.localPosition = Vector3.zero;
    }

    void DealCards()
    {
        if (deck == null)
        {
            return;
        }
        deck.CreateDeck();

        if (deck.cards.Count == 0)
        {
            return;
        }
        deck.Shuffle();
        int cardIndex = 0;
        for (int col = 0; col < 8; col++)
        {
            int cardsInColumn = (col < 4) ? 7 : 6;
            
            for (int row = 0; row < cardsInColumn; row++)
            {
                if (cardIndex >= deck.cards.Count)
                {
                    return;
                }
                
                Card card = deck.cards[cardIndex];
                tableauColumns[col].AddCard(card);
                cardIndex++;
            }
        }

        if (recordInitialStateOnDeal)
        {
            RecordInitialState();
            recordInitialStateOnDeal = false;
        }
    }

    private void RecordInitialState()
    {
        initialState.tableauCards.Clear();
        foreach (TableauColumn column in tableauColumns)
        {
            initialState.tableauCards.Add(new List<Card>(column.cards));
        }

        initialState.freeCellCards.Clear();
        foreach (FreeCell cell in freeCells)
        {
            initialState.freeCellCards.Add(cell.card);
        }

        initialState.foundationCards.Clear();
        foreach (Foundation foundation in foundations)
        {
            initialState.foundationCards.Add(new List<Card>(foundation.cards));
        }

        hasInitialState = true;
    }
    
    public void MoveCard(Card card, ICardContainer from, ICardContainer to)
    {
        from.RemoveCard(card);
        to.AddCard(card);

        RegisterMove(new List<Card> { card }, from, to);
    }
    
    public void IncrementMoveCount()
    {
        moveCount++;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoveCount(moveCount);
        }
    }
    
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

            if (winCelebration != null)
            {
                winCelebration.Play();
            }
        }
    }
    
    public void NewGame()
    {
        ClearInitialStateData();

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
        undoStack.Clear();
        redoStack.Clear();
        recordInitialStateOnDeal = true;
        DealCards();
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoveCount(moveCount);
            UIManager.Instance.HideWinPanel();
        }

        if (winCelebration != null)
        {
            winCelebration.Stop();
        }
    }

    public void RestartRound()
    {
        if (!hasInitialState)
        {
            NewGame();
            return;
        }

        undoStack.Clear();
        redoStack.Clear();
        moveCount = 0;

        Transform cardPool = deck != null ? deck.transform : tableauParent;

        foreach (TableauColumn column in tableauColumns)
        {
            List<Card> cardsCopy = new List<Card>(column.cards);
            foreach (Card card in cardsCopy)
            {
                column.RemoveCard(card);
                if (cardPool != null)
                {
                    card.transform.SetParent(cardPool, false);
                    RectTransform rect = card.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        foreach (Foundation foundation in foundations)
        {
            List<Card> cardsCopy = new List<Card>(foundation.cards);
            foreach (Card card in cardsCopy)
            {
                foundation.RemoveCard(card);
                if (cardPool != null)
                {
                    card.transform.SetParent(cardPool, false);
                    RectTransform rect = card.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        foreach (FreeCell cell in freeCells)
        {
            if (cell.card != null)
            {
                Card card = cell.card;
                cell.RemoveCard(card);
                if (cardPool != null)
                {
                    card.transform.SetParent(cardPool, false);
                    RectTransform rect = card.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        for (int i = 0; i < tableauColumns.Count; i++)
        {
            if (i >= initialState.tableauCards.Count) break;
            TableauColumn column = tableauColumns[i];
            foreach (Card card in initialState.tableauCards[i])
            {
                if (card != null)
                {
                    column.AddCard(card);
                }
            }
        }

        for (int i = 0; i < freeCells.Count; i++)
        {
            if (i >= initialState.freeCellCards.Count) break;
            Card card = initialState.freeCellCards[i];
            if (card != null)
            {
                freeCells[i].AddCard(card);
            }
        }

        for (int i = 0; i < foundations.Count; i++)
        {
            if (i >= initialState.foundationCards.Count) break;
            Foundation foundation = foundations[i];
            foreach (Card card in initialState.foundationCards[i])
            {
                if (card != null)
                {
                    foundation.AddCard(card);
                }
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateMoveCount(moveCount);
            UIManager.Instance.HideWinPanel();
        }
    }

    private void ClearInitialStateData()
    {
        initialState.tableauCards.Clear();
        initialState.freeCellCards.Clear();
        initialState.foundationCards.Clear();
        hasInitialState = false;
    }
}
