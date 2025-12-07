using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum ZoneType { Tableau, Foundation, FreeCell }
    public ZoneType zoneType;
    
    private Image highlightImage;
    private Color originalColor;
    
    void Start()
    {
        highlightImage = GetComponent<Image>();
        
        if (highlightImage != null)
        {
            originalColor = highlightImage.color;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && highlightImage != null)
        {
            highlightImage.color = new Color(1, 1, 0, 0.5f);

            DragDrop dragDrop = eventData.pointerDrag.GetComponent<DragDrop>();
            if (dragDrop != null)
            {
                dragDrop.SetDropZone(this);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.color = originalColor;
        }

        if (eventData.pointerDrag != null)
        {
            DragDrop dragDrop = eventData.pointerDrag.GetComponent<DragDrop>();
            if (dragDrop != null)
            {
                dragDrop.SetDropZone(null);
            }
        }
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.color = originalColor;
        }
        
        DragDrop draggable = eventData.pointerDrag.GetComponent<DragDrop>();
        if (draggable != null)
        {
            draggable.SetDropZone(this);
        }
    }
}
