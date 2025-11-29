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
        // Get existing Image component instead of adding a new one
        highlightImage = GetComponent<Image>();
        
        if (highlightImage != null)
        {
            originalColor = highlightImage.color;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlight when dragging over
        if (eventData.pointerDrag != null && highlightImage != null)
        {
            highlightImage.color = new Color(1, 1, 0, 0.5f); // Yellow highlight
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.color = originalColor; // Restore original color
        }
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (highlightImage != null)
        {
            highlightImage.color = originalColor; // Restore original color
        }
        
        DragDrop draggable = eventData.pointerDrag.GetComponent<DragDrop>();
        if (draggable != null)
        {
            draggable.SetDropZone(this);
        }
    }
}