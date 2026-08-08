using System.Collections.Generic;
using UnityEngine;
using TMPro;

// One self-contained inventory grid pane: builds the cell background at whatever
// size the bound Inventory has, and spawns/refreshes the draggable InventoryItemViews
// for it. InventoryUIController owns two instances of this (self + other) so the
// dual-pane loot UI is just two panes side by side instead of duplicated grid logic.
public class InventoryPanelView : MonoBehaviour
{
    public RectTransform gridContainer;
    public GameObject cellBackgroundPrefab;
    public GameObject itemViewPrefab;
    public int cellSize = 48;
    public TextMeshProUGUI titleText; // optional

    // On by default so a loot pane (unknown/variable container size) always matches its
    // bound Inventory. Turn off for a pane with a fixed, hand-authored size/art (e.g. the
    // player's own pane) - its Grid Width/Height and Cell Size should then just match
    // whatever the art was built for, and this stops touching its RectTransform size.
    public bool autoSizeToGrid = true;

    public InventoryGrid Grid { get; private set; }
    public RectTransform GridContainer => gridContainer;
    public int CellSize => cellSize;

    private Inventory inventory;
    private InventoryUIController owner;
    private readonly List<GameObject> cellBackgrounds = new();

    // Binds this pane to an Inventory, rebuilding the cell background at that
    // inventory's dimensions (container grids vary in size, so this can't be built once).
    public void Bind(Inventory boundInventory, InventoryUIController owningController, string title)
    {
        inventory = boundInventory;
        owner = owningController;
        Grid = inventory.Grid;

        if (titleText != null) titleText.text = title;

        // Size this pane's own rect to match its bound inventory's grid dimensions, so a
        // parent Horizontal/Vertical Layout Group + Content Size Fitter can lay out and
        // center panes of different container sizes automatically instead of using
        // hand-picked pixel positions. Skipped for fixed-size/hand-authored panes.
        if (autoSizeToGrid)
        {
            gridContainer.sizeDelta = new Vector2(inventory.gridWidth * cellSize, inventory.gridHeight * cellSize);
        }

        RebuildGridBackground();
        RefreshItemViews();
    }

    // Converts a screen point into this pane's GRID-local space: (0,0) at the actual
    // cell grid's own top-left corner, x increasing right, y increasing UP (negative
    // going down) - matching the convention cell/item placement already uses (see
    // RebuildGridBackground's `-y * cellSize`). Deliberately derived from gridContainer's
    // .rect (its xMin/yMax corner) rather than assuming pivot = (0,1): a fixed/hand-
    // authored gridContainer (autoSizeToGrid = false) may have any pivot or size, and
    // .rect already accounts for that correctly, same idiom as Rect.Contains(localPoint).
    public bool TryGetGridLocalPoint(Vector2 screenPoint, Camera eventCamera, out Vector2 gridLocalPoint)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(gridContainer, screenPoint, eventCamera, out Vector2 localPoint))
        {
            gridLocalPoint = default;
            return false;
        }

        Rect rect = gridContainer.rect;
        gridLocalPoint = localPoint - new Vector2(rect.xMin, rect.yMax);
        return true;
    }

    // Whether a screen point (e.g. a drag drop) lands within this pane's actual cell
    // grid - checked against gridWidth/gridHeight * cellSize rather than gridContainer's
    // own RectTransform size, so a fixed/hand-authored art size on gridContainer
    // (autoSizeToGrid = false) never breaks drop detection.
    public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
    {
        if (!TryGetGridLocalPoint(screenPoint, eventCamera, out Vector2 p)) return false;

        float width = Grid.width * cellSize;
        float height = Grid.height * cellSize;
        return p.x >= 0 && p.x <= width && p.y <= 0 && p.y >= -height;
    }

    public void RefreshItemViews()
    {
        foreach (Transform child in gridContainer)
        {
            if (child.TryGetComponent<InventoryItemView>(out _)) Destroy(child.gameObject);
        }

        foreach (PlacedItem placedItem in Grid.GetPlacedItems())
        {
            GameObject go = Instantiate(itemViewPrefab, gridContainer);
            InventoryItemView view = go.GetComponent<InventoryItemView>();
            view.Initialize(this, placedItem, owner);
        }
    }

    private void RebuildGridBackground()
    {
        foreach (GameObject cell in cellBackgrounds)
        {
            if (cell != null) Destroy(cell);
        }
        cellBackgrounds.Clear();

        for (int y = 0; y < inventory.gridHeight; y++)
        {
            for (int x = 0; x < inventory.gridWidth; x++)
            {
                GameObject cell = Instantiate(cellBackgroundPrefab, gridContainer);
                RectTransform rt = cell.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
                cellBackgrounds.Add(cell);
            }
        }
    }
}
