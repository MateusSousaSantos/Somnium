using UnityEngine;
using UnityEngine.EventSystems;

// Sits on the InventoryPanel's background Image (which already covers the whole panel
// and is raycastable). Closes the docked ContextWindow item view on any click that isn't
// on an item - items sit on top and consume the click themselves via their own
// ICanvasRaycastFilter-constrained hit area before it would reach this background.
public class InventoryPanelBackground : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryUIController.Instance?.CloseItemView();
    }
}
