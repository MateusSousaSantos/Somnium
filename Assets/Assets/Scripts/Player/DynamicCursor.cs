using System;
using UnityEngine;

public class DynamicCircleCursor : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    private PlayerStats playerStats;
    public float rotationSpeed = 30f; // degrees per second
    public float smoothSpeed = 5f; // smoothing speed for size transitions

    public float minSize = 1f;
    public float maxSize = 3f;
    public float accuracy; // Exposed for debugging

    private Vector3 targetScale = Vector3.one;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerStats = GetComponentInParent<PlayerStats>();
    }

    private void Update()
    {
        accuracy = playerStats.currentGun.GetComponent<RevolverScript>().AccuracyFromConcentration(playerStats.concentration);

        // Map accuracy to cursor size: high accuracy = small cursor, low accuracy = big cursor
        // Linear mapping from [0, 1] accuracy to [maxSize, minSize] cursor size
        float cursorSize = Mathf.Lerp(maxSize, minSize, accuracy);
        targetScale = Vector3.one * cursorSize;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, smoothSpeed * Time.deltaTime);

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        transform.position = mousePosition;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 15, 200, 20), "Accuracy: " + accuracy.ToString("F2"));
    }

}