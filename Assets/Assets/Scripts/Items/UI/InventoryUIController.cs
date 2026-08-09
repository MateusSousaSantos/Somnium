using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Orchestrates the inventory UI: a "self" pane (always the player's own Inventory)
// and an "other" pane (bound only while looting a container). Exposed as a singleton
// (same idiom as PlayerReference) so world objects like LootContainer can reach it
// without a cross-scene Inspector reference.
//
// Lives on its own object (under Bootstrap, alongside InventoryCanvas/EventSystem) rather than
// on Player - it resolves the player's Inventory/EquipmentSlot via PlayerReference.Instance
// instead of RequireComponent, the same idiom CameraFollow uses to find the player without
// being parented under it. Awake ordering between this object and Player isn't guaranteed once
// they're separate roots, so those lookups happen in Open()/OpenLoot() (always well after both
// exist, since they only fire from user input) rather than being cached in Awake().
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

        inventoryPanel.SetActive(false);
        if (otherPaneRoot != null) otherPaneRoot.SetActive(false);
        detailPanel.gameObject.SetActive(false);
        if (equipSlotView != null) equipSlotView.Initialize(this);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Forwarded from InventoryInputRelay (which stays on Player - PlayerInput's Send Messages
    // notification behavior only reaches components on its own GameObject, not this one).
    public void Toggle()
    {
        if (IsOpen) Close(); else Open();
    }

    // Self-only entry point (the "I" key).
    public void Open()
    {
        bool wasOpen = IsOpen;
        IsOpen = true;
        inventoryPanel.SetActive(true);
        if (!wasOpen) PauseAndUnlockCursor();

        ResolvePlayerBindings();
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

        ResolvePlayerBindings();
        selfPane.Bind(selfInventory, this, "Your Inventory");
        otherPane.Bind(otherInventory, this, label);
        RebuildPanesLayout();
        CloseItemView();
    }

    // Re-resolves selfInventory and the equip slot view's bound EquipmentSlot from
    // PlayerReference.Instance - looked up fresh on every open rather than cached in Awake, see
    // the class-level comment for why.
    private void ResolvePlayerBindings()
    {
        PlayerReference player = PlayerReference.Instance;
        selfInventory = player != null ? player.GetComponent<Inventory>() : null;
        if (equipSlotView != null)
        {
            equipSlotView.SetSlot(player != null ? player.GetComponent<EquipmentSlot>() : null);
        }
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

    // Called after a cross-grid transfer. The item-view rebuild itself now happens
    // automatically on both sides via Inventory.Changed (see InventoryPanelView.Bind) - this
    // just handles the bookkeeping that isn't a grid-data change: the pane layout needs to
    // re-flow, and any held selectedView reference would otherwise dangle once views get
    // rebuilt out from under it.
    public void RefreshBothPanes()
    {
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
