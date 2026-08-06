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

    private InventoryGrid grid;
    private PlacedItem placedItem;
    private int cellSize;
    private InventoryUIController owner;
    private readonly List<GameObject> highlightTiles = new List<GameObject>();

    private RectTransform rectTransform;
    private Vector2 dragStartAnchoredPos;
    private int dragStartRotation;
    private bool isDragging;

    public void Initialize(InventoryGrid inventoryGrid, PlacedItem item, int cellPixelSize, InventoryUIController owningController)
    {
        grid = inventoryGrid;
        placedItem = item;
        cellSize = cellPixelSize;
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
        owner?.SelectItem(this);
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
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / GetCanvasScale();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // Snap-target math must undo the same center-pivot offset UpdateVisualPosition applies,
        // using the footprint at the item's rotation as of drag-end.
        Vector2Int fp = GetRotatedFootprintCellSize();
        int targetX = Mathf.RoundToInt((rectTransform.anchoredPosition.x - fp.x * cellSize / 2f) / cellSize);
        int targetY = Mathf.RoundToInt((-rectTransform.anchoredPosition.y - fp.y * cellSize / 2f) / cellSize);

        if (grid.MoveItem(placedItem, targetX, targetY, placedItem.rotationSteps))
        {
            UpdateVisualPosition();
        }
        else
        {
            // Revert: restore the original grid position/rotation and snap visuals back.
            grid.MoveItem(placedItem, placedItem.gridX, placedItem.gridY, dragStartRotation);
            rectTransform.anchoredPosition = dragStartAnchoredPos;
            UpdateVisualRotation();
        }
    }

    // Bounding-box size (in cells) of a shape, given as a list of occupied cell offsets.
    private Vector2Int GetFootprintCellSize(List<Vector2Int> shape)
    {
        int maxX = 0;
        int maxY = 0;
        foreach (Vector2Int cell in shape)
        {
            if (cell.x + 1 > maxX) maxX = cell.x + 1;
            if (cell.y + 1 > maxY) maxY = cell.y + 1;
        }
        return new Vector2Int(maxX, maxY);
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
