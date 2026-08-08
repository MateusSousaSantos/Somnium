using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// UI for a single, non-grid EquipmentSlot. Unlike the tetris grid panes (InventoryPanelView +
// InventoryItemView), there's no per-cell background or shape/rotation rendering here - the
// equipped item's icon just fills this one fixed-size slot. Acts as a drop target for items
// dragged out of a grid pane (see InventoryItemView.OnEndDrag) and lets the equipped item be
// dragged back out into a grid pane to unequip.
public class EquipmentSlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public EquipmentSlot slot;
    public Image iconImage;
    public TextMeshProUGUI equipamentName;

    private InventoryUIController owner;
    private RectTransform rectTransform;
    private RectTransform iconRectTransform;
    private bool isDragging;

    // Throwaway copy of iconImage shown in the drag layer while dragging - the slot's own
    // icon is never reparented, just hidden, so a cancelled drag needs nothing more than
    // Refresh() to restore it.
    private GameObject dragIcon;
    private RectTransform dragIconRectTransform;

    // Rotation applied while dragging this item out of the slot (R key, mirroring
    // InventoryItemView's held-item rotation). The equipped item itself never carries a
    // rotation (EquipmentSlot.Equip always stores 0 - shape doesn't matter while equipped),
    // so this only comes into play for the pane it lands on.
    private int dragRotationSteps;

    public void Initialize(InventoryUIController owningController)
    {
        owner = owningController;
        rectTransform = GetComponent<RectTransform>();
        if (iconImage != null) iconRectTransform = iconImage.rectTransform;
        Refresh();
    }

    public void Refresh()
    {
        bool hasItem = slot.EquippedItem != null;
        if (iconImage == null) return;

        iconImage.enabled = hasItem;
        iconImage.sprite = hasItem ? slot.EquippedItem.itemData.itemIcon : null;
        iconRectTransform.anchoredPosition = Vector2.zero;
        if (equipamentName != null) equipamentName.text = hasItem ? slot.EquippedItem.itemData.itemName : "";
    }

    public bool CanAccept(ItemData itemData) => slot.CanAccept(itemData);

    // Called by InventoryItemView.OnEndDrag when a grid item is dropped on this slot.
    public bool TryAccept(ItemData itemData, int quantity)
    {
        if (slot.Equip(itemData, quantity) == null) return false;
        Refresh();
        return true;
    }

    // Whether a screen point (e.g. a dragged grid item's drop point) lands on this slot.
    public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint, eventCamera);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (slot.EquippedItem == null) return;
        isDragging = true;
        dragRotationSteps = 0;

        // Spawn a throwaway copy of the icon into the shared drag layer, so it renders
        // above both grid panes regardless of EquipSlot's own sibling order, and hide the
        // slot's own icon rather than reparenting it - the copy carries the current sprite
        // along automatically since it's cloned before iconImage.enabled is cleared below.
        if (owner.DragLayer != null)
        {
            dragIcon = Instantiate(iconImage.gameObject, owner.DragLayer);
            dragIconRectTransform = dragIcon.GetComponent<RectTransform>();
            dragIconRectTransform.position = iconRectTransform.position;
            dragIconRectTransform.sizeDelta = iconRectTransform.sizeDelta;
            dragIcon.GetComponent<Image>().raycastTarget = false;
        }

        iconImage.enabled = false;
        if (equipamentName != null) equipamentName.text = "";
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || dragIconRectTransform == null) return;
        dragIconRectTransform.anchoredPosition += eventData.delta / GetCanvasScale();
    }

    // Mirrors InventoryItemView's Update: R rotates the item while it's being held, here
    // just the throwaway drag icon since the slot itself has no shape/rotation concept.
    void Update()
    {
        if (!isDragging) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            dragRotationSteps = (dragRotationSteps + 1) % 4;
            if (dragIconRectTransform != null)
                dragIconRectTransform.localEulerAngles = new Vector3(0, 0, 90f * dragRotationSteps);
        }
    }

    // Dropping the equipped item onto a grid pane unequips it and places it there at
    // rotation 0 (same starting rotation a freshly-added item gets); dropping anywhere else
    // just snaps the icon back into the slot, still equipped.
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        InventoryPanelView targetPane = FindPaneUnderPointer(eventData);
        if (targetPane != null) TryDropOnPane(targetPane, eventData);

        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
            dragIconRectTransform = null;
        }

        // Restores the slot's own icon/name if the item is still equipped (cancelled or
        // rejected drop), or clears both if TryDropOnPane just unequipped it.
        Refresh();
    }

    private void TryDropOnPane(InventoryPanelView targetPane, PointerEventData eventData)
    {
        if (!targetPane.TryGetGridLocalPoint(eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        ItemData itemData = slot.EquippedItem.itemData;
        int cellSize = targetPane.CellSize;
        Vector2Int fp = ItemShapeUtility.GetFootprintSize(ItemShapeUtility.GetRotatedShape(itemData.shape, dragRotationSteps));
        int targetX = Mathf.RoundToInt((localPoint.x - fp.x * cellSize / 2f) / cellSize);
        int targetY = Mathf.RoundToInt((-localPoint.y - fp.y * cellSize / 2f) / cellSize);

        if (!targetPane.Grid.CanPlaceItem(itemData, dragRotationSteps, targetX, targetY)) return;

        int quantity = slot.EquippedItem.quantity;
        slot.Unequip();
        targetPane.Grid.PlaceItem(itemData, dragRotationSteps, targetX, targetY, quantity);
        owner.RefreshBothPanes();
    }

    private InventoryPanelView FindPaneUnderPointer(PointerEventData eventData)
    {
        foreach (InventoryPanelView pane in owner.GetActivePanes())
        {
            if (pane.ContainsScreenPoint(eventData.position, eventData.pressEventCamera))
                return pane;
        }
        return null;
    }

    private float GetCanvasScale()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.scaleFactor : 1f;
    }
}
