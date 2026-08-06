using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    public GameObject inventoryPanel;
    public RectTransform gridContainer;
    public GameObject cellBackgroundPrefab; // reuses InventoryUiTile.png
    public GameObject itemViewPrefab;       // the draggable item view
    public ItemDetailPanel detailPanel;

    public int cellSize = 48;

    private Inventory inventory;
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;
    private InventoryItemView selectedView;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
        inventoryPanel.SetActive(false);
    }

    void Start()
    {
        BuildGridBackground();
    }

    private void OnToggleInventory(InputValue value)
    {
        if (!value.isPressed) return;
        if (IsOpen) Close(); else Open();
    }

    public void Open()
    {
        IsOpen = true;
        inventoryPanel.SetActive(true);
        Time.timeScale = 0f;
        previousLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        RefreshItemViews();
        selectedView = null;
        detailPanel.Clear();
    }

    public void Close()
    {
        IsOpen = false;
        inventoryPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = previousLockState;
        Cursor.visible = previousCursorVisible;
        selectedView = null;
        detailPanel.Clear();
    }

    public void SelectItem(InventoryItemView view)
    {
        if (selectedView != null) selectedView.SetSelected(false);
        selectedView = view;
        view.SetSelected(true);
        detailPanel.ShowItem(view.PlacedItem);
    }

    private void BuildGridBackground()
    {
        for (int y = 0; y < inventory.gridHeight; y++)
        {
            for (int x = 0; x < inventory.gridWidth; x++)
            {
                GameObject cell = Instantiate(cellBackgroundPrefab, gridContainer);
                RectTransform rt = cell.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
                rt.sizeDelta = new Vector2(cellSize, cellSize);
            }
        }
    }

    private void RefreshItemViews()
    {
        foreach (Transform child in gridContainer)
        {
            InventoryItemView view = child.GetComponent<InventoryItemView>();
            if (view != null) Destroy(child.gameObject);
        }

        foreach (PlacedItem placedItem in inventory.Grid.GetPlacedItems())
        {
            GameObject go = Instantiate(itemViewPrefab, gridContainer);
            InventoryItemView view = go.GetComponent<InventoryItemView>();
            view.Initialize(inventory.Grid, placedItem, cellSize, this);
        }
    }
}
