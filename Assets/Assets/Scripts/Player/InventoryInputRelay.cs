using UnityEngine;
using UnityEngine.InputSystem;

// PlayerInput's notification behavior on Player.prefab is "Send Messages", which - per Unity's
// Input System - only calls methods on components attached to the SAME GameObject as PlayerInput,
// not on children or other objects. InventoryUIController lives on its own object under
// Bootstrap now (see CameraFollow for the same "found via PlayerReference.Instance, not
// parented under Player" idiom), so this stays behind on Player purely to catch the "I" toggle
// message and forward it to whichever InventoryUIController is currently active.
public class InventoryInputRelay : MonoBehaviour
{
    private void OnToggleInventory(InputValue value)
    {
        if (!value.isPressed) return;
        InventoryUIController.Instance?.Toggle();
    }
}
