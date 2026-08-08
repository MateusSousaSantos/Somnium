using UnityEngine;

// Shared base for all gun types: owns the player reference and the mouse-aim/flip behavior
// that's identical no matter which gun is equipped (aiming is purely a function of the gun's
// own transform vs the player and the mouse - it doesn't know about ammo, shooting, or any
// other per-weapon behavior). Concrete guns (e.g. RevolverScript) override Start()/Update() to
// layer their own logic on top - see RevolverScript for the pattern (base.Start()/base.Update()
// first, then its own setup/shoot/reload/etc.).
public abstract class GunBase : MonoBehaviour
{
    // >1 makes this gun more precise than AccuracySystem's base formula, <1 sloppier. Tunable
    // per gun prefab in the Inspector; 1 = no change from the shared formula.
    public float accuracyMultiplier = 1f;

    protected PlayerStats playerStats;

    public float GetAccuracy() => AccuracySystem.CalculateAccuracy(playerStats, accuracyMultiplier);

    protected virtual void Start()
    {
        playerStats = GetComponentInParent<PlayerStats>();
    }

    protected virtual void Update()
    {
        if (InventoryUIController.IsOpen) return;
        Aim();
    }

    // Rotates this gun to point at the mouse cursor, flipping it 180 degrees past the player's
    // own screen X so the sprite doesn't render upside-down when aiming to the left. Moved here
    // unchanged from RevolverScript.RevolverAim() - identical for any single-sprite gun that
    // pivots on the player.
    protected void Aim()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);

        Vector2 offset = new Vector2(screenPoint.x + 1f - mousePos.x, screenPoint.y + 1f - mousePos.y);

        float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (mousePos.x > screenPoint.x)
        {
            transform.Rotate(180, 0, 0);
        }
        else
        {
            transform.Rotate(0, 0, 0);
        }
    }
}
