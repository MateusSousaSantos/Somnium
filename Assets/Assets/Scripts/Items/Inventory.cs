using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // A single starting-item entry: what to add and how many. Used by any Inventory
    // (container or otherwise) to author its initial contents from the Inspector,
    // without per-container code.
    [System.Serializable]
    public struct ItemStack
    {
        public ItemData itemData;
        public int quantity;
    }

    public int gridWidth = 8;
    public int gridHeight = 5;

    public InventoryGrid Grid { get; private set; }

    // Forwarded from Grid so bound views only ever need to know about Inventory, not the grid
    // instance underneath it (Grid itself isn't created until Awake).
    public event Action Changed
    {
        add => Grid.Changed += value;
        remove => Grid.Changed -= value;
    }

    [Header("Starting items")]
    public List<ItemStack> startingItems = new List<ItemStack>();

    void Awake()
    {
        Grid = new InventoryGrid(gridWidth, gridHeight);
    }

    void Start()
    {
        foreach (ItemStack stack in startingItems)
        {
            if (stack.itemData != null) AddItem(stack.itemData, stack.quantity);
        }
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
}
