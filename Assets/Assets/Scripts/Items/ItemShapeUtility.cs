using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemShapeUtility
{
    // Rotates each cell 90 degrees clockwise: (x, y) -> (y, -x), then
    // re-normalizes so the minimum x/y are 0. Call repeatedly for further rotations.
    public static List<Vector2Int> RotateShape90(List<Vector2Int> shape)
    {
        List<Vector2Int> rotated = new List<Vector2Int>(shape.Count);
        foreach (Vector2Int cell in shape)
        {
            rotated.Add(new Vector2Int(cell.y, -cell.x));
        }
        return NormalizeShape(rotated);
    }

    public static List<Vector2Int> NormalizeShape(List<Vector2Int> shape)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        foreach (Vector2Int cell in shape)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.y < minY) minY = cell.y;
        }

        List<Vector2Int> normalized = new List<Vector2Int>(shape.Count);
        foreach (Vector2Int cell in shape)
        {
            normalized.Add(new Vector2Int(cell.x - minX, cell.y - minY));
        }
        return normalized;
    }

    // rotationSteps: 0 = 0 degrees, 1 = 90, 2 = 180, 3 = 270 (clockwise)
    public static List<Vector2Int> GetRotatedShape(List<Vector2Int> baseShape, int rotationSteps)
    {
        List<Vector2Int> current = baseShape;
        int steps = ((rotationSteps % 4) + 4) % 4;
        for (int i = 0; i < steps; i++)
        {
            current = RotateShape90(current);
        }
        return current;
    }
}
