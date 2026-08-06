using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Algorithmic core of the grid inventory: tracks which cells are occupied
// and handles placement/movement/rotation fit-checks. Not a MonoBehaviour -
// wrapped and owned by the Inventory component.
public class InventoryGrid
{
    public int width;
    public int height;

    private PlacedItem[,] occupancy;
    private List<PlacedItem> placedItems = new List<PlacedItem>();
    private int nextInstanceId = 1;

    public InventoryGrid(int width, int height)
    {
        this.width = width;
        this.height = height;
        occupancy = new PlacedItem[width, height];
    }

    public List<PlacedItem> GetPlacedItems()
    {
        return placedItems;
    }

    public bool CanPlaceItem(ItemData itemData, int rotationSteps, int originX, int originY, PlacedItem ignore = null)
    {
        List<Vector2Int> shape = ItemShapeUtility.GetRotatedShape(itemData.shape, rotationSteps);
        foreach (Vector2Int cell in shape)
        {
            int x = originX + cell.x;
            int y = originY + cell.y;
            if (x < 0 || x >= width || y < 0 || y >= height) return false;

            PlacedItem occupant = occupancy[x, y];
            if (occupant != null && occupant != ignore) return false;
        }
        return true;
    }

    public PlacedItem PlaceItem(ItemData itemData, int rotationSteps, int originX, int originY, int quantity = 1)
    {
        if (!CanPlaceItem(itemData, rotationSteps, originX, originY)) return null;

        PlacedItem item = new PlacedItem
        {
            instanceId = nextInstanceId++,
            itemData = itemData,
            rotationSteps = rotationSteps,
            gridX = originX,
            gridY = originY,
            quantity = quantity
        };
        WriteOccupancy(item);
        placedItems.Add(item);
        return item;
    }

    public bool MoveItem(PlacedItem item, int newX, int newY, int newRotationSteps)
    {
        if (!CanPlaceItem(item.itemData, newRotationSteps, newX, newY, item)) return false;

        ClearOccupancy(item);
        item.gridX = newX;
        item.gridY = newY;
        item.rotationSteps = newRotationSteps;
        WriteOccupancy(item);
        return true;
    }

    public bool RotateHeldItem(PlacedItem item)
    {
        int newRotation = (item.rotationSteps + 1) % 4;
        return MoveItem(item, item.gridX, item.gridY, newRotation);
    }

    public void RemoveItem(PlacedItem item)
    {
        ClearOccupancy(item);
        placedItems.Remove(item);
    }

    private void WriteOccupancy(PlacedItem item)
    {
        foreach (Vector2Int cell in item.GetOccupiedCells())
        {
            occupancy[cell.x, cell.y] = item;
        }
    }

    private void ClearOccupancy(PlacedItem item)
    {
        foreach (Vector2Int cell in item.GetOccupiedCells())
        {
            occupancy[cell.x, cell.y] = null;
        }
    }
}
