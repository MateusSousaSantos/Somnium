using UnityEngine;

// Sits on world container GameObjects (chest, crate, locker, ...) alongside a
// same-object Inventory. No longer tracks proximity itself - PlayerInteractor finds the
// closest IInteractable within range each frame and calls Interact() on E, matching the
// codebase's existing legacy-Input convention for one-off interaction keys (reload/
// flashlight/crouch/in-drag-rotate all do the same instead of adding Input Actions).
[RequireComponent(typeof(Inventory))]
public class LootContainer : MonoBehaviour, IInteractable
{
    public string displayName = "Container";

    public string DisplayName => displayName;

    private Inventory inventory;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
    }

    public void Interact()
    {
        if (InventoryUIController.Instance == null) return;

        if (InventoryUIController.Instance.IsLooting(this))
        {
            InventoryUIController.Instance.Close();
        }
        else
        {
            InventoryUIController.Instance.OpenLoot(inventory, displayName, this);
        }
    }
}
