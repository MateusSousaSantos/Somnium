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

    // Pool of released-but-not-destroyed InventoryItemViews, so refreshing (including every
    // drag-drop transfer) reuses instances instead of Destroy+Instantiate churn. Always parented
    // under gridContainer and inactive while pooled - see AcquireView/ReleaseView.
    private readonly List<InventoryItemView> viewPool = new();

    // Set true for the duration of a drag sourced from this pane (InventoryItemView.OnBeginDrag/
    // OnEndDrag) - the dragged view leaves GridContainer for owner.DragLayer while held, so an
    // in-between Changed event (e.g. rotating the held item with R, which still writes through
    // InventoryGrid) would otherwise refresh without finding that view in GridContainer and
    // spawn a second, "ghost" view for the same item. The drop itself always mutates the grid
    // again on every exit path, so un-suspending doesn't need to force a refresh here - the
    // drop's own Changed event covers it.
    private bool refreshSuspended;

    public void SetRefreshSuspended(bool suspended)
    {
        refreshSuspended = suspended;
    }

    // Binds this pane to an Inventory, rebuilding the cell background at that
    // inventory's dimensions (container grids vary in size, so this can't be built once).
    public void Bind(Inventory boundInventory, InventoryUIController owningController, string title)
    {
        if (inventory != null) inventory.Changed -= RefreshItemViews;

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

        // Subscribed after the initial RefreshItemViews() above so binding doesn't also fire a
        // redundant refresh - subsequent grid mutations (place/move/remove) drive it from here on.
        inventory.Changed += RefreshItemViews;
    }

    void OnDestroy()
    {
        if (inventory != null) inventory.Changed -= RefreshItemViews;
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
        if (refreshSuspended) return;

        // Collected first rather than released while enumerating gridContainer's children -
        // ReleaseView reparents (a no-op here, but not always - see below), which would
        // otherwise mutate the Transform child list out from under this same foreach.
        List<InventoryItemView> activeViews = new List<InventoryItemView>();
        foreach (Transform child in gridContainer)
        {
            if (child.TryGetComponent<InventoryItemView>(out InventoryItemView view) && view.gameObject.activeSelf)
                activeViews.Add(view);
        }
        foreach (InventoryItemView view in activeViews) ReleaseView(view);

        foreach (PlacedItem placedItem in Grid.GetPlacedItems())
        {
            InventoryItemView view = AcquireView();
            view.Initialize(this, placedItem, owner);
        }
    }

    // Reuses a pooled instance if one's free, otherwise instantiates a new one. Instances only
    // ever grow the pool's high-water mark - they're hidden/shown, never destroyed, once made.
    public InventoryItemView AcquireView()
    {
        InventoryItemView view;
        if (viewPool.Count > 0)
        {
            view = viewPool[viewPool.Count - 1];
            viewPool.RemoveAt(viewPool.Count - 1);
            view.gameObject.SetActive(true);
        }
        else
        {
            GameObject go = Instantiate(itemViewPrefab, gridContainer);
            view = go.GetComponent<InventoryItemView>();
        }

        // RebuildGridBackground() destroys and recreates the cell backgrounds as fresh children
        // appended to the end of gridContainer on every Bind() - without this, a REUSED item
        // view (kept at whatever sibling index it had from an earlier refresh) would end up
        // sitting BEHIND those newly-rebuilt backgrounds and look like it had disappeared, even
        // though it's still there with correct data. Freshly instantiated views are already
        // last, so this is a no-op for those.
        view.transform.SetAsLastSibling();
        return view;
    }

    // Returns a view to this pane's pool instead of destroying it - called both from
    // RefreshItemViews' own release pass and directly by InventoryItemView when a drag ends by
    // moving the item to a different pane (its view is reparented into owner.DragLayer for the
    // duration of the drag, so this also brings it back home under gridContainer).
    public void ReleaseView(InventoryItemView view)
    {
        view.SetSelected(false);
        view.gameObject.SetActive(false);
        view.transform.SetParent(gridContainer, false);
        viewPool.Add(view);
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
