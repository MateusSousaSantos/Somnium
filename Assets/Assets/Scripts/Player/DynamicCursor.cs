using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DynamicCircleCursor : MonoBehaviour
{
    public Texture2D circleTexture; // Transparent circle texture

    private Vector2 cursorHotspot; // Hotspot for the cursor
    private Vector2 mousePosition; // Current mouse position
    [SerializeField]
    private PlayerStats playerStats;

    public float radius = 5f; // Radius of the circle
    public int segments = 50; // Number of segments for the circle

    public float thickness = 0.2f; // Thickness of the circle line
    private LineRenderer line;

    private void Awake()
    {
        if (circleTexture == null)
        {
            Debug.LogError("Circle texture is not assigned in the inspector.");
            return;
        }

        line = gameObject.GetComponent<LineRenderer>();

        if (line == null)
        {
            Debug.LogError("LineRenderer component is missing on the GameObject.");
            return;
        }

        line.positionCount = segments + 1;
        line.useWorldSpace = true;

        line.material = new Material(Shader.Find("Sprites/Default")); // Use a simple shader for the line

        gameObject.layer = LayerMask.NameToLayer("UI");

        line.startColor = Color.white; // Set the color of the line
        line.endColor = Color.white;   // Set the color of the line

        playerStats = GetComponent<PlayerStats>();

        line.startWidth = thickness; // Adjust this value for the desired thickness
        line.endWidth = thickness;   // Adjust this value for the desired thickness
    }

    void Start()
    {
        cursorHotspot = new Vector2(circleTexture.width / 2, circleTexture.height / 2);
        Cursor.SetCursor(circleTexture, cursorHotspot, CursorMode.Auto);
    }
    [SerializeField]
    public float targetRadius; // The target radius value
    private float smoothingSpeed = 5f; // Speed of smoothing
    void Update()
    {

        if (line == null)
        {
            Debug.LogError("LineRenderer is not initialized.");
            return;
        }
        if (playerStats != null)
        {
            float speedNormalized = playerStats.speed / 5f; // Normalize speed to a value between 0 and 1
            float concentration = playerStats.concentration / 100f; // Normalize concentration to a value between 0 and 1
            targetRadius = 0.7f * speedNormalized * concentration * 5f; // Calculate the target radius
            targetRadius = (float)(0.2 + 0.6 * targetRadius); // Adjust the target radius based on speed and concentration
        }
        if (targetRadius < 0.2f)
        {
            thickness = 0.01f; // Set a minimum thickness
        }

        // Smoothly transition the radius value
        radius = Mathf.Lerp(radius, targetRadius, Time.deltaTime * smoothingSpeed);
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Convert mouse position to world space
        CreatePoints(mousePosition);
    }

    void CreatePoints(Vector2 center)
    {
        if (line == null)
        {
            Debug.LogError("LineRenderer is not initialized.");
            return;
        }

        float angle = 0f;

        for (int i = 0; i < (segments + 1); i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius + center.x;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius + center.y;

            line.SetPosition(i, new Vector3(x, y, 0));
            line.sortingOrder = 10; // Ensure the line is rendered above other objects
            angle += (360f / segments);
        }
    }
}