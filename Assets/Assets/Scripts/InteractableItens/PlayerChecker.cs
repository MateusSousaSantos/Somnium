using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerChecker : MonoBehaviour
{
    public float detectionRange = 5f;
    public int numberOfRays = 65;

    public bool isPlayerInRange = false;

    void Update()
    {
        CastRays360();
    }

    void CastRays360()
    {
        for (int i = 0; i < numberOfRays; i++)
        {
            float angle = i * (360f / numberOfRays);
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectionRange);
            if (hit.collider != null)
            {
                if (hit.collider.tag != "Player")
                {
                    Debug.DrawRay(transform.position, direction * hit.distance, Color.yellow);
                }
                else
                {
                    Debug.DrawRay(transform.position, direction * detectionRange, Color.red);

                }
            }
            else
            {
                // No object was hit, draw the ray to its maximum range
                Debug.DrawRay(transform.position, direction * detectionRange, Color.green);
            }
        }
    }

    void ExecuteFunction()
    {
        // Your function logic here
        Debug.Log("Function Executed");
    }
}
