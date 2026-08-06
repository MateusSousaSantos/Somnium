using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public int gridWidth = 8;
    public int gridHeight = 5;

    public InventoryGrid Grid { get; private set; }

    [Header("Debug / test items")]
    public ItemData debugAmmo357;
    public ItemData debugKnife;

    void Awake()
    {
        Grid = new InventoryGrid(gridWidth, gridHeight);
    }

    void Start()
    {
        if (debugAmmo357 != null) AddItem(debugAmmo357, 12);
        if (debugKnife != null) AddItem(debugKnife,1);
    }

    // Finds the first free position (scanning left-to-right, top-to-bottom)
    // that fits the item at rotation 0, and places it there.
    public PlacedItem AddItem(ItemData itemData, int quantity = 1)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (Grid.CanPlaceItem(itemData, 0, x, y))
                {
                    return Grid.PlaceItem(itemData, 0, x, y, quantity);
                }
            }
        }
        Debug.Log("Inventory full, could not place " + itemData.itemName);
        return null;
    }

    [ContextMenu("Debug Add Ammo357")]
    private void DebugAddAmmo357()
    {
        if (debugAmmo357 != null) AddItem(debugAmmo357, 6);
    }
}
