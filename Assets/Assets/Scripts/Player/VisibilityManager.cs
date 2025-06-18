using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityManager : MonoBehaviour
{
    private static VisibilityManager instance;
    [SerializeField]
    private HashSet<EnemyController> seenEnemies = new HashSet<EnemyController>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void ReportSeenEnemy(EnemyController enemy)
    {
        if (instance != null && enemy != null)
        {
            instance.seenEnemies.Add(enemy);
            enemy.beingSeen = true;
        }
    }

    public static void ReportUnseenEnemy(EnemyController enemy)
    {
        if (instance != null && enemy != null)
        {
            instance.seenEnemies.Remove(enemy);
            enemy.beingSeen = false;
        }
    }

    public static void UpdateVisibility()
    {
        if (instance != null)
        {
            foreach (EnemyController enemy in FindObjectsOfType<EnemyController>())
            {
                if (!instance.seenEnemies.Contains(enemy))
                {
                    enemy.beingSeen = false;
                }
            }
        }
    }
}
