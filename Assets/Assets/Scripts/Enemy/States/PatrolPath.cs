using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public List<Transform> patrolPoints = new();

    public int length { get => patrolPoints.Count; }

    [Header("Gizmos")]
    public Color lineColor = Color.green;

    public struct PathPoint
    {
        public int Index;
        public Vector2 Position;
    }
    public PathPoint GetClosestPathPoint(Vector3 postion)
    {
        var minDistance = float.MaxValue;
        var index = -1;

        for (int i = 0; i < patrolPoints.Count; i++)
        {
            var distance = Vector2.Distance(postion, patrolPoints[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
                index = i;
            }
        }

        return new PathPoint
        {
            Index = index,
            Position = patrolPoints[index].position
        };
    }

    public PathPoint GetNextPathPoint(int currentIndex)
    {
        var nextIndex = currentIndex + 1;
        if (nextIndex >= patrolPoints.Count)
        {
            nextIndex = 0;
        }

        return new PathPoint
        {
            Index = nextIndex,
            Position = patrolPoints[nextIndex].position
        };
    }

    private void OnDrawGizmos()
    {

        if (patrolPoints.Count == 0)
            return;

        foreach (var point in patrolPoints)
        {
            if (point == null)
            {
                return;
            }
        }

        for (int i = patrolPoints.Count - 1; i > 0; i--)
        {
            if (patrolPoints[i] == null)
            {
                return;
            }

            if (patrolPoints.Count == 1 || i == 0)
                return;

            Gizmos.color = lineColor;
            Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i - 1].position);

            if (patrolPoints.Count > 2 && i == patrolPoints.Count - 1)
            {
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
            }


        }

    }
}