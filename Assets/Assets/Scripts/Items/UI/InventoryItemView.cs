using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventoryItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, ICanvasRaycastFilter
{
    public Image iconImage;
    public RectTransform highlightContainer;

    private const float HighlightBorderThickness = 3f;

    public PlacedItem PlacedItem => placedItem;
    public RectTransform RectTransform => rectTransform;

    private InventoryGrid grid;
    private InventoryPanelView sourcePane;
    private PlacedItem placedItem;
    private int cellSize;
    private InventoryUIController owner;
    private readonly List<GameObject> highlightTiles = new List<GameObject>();

    private RectTransform rectTransform;
    private Vector2 dragStartAnchoredPos;
    private int dragStartRotation;
    private bool isDragging;

    public void Initialize(InventoryPanelView pane, PlacedItem item, InventoryUIController owningController)
    {
        sourcePane = pane;
        grid = pane.Grid;
        placedItem = item;
        cellSize = pane.CellSize;
        owner = owningController;
        rectTransform = GetComponent<RectTransform>();

        if (iconImage != null) iconImage.sprite = placedItem.itemData.itemIcon;
        UpdateVisualPosition();
        UpdateVisualRotation();
    }

    public void SetSelected(bool selected)
    {
        if (selected) RebuildSelectionHighlight();
        else ClearSelectionHighlight();
    }

    // Traces only the OUTER boundary of the item's silhouette (base/unrotated shape - see
    // UpdateVisualSize for why base coordinates are correct here: highlightContainer is a
    // child of this rotating rectTransform, so it inherits the rotation for free) - a thin
    // bar per exposed cell edge, skipping edges shared between two occupied cells, so L/T
    // shapes get an outline instead of a filled grid of squares.
    private void RebuildSelectionHighlight()
    {
        ClearSelectionHighlight();
        if (highlightContainer == null || placedItem == null) return;

        List<Vector2Int> shape = placedItem.itemData.shape;
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(shape);
        float half = HighlightBorderThickness / 2f;

        foreach (Vector2Int cell in shape)
        {
            float left = cell.x * cellSize;
            float right = (cell.x + 1) * cellSize;
            float top = -cell.y * cellSize;
            float bottom = -(cell.y + 1) * cellSize;

            if (!occupied.Contains(new Vector2Int(cell.x, cell.y - 1)))
                CreateHighlightBar(new Vector2(left, top + half), new Vector2(cellSize, HighlightBorderThickness));

            if (!occupied.Contains(new Vector2Int(cell.x, cell.y + 1)))
                CreateHighlightBar(new Vector2(left, bottom + half), new Vector2(cellSize, HighlightBorderThickness));

            if (!occupied.Contains(new Vector2Int(cell.x - 1, cell.y)))
                CreateHighlightBar(new Vector2(left - half, top), new Vector2(HighlightBorderThickness, cellSize));

            if (!occupied.Contains(new Vector2Int(cell.x + 1, cell.y)))
                CreateHighlightBar(new Vector2(right - half, top), new Vector2(HighlightBorderThickness, cellSize));
        }
    }

    private void CreateHighlightBar(Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject bar = new GameObject("HighlightBorder", typeof(RectTransform), typeof(Image));
        RectTransform barRt = (RectTransform)bar.transform;
        barRt.SetParent(highlightContainer, false);
        barRt.anchorMin = barRt.anchorMax = new Vector2(0, 1);
        barRt.pivot = new Vector2(0, 1);
        barRt.anchoredPosition = anchoredPosition;
        barRt.sizeDelta = sizeDelta;

        Image img = bar.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.50980395f);
        img.raycastTarget = false;

        highlightTiles.Add(bar);
    }

    private void ClearSelectionHighlight()
    {
        foreach (GameObject tile in highlightTiles) Destroy(tile);
        highlightTiles.Clear();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        owner?.ShowItemView(this);
    }

    // Restricts click/drag hit-testing to the item's actual occupied cells rather than its
    // rectangular bounding box, so L/T-shaped items (e.g. Knife) can't be grabbed/selected
    // through a cell they don't occupy - and a raycast there correctly falls through to
    // whatever (if anything) actually occupies that cell instead.
    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (placedItem == null) return true;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out Vector2 localPoint))
            return false;

        Vector2Int baseFp = GetFootprintCellSize(placedItem.itemData.shape);
        int cellX = Mathf.FloorToInt((localPoint.x + baseFp.x * cellSize / 2f) / cellSize);
        int cellY = Mathf.FloorToInt((baseFp.y * cellSize / 2f - localPoint.y) / cellSize);

        foreach (Vector2Int cell in placedItem.itemData.shape)
        {
            if (cell.x == cellX && cell.y == cellY) return true;
        }
        return false;
    }

    void Update()
    {
        if (!isDragging) return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            grid.RotateHeldItem(placedItem);
            UpdateVisualRotation();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartAnchoredPos = rectTransform.anchoredPosition;
        dragStartRotation = placedItem.rotationSteps;

        // Reparent to a top-level drag layer while dragging. Canvas render order follows
        // sibling index, and this view otherwise stays parented under its source pane for
        // the whole drag - so dragging it over a pane with a HIGHER sibling index (e.g.
        // from the container's pane onto the player's own pane) would render it behind
        // that pane's cell backgrounds/items instead of on top, regardless of where the
        // pointer actually is. worldPositionStays=true keeps it visually in place.
        if (owner != null && owner.DragLayer != null)
        {
            rectTransform.SetParent(owner.DragLayer, true);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / GetCanvasScale();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // Equip slot is a special-cased drop target: it's not one of owner.GetActivePanes()
        // (no InventoryGrid/shape fitting - see EquipmentSlotView), so it's checked here first.
        if (owner.equipSlotView != null && owner.equipSlotView.ContainsScreenPoint(eventData.position, eventData.pressEventCamera))
        {
            if (TryDropOnEquipSlot()) return;
            RevertDrag();
            return;
        }

        InventoryPanelView targetPane = FindPaneUnderPointer(eventData);
        if (targetPane == null)
        {
            RevertDrag();
            return;
        }

        // Project this item's world-space center (the rect's pivot, which UpdateVisualPosition
        // treats as the box center) into the target pane's grid-local space to find the target
        // cell. Same computation whether the target is the source pane or the other one -
        // this view is parented under the shared DragLayer for the whole drag (see
        // OnBeginDrag), not under either pane's GridContainer, so anchoredPosition alone
        // can't be used to find a target cell in ANY pane anymore, source included.
        Camera eventCamera = eventData.pressEventCamera;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(eventCamera, rectTransform.position);
        if (!targetPane.TryGetGridLocalPoint(screenPoint, eventCamera, out Vector2 localPoint))
        {
            RevertDrag();
            return;
        }

        Vector2Int fp = GetRotatedFootprintCellSize();
        int destCellSize = targetPane.CellSize;
        int targetX = Mathf.RoundToInt((localPoint.x - fp.x * destCellSize / 2f) / destCellSize);
        int targetY = Mathf.RoundToInt((-localPoint.y - fp.y * destCellSize / 2f) / destCellSize);

        if (targetPane == sourcePane)
        {
            if (grid.MoveItem(placedItem, targetX, targetY, placedItem.rotationSteps))
            {
                rectTransform.SetParent(sourcePane.GridContainer, false);
                UpdateVisualPosition();
            }
            else
            {
                RevertDrag();
            }
            return;
        }

        if (!targetPane.Grid.CanPlaceItem(placedItem.itemData, placedItem.rotationSteps, targetX, targetY))
        {
            RevertDrag();
            return;
        }

        grid.RemoveItem(placedItem);
        targetPane.Grid.PlaceItem(placedItem.itemData, placedItem.rotationSteps, targetX, targetY, placedItem.quantity);

        // This view is currently parented under the drag layer (not either pane's
        // GridContainer), so RefreshBothPanes' own "destroy existing item views" pass
        // wouldn't find it - destroy it explicitly instead of relying on that.
        Destroy(gameObject);
        owner.RefreshBothPanes();
    }

    // Equips this item (if the slot accepts its ItemType and is empty), removing it from its
    // source grid. Shape/rotation don't matter here - the slot just wants the ItemData.
    private bool TryDropOnEquipSlot()
    {
        if (!owner.equipSlotView.TryAccept(placedItem.itemData, placedItem.quantity)) return false;

        grid.RemoveItem(placedItem);
        Destroy(gameObject);
        owner.RefreshBothPanes();
        return true;
    }

    // Which currently-visible pane (if any) the dragged item's center is over. Tested
    // against each pane's actual cell grid (ContainsScreenPoint), not its GridContainer's
    // own RectTransform rect - a pane's rect can be a larger, hand-authored art size
    // (autoSizeToGrid = false) that shouldn't affect where drops are allowed to land.
    private InventoryPanelView FindPaneUnderPointer(PointerEventData eventData)
    {
        foreach (InventoryPanelView pane in owner.GetActivePanes())
        {
            if (pane.ContainsScreenPoint(eventData.position, eventData.pressEventCamera))
                return pane;
        }
        return null;
    }

    // Restore the original grid position/rotation, reparent back to the source pane
    // (undoing the drag-layer reparent from OnBeginDrag), and snap visuals back.
    private void RevertDrag()
    {
        rectTransform.SetParent(sourcePane.GridContainer, false);
        grid.MoveItem(placedItem, placedItem.gridX, placedItem.gridY, dragStartRotation);
        rectTransform.anchoredPosition = dragStartAnchoredPos;
        UpdateVisualRotation();
    }

    // Bounding-box size (in cells) of a shape, given as a list of occupied cell offsets.
    private Vector2Int GetFootprintCellSize(List<Vector2Int> shape)
    {
        return ItemShapeUtility.GetFootprintSize(shape);
    }

    private Vector2Int GetRotatedFootprintCellSize()
    {
        return GetFootprintCellSize(ItemShapeUtility.GetRotatedShape(placedItem.itemData.shape, placedItem.rotationSteps));
    }

    private void UpdateVisualPosition()
    {
        // anchoredPosition is the box's CENTER (the prefab uses a center pivot so rotating
        // in place while dragging looks natural), so offset by half the rotated footprint
        // to land the box over the correct occupied cells.
        Vector2Int fp = GetRotatedFootprintCellSize();
        float centerX = placedItem.gridX * cellSize + fp.x * cellSize / 2f;
        float centerYDown = placedItem.gridY * cellSize + fp.y * cellSize / 2f;
        rectTransform.anchoredPosition = new Vector2(centerX, -centerYDown);
    }

    private void UpdateVisualRotation()
    {
        rectTransform.localEulerAngles = new Vector3(0, 0, 90f * placedItem.rotationSteps);
        UpdateVisualSize();
    }

    private void UpdateVisualSize()
    {
        // Base (rotationSteps=0) shape's own bounding box - NOT the rotated one. The transform
        // rotation above already swaps width/height on screen; sizing from the rotated shape
        // here would double-apply that swap and non-uniformly stretch icon art (which is
        // authored to the item's base orientation).
        Vector2Int baseFp = GetFootprintCellSize(placedItem.itemData.shape);
        rectTransform.sizeDelta = new Vector2(baseFp.x * cellSize, baseFp.y * cellSize);
    }

    private float GetCanvasScale()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        return canvas != null ? canvas.scaleFactor : 1f;
    }
}
