using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;
    private Transform originalParent;
    private Card card;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private DropZone potentialDropZone;
    private ICardContainer originalContainer;
    
    private List<Card> draggedCards = new List<Card>(); // Cards being dragged together
    private List<CanvasGroup> cardCanvasGroups = new List<CanvasGroup>();
    private List<Vector3> originalPositions = new List<Vector3>();
    
    void Awake()
    {
        card = GetComponent<Card>();
        canvas = FindFirstObjectByType<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Get the sequence of cards we're trying to drag
        draggedCards = GetDraggableSequence();
        
        if (draggedCards == null || draggedCards.Count == 0)
        {
            eventData.pointerDrag = null; // Cancel the drag
            return;
        }
        
        originalPosition = transform.position;
        originalParent = transform.parent;
        originalContainer = originalParent.GetComponent<ICardContainer>();
        
        // Store original positions and prepare all cards for dragging
        originalPositions.Clear();
        cardCanvasGroups.Clear();
        
        foreach (Card draggedCard in draggedCards)
        {
            originalPositions.Add(draggedCard.transform.position);
            
            CanvasGroup cg = draggedCard.GetComponent<CanvasGroup>();
            if (cg == null) cg = draggedCard.gameObject.AddComponent<CanvasGroup>();
            
            cardCanvasGroups.Add(cg);
            cg.blocksRaycasts = false;
            cg.alpha = 0.8f;
            
            draggedCard.transform.SetParent(canvas.transform);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (draggedCards.Count == 0) return;
        
        // Move all cards together, maintaining their relative positions
        Vector3 delta = (Vector3)eventData.position - originalPosition;
        
        for (int i = 0; i < draggedCards.Count; i++)
        {
            draggedCards[i].transform.position = originalPositions[i] + delta;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore raycast blocking
        foreach (CanvasGroup cg in cardCanvasGroups)
        {
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
        }
        
        if (potentialDropZone != null && ValidateMove(potentialDropZone))
        {
            // Valid move - get the target container
            ICardContainer targetContainer = potentialDropZone.GetComponent<ICardContainer>();
            
            if (targetContainer != null && originalContainer != null)
            {
                // Move all cards in sequence
                foreach (Card draggedCard in draggedCards)
                {
                    originalContainer.RemoveCard(draggedCard);
                    targetContainer.AddCard(draggedCard);
                }
                
                // Update move count (count as one move)
                GameManager.Instance.IncrementMoveCount();
                GameManager.Instance.CheckWinCondition();
            }
            else
            {
                ReturnCardsToOriginal();
            }
        }
        else
        {
            // Invalid move - return to original position
            ReturnCardsToOriginal();
        }
        
        draggedCards.Clear();
        cardCanvasGroups.Clear();
        originalPositions.Clear();
        potentialDropZone = null;
        originalContainer = null;
    }
    
    private void ReturnCardsToOriginal()
    {
        foreach (Card draggedCard in draggedCards)
        {
            draggedCard.transform.SetParent(originalParent);
        }
        
        // Let the original container update positions
        if (originalContainer != null)
        {
            TableauColumn tableau = originalContainer as TableauColumn;
            if (tableau != null)
            {
                // Force position update
                tableau.cards.Clear();
                foreach (Card c in draggedCards)
                {
                    tableau.cards.Add(c);
                }
                // This will trigger UpdateCardPositions via the tableau's logic
            }
        }
    }
    
    public void SetDropZone(DropZone dropZone)
    {
        potentialDropZone = dropZone;
    }
    
    private List<Card> GetDraggableSequence()
    {
        List<Card> sequence = new List<Card>();
        
        // Only works from tableau columns
        TableauColumn tableau = transform.parent.GetComponent<TableauColumn>();
        if (tableau == null)
        {
            // Check free cell (single card only)
            FreeCell freeCell = transform.parent.GetComponent<FreeCell>();
            if (freeCell != null && freeCell.card == card)
            {
                sequence.Add(card);
                return sequence;
            }
            return null;
        }
        
        // Find this card's index in the tableau
        int startIndex = tableau.cards.IndexOf(card);
        if (startIndex == -1) return null;
        
        // Build the sequence from this card to the end
        for (int i = startIndex; i < tableau.cards.Count; i++)
        {
            sequence.Add(tableau.cards[i]);
        }
        
        // Check if this is a valid sequence (alternating colors, descending)
        if (!IsValidSequence(sequence)) return null;
        
        // Check if we have enough free spaces to move this many cards
        int maxMovable = CalculateMaxMovableCards();
        if (sequence.Count > maxMovable)
        {
            Debug.Log($"Cannot move {sequence.Count} cards. Maximum movable: {maxMovable}");
            return null;
        }
        
        return sequence;
    }
    
    private bool IsValidSequence(List<Card> sequence)
    {
        if (sequence.Count == 0) return false;
        if (sequence.Count == 1) return true;
        
        for (int i = 0; i < sequence.Count - 1; i++)
        {
            Card current = sequence[i];
            Card next = sequence[i + 1];
            
            // Must alternate colors and descend by 1
            if (current.IsRed() == next.IsRed()) return false;
            if (current.GetRankValue() != next.GetRankValue() + 1) return false;
        }
        
        return true;
    }
    
    private int CalculateMaxMovableCards()
    {
        int emptyFreeCells = 0;
        int emptyColumns = 0;
        
        // Count empty free cells
        foreach (FreeCell cell in GameManager.Instance.freeCells)
        {
            if (cell.card == null) emptyFreeCells++;
        }
        
        // Count empty tableau columns
        foreach (TableauColumn col in GameManager.Instance.tableauColumns)
        {
            if (col.cards.Count == 0) emptyColumns++;
        }
        
        // Formula: (1 + emptyFreeCells) * 2^emptyColumns
        int maxCards = (1 + emptyFreeCells) * (int)Mathf.Pow(2, emptyColumns);
        return maxCards;
    }
    
    private bool ValidateMove(DropZone dropZone)
    {
        if (draggedCards.Count == 0) return false;
        
        // Only the bottom card of the sequence matters for validation
        Card bottomCard = draggedCards[draggedCards.Count - 1];
        
        if (dropZone.zoneType == DropZone.ZoneType.Tableau)
        {
            TableauColumn tableau = dropZone.GetComponent<TableauColumn>();
            return tableau != null && tableau.CanPlaceCard(bottomCard);
        }
        else if (dropZone.zoneType == DropZone.ZoneType.Foundation)
        {
            // Can only move single cards to foundation
            if (draggedCards.Count > 1) return false;
            
            Foundation foundation = dropZone.GetComponent<Foundation>();
            return foundation != null && foundation.CanPlaceCard(bottomCard);
        }
        else if (dropZone.zoneType == DropZone.ZoneType.FreeCell)
        {
            // Can only move single cards to free cell
            if (draggedCards.Count > 1) return false;
            
            FreeCell freeCell = dropZone.GetComponent<FreeCell>();
            return freeCell != null && freeCell.CanPlaceCard(bottomCard);
        }
        
        return false;
    }
}