using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Plain data representation of an item instance placed on the inventory grid.
// No scene presence of its own - InventoryItemView (UI) reads from this.
public class PlacedItem
{
    public int instanceId;
    public ItemData itemData;
    public int rotationSteps; // 0-3
    public int gridX;
    public int gridY;
    public int quantity;

    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> rotatedShape = ItemShapeUtility.GetRotatedShape(itemData.shape, rotationSteps);
        List<Vector2Int> worldCells = new List<Vector2Int>(rotatedShape.Count);
        foreach (Vector2Int cell in rotatedShape)
        {
            worldCells.Add(new Vector2Int(gridX + cell.x, gridY + cell.y));
        }
        return worldCells;
    }
}
