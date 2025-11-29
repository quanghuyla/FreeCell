using UnityEngine;
using UnityEngine.EventSystems;

public class AutoMove : MonoBehaviour, IPointerClickHandler
{
    private Card card;
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f; // 0.3 giây cho double-click
    
    void Awake()
    {
        card = GetComponent<Card>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // Kiểm tra double-click
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            // Double-click detected!
            TryAutoMove();
            lastClickTime = 0f;
        }
        else
        {
            // Click đầu tiên
            lastClickTime = Time.time;
        }
    }
    
    private void TryAutoMove()
    {
        // Kiểm tra xem có phải quân bài trên cùng không
        if (!IsTopCard())
        {
            Debug.Log("Không phải quân bài trên cùng!");
            return;
        }
        
        ICardContainer currentContainer = transform.parent.GetComponent<ICardContainer>();
        if (currentContainer == null)
        {
            Debug.Log("Không tìm thấy container!");
            return;
        }
        
        // Ưu tiên 1: Thử chuyển lên Foundation
        if (TryMoveToFoundation(currentContainer))
        {
            Debug.Log($"✅ Auto-moved {card.GetDisplayString()} to Foundation");
            return;
        }
        
        // Ưu tiên 2: Thử chuyển sang Tableau khác
        if (TryMoveToTableau(currentContainer))
        {
            Debug.Log($"✅ Auto-moved {card.GetDisplayString()} to Tableau");
            return;
        }
        
        // Ưu tiên 3: Thử chuyển vào Free Cell
        if (TryMoveToFreeCell(currentContainer))
        {
            Debug.Log($"✅ Auto-moved {card.GetDisplayString()} to Free Cell");
            return;
        }
        
        // Không có nước đi hợp lệ
        Debug.Log($"❌ Không tìm thấy nước đi hợp lệ cho {card.GetDisplayString()}");
    }
    
    private bool IsTopCard()
    {
        // Kiểm tra trong Tableau
        TableauColumn tableau = transform.parent.GetComponent<TableauColumn>();
        if (tableau != null)
        {
            return tableau.cards.Count > 0 && tableau.cards[tableau.cards.Count - 1] == card;
        }
        
        // Kiểm tra trong Free Cell
        FreeCell freeCell = transform.parent.GetComponent<FreeCell>();
        if (freeCell != null)
        {
            return freeCell.card == card;
        }
        
        // Kiểm tra trong Foundation (optional - cho phép lấy từ Foundation)
        Foundation foundation = transform.parent.GetComponent<Foundation>();
        if (foundation != null)
        {
            return foundation.cards.Count > 0 && foundation.cards[foundation.cards.Count - 1] == card;
        }
        
        return false;
    }
    
    private bool TryMoveToFoundation(ICardContainer from)
    {
        foreach (Foundation foundation in GameManager.Instance.foundations)
        {
            if (foundation.CanPlaceCard(card))
            {
                // Di chuyển
                from.RemoveCard(card);
                foundation.AddCard(card);
                
                // Cập nhật game
                GameManager.Instance.IncrementMoveCount();
                GameManager.Instance.CheckWinCondition();
                
                return true;
            }
        }
        return false;
    }
    
    private bool TryMoveToTableau(ICardContainer from)
    {
        foreach (TableauColumn tableau in GameManager.Instance.tableauColumns)
        {
            // Không chuyển về cột cũ
            if (tableau == from) continue;
            
            if (tableau.CanPlaceCard(card))
            {
                // Di chuyển
                from.RemoveCard(card);
                tableau.AddCard(card);
                
                // Cập nhật game
                GameManager.Instance.IncrementMoveCount();
                
                return true;
            }
        }
        return false;
    }
    
    private bool TryMoveToFreeCell(ICardContainer from)
    {
        // Không chuyển từ FreeCell sang FreeCell khác (vô nghĩa)
        if (from is FreeCell) return false;
        
        foreach (FreeCell freeCell in GameManager.Instance.freeCells)
        {
            if (freeCell.CanPlaceCard(card))
            {
                // Di chuyển
                from.RemoveCard(card);
                freeCell.AddCard(card);
                
                // Cập nhật game
                GameManager.Instance.IncrementMoveCount();
                
                return true;
            }
        }
        return false;
    }
}