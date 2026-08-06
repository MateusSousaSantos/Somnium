using UnityEngine;

/// <summary>
/// Attach to Bootstrap's Main Camera. Keeps the camera tracking PlayerReference.Instance now
/// that the camera lives in a persistent scene decoupled from level geometry, so nothing else
/// moves it as the player walks around a gameplay scene.
///
/// First pass only - no bounds/dead-zone logic yet.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 5f;

    private void LateUpdate()
    {
        PlayerReference player = PlayerReference.Instance;
        if (player == null) return;

        Vector3 targetPosition = player.transform.position;
        targetPosition.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
