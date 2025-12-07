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
    
    private List<Card> draggedCards = new List<Card>();
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

        draggedCards = GetDraggableSequence();
        
        if (draggedCards == null || draggedCards.Count == 0)
        {
            eventData.pointerDrag = null;
            return;
        }
        
        originalPosition = transform.position;
        originalParent = transform.parent;
        originalContainer = originalParent.GetComponent<ICardContainer>();
        

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
        

        Vector3 delta = (Vector3)eventData.position - originalPosition;
        
        for (int i = 0; i < draggedCards.Count; i++)
        {
            draggedCards[i].transform.position = originalPositions[i] + delta;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {

        foreach (CanvasGroup cg in cardCanvasGroups)
        {
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
        }


        if (potentialDropZone != null && ValidateMove(potentialDropZone))
        {

            ICardContainer targetContainer = potentialDropZone.GetComponent<ICardContainer>();

            if (targetContainer != null && originalContainer != null)
            {

                foreach (Card draggedCard in draggedCards)
                {
                    originalContainer.RemoveCard(draggedCard);
                    targetContainer.AddCard(draggedCard);
                }

                GameManager.Instance.RegisterMove(new List<Card>(draggedCards), originalContainer, targetContainer);
            }
            else
            {
                ReturnCardsToOriginal();
            }
        }
        else
        {

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
        if (originalContainer == null) return;


        foreach (Card draggedCard in draggedCards)
        {
            draggedCard.transform.SetParent(originalParent);
        }


        if (originalContainer is TableauColumn tableau)
        {
            tableau.UpdateCardPositions();
        }
        else if (originalContainer is FreeCell freeCell)
        {
            if (draggedCards.Count > 0)
            {
                RectTransform rect = draggedCards[0].GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                }
            }
        }
        else if (originalContainer is Foundation foundation)
        {
            foreach (Card draggedCard in draggedCards)
            {
                RectTransform rect = draggedCard.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                }
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


        TableauColumn tableau = transform.parent.GetComponent<TableauColumn>();
        if (tableau == null)
        {

            FreeCell freeCell = transform.parent.GetComponent<FreeCell>();
            if (freeCell != null && freeCell.card == card)
            {
                sequence.Add(card);
                return sequence;
            }
            return null;
        }
        

        int startIndex = tableau.cards.IndexOf(card);
        if (startIndex == -1) return null;
        

        for (int i = startIndex; i < tableau.cards.Count; i++)
        {
            sequence.Add(tableau.cards[i]);
        }
        

        if (!IsValidSequence(sequence)) return null;


        int maxMovable = CalculateMaxMovableCards(false);
        if (sequence.Count > maxMovable)
        {
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
            

            if (current.IsRed() == next.IsRed()) return false;
            if (current.GetRankValue() != next.GetRankValue() + 1) return false;
        }
        
        return true;
    }
    
    private int CalculateMaxMovableCards(bool reserveEmptyColumn)
    {
        int emptyFreeCells = 0;
        int emptyColumns = 0;

        foreach (FreeCell cell in GameManager.Instance.freeCells)
        {
            if (cell.card == null)
            {
                emptyFreeCells++;
            }
        }

        foreach (TableauColumn col in GameManager.Instance.tableauColumns)
        {
            if (col.cards.Count == 0)
            {
                emptyColumns++;
            }
        }

        if (reserveEmptyColumn && emptyColumns > 0)
        {
            emptyColumns--;
        }

        return (1 + emptyFreeCells) * (int)Mathf.Pow(2, emptyColumns);
    }

    private bool ValidateMove(DropZone dropZone)
    {
        if (draggedCards.Count == 0) return false;


        Card movingCard = draggedCards[0];

        if (dropZone.zoneType == DropZone.ZoneType.Tableau)
        {
            TableauColumn tableau = dropZone.GetComponent<TableauColumn>();
            if (tableau == null)
            {
                return false;
            }

            bool targetEmpty = tableau.cards.Count == 0;
            int maxMovable = CalculateMaxMovableCards(targetEmpty);
            if (draggedCards.Count > maxMovable)
            {
                return false;
            }

            return tableau.CanPlaceCard(movingCard);
        }
        else if (dropZone.zoneType == DropZone.ZoneType.Foundation)
        {

            if (draggedCards.Count > 1) return false;
            
            Foundation foundation = dropZone.GetComponent<Foundation>();
            return foundation != null && foundation.CanPlaceCard(movingCard);
        }
        else if (dropZone.zoneType == DropZone.ZoneType.FreeCell)
        {

            if (draggedCards.Count > 1) return false;
            
            FreeCell freeCell = dropZone.GetComponent<FreeCell>();
            return freeCell != null && freeCell.CanPlaceCard(movingCard);
        }
        
        return false;
    }
}
