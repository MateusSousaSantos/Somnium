using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Orchestrates the inventory UI: a "self" pane (always the player's own Inventory)
// and an "other" pane (bound only while looting a container). Exposed as a singleton
// (same idiom as PlayerReference) so world objects like LootContainer can reach it
// without a cross-scene Inspector reference.
[RequireComponent(typeof(Inventory))]
public class InventoryUIController : MonoBehaviour
{
    public static InventoryUIController Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    public GameObject inventoryPanel;

    // Both panes live under this as children of a Horizontal Layout Group +
    // Content Size Fitter, which centers/spaces however many panes are active (one
    // while browsing your own inventory, two while looting) - no hand-picked pixel
    // positions to maintain as pane sizes/counts change.
    public RectTransform panesContainer;
    public InventoryPanelView selfPane;
    public InventoryPanelView otherPane;
    public GameObject otherPaneRoot;

    // Top-level layer InventoryItemView reparents into while being dragged, so it always
    // renders above both panes regardless of their relative sibling order.
    public RectTransform dragLayer;
    public RectTransform DragLayer => dragLayer;

    // Docked item view, shown as a floating overlay on left-click - parented outside any
    // layout group (see DetailOverlay in the prefab) so opening it never reflows
    // LeftRegion/RightRegion the way it did while nested in ContextWindow's layout group.
    public ItemDetailPanel detailPanel;

    // Basic weapon equip slot (paper doll): a single, non-grid slot living in LeftRegion.
    // InventoryItemView.OnEndDrag checks this as a drop target explicitly (it isn't one of
    // GetActivePanes()'s grid panes, since it has no InventoryGrid of its own).
    public EquipmentSlotView equipSlotView;

    private Inventory selfInventory;
    private LootContainer currentLootContainer;
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;
    private InventoryItemView selectedView;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        selfInventory = GetComponent<Inventory>();

        inventoryPanel.SetActive(false);
        if (otherPaneRoot != null) otherPaneRoot.SetActive(false);
        detailPanel.gameObject.SetActive(false);
        if (equipSlotView != null) equipSlotView.Initialize(this);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnToggleInventory(InputValue value)
    {
        if (!value.isPressed) return;
        if (IsOpen) Close(); else Open();
    }

    // Self-only entry point (the "I" key).
    public void Open()
    {
        bool wasOpen = IsOpen;
        IsOpen = true;
        inventoryPanel.SetActive(true);
        if (!wasOpen) PauseAndUnlockCursor();

        selfPane.Bind(selfInventory, this, "Your Inventory");
        RebuildPanesLayout();
        CloseItemView();
    }

    // Dual-pane entry point, called by a LootContainer in range.
    public void OpenLoot(Inventory otherInventory, string label, LootContainer source)
    {
        bool wasOpen = IsOpen;
        IsOpen = true;
        currentLootContainer = source;
        inventoryPanel.SetActive(true);
        if (otherPaneRoot != null) otherPaneRoot.SetActive(true);
        if (!wasOpen) PauseAndUnlockCursor();

        selfPane.Bind(selfInventory, this, "Your Inventory");
        otherPane.Bind(otherInventory, this, label);
        RebuildPanesLayout();
        CloseItemView();
    }

    public void Close()
    {
        IsOpen = false;
        currentLootContainer = null;
        inventoryPanel.SetActive(false);
        if (otherPaneRoot != null) otherPaneRoot.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;
        CloseItemView();
    }

    public bool IsLooting(LootContainer container)
    {
        return IsOpen && currentLootContainer == container;
    }

    // Exposed so PlayerInteractor (which now owns all interaction-range checks) can close
    // the loot panel if the player wanders out of range of whichever container is open -
    // this class no longer has any range/collider knowledge of its own to do that itself.
    public LootContainer CurrentLootContainer => currentLootContainer;

    // Left-click entry point (InventoryItemView.OnPointerClick). First click on an item just
    // selects/highlights it; the docked detail view only opens on a second click, once that
    // item is already the selection - clicking a different item re-targets the selection
    // (closing any open detail view) without opening its own detail view yet. Clicking the
    // selected item again while its detail view is open toggles it closed.
    public void ShowItemView(InventoryItemView view)
    {
        if (view == selectedView)
        {
            if (detailPanel.gameObject.activeSelf)
            {
                detailPanel.gameObject.SetActive(false);
            }
            else
            {
                // Always the default docked view for now. Once per-item custom views exist,
                // branch here on view.PlacedItem.itemData.customItemViewPrefab instead.
                detailPanel.ShowItem(view.PlacedItem);
                detailPanel.PositionAbove(view.RectTransform);
                detailPanel.gameObject.SetActive(true);
            }
            return;
        }

        if (selectedView != null) selectedView.SetSelected(false);
        selectedView = view;
        view.SetSelected(true);
        detailPanel.gameObject.SetActive(false);
    }

    // Closes the docked item view - called on background click (InventoryPanelBackground),
    // on Close(), and whenever item views get rebuilt (the selected view would dangle).
    public void CloseItemView()
    {
        if (selectedView != null) selectedView.SetSelected(false);
        selectedView = null;
        detailPanel.gameObject.SetActive(false);
    }

    // Every pane currently visible - used by InventoryItemView to figure out which
    // pane a drag ended over. Self is always present; other only while looting.
    public IEnumerable<InventoryPanelView> GetActivePanes()
    {
        yield return selfPane;
        if (IsOpen && currentLootContainer != null) yield return otherPane;
    }

    // Called after a cross-grid transfer, which destroys and recreates InventoryItemViews
    // on both sides - any held selectedView reference would otherwise dangle.
    public void RefreshBothPanes()
    {
        selfPane.RefreshItemViews();
        if (IsOpen && currentLootContainer != null) otherPane.RefreshItemViews();
        RebuildPanesLayout();
        CloseItemView();
    }

    // No-op if panesContainer isn't assigned - e.g. if the panes are laid out by hand
    // instead of a Layout Group, there's nothing here to rebuild.
    private void RebuildPanesLayout()
    {
        if (panesContainer != null) LayoutRebuilder.ForceRebuildLayoutImmediate(panesContainer);
    }

    private void PauseAndUnlockCursor()
    {
        Time.timeScale = 0f;
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
