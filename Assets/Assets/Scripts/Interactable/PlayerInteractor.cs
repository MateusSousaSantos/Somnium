using System.Collections.Generic;
using UnityEngine;

// Replaces the old per-container trigger collider: each frame, sweeps a circle around the
// player, picks the closest IInteractable in range, and lets E interact with it. Attach
// this to the Player GameObject (alongside PlayerStats etc.) - see EquipmentSlot/Inventory
// for the codebase's other "player owns the check, world objects just react" idioms.
public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 2.8f;
    public LayerMask interactableMask = ~0;

    // Reused List for the non-alloc OverlapCircle overload (OverlapCircleNonAlloc's
    // Collider2D[] form is obsolete as of Unity 6) - Physics2D.OverlapCircle clears and
    // refills this in place each call rather than allocating a new list.
    private readonly List<Collider2D> overlapResults = new List<Collider2D>(16);
    private ContactFilter2D contactFilter;

    private IInteractable closest;

    public IInteractable Closest => closest;

    void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(interactableMask);
        contactFilter.useTriggers = true;
    }

    void Update()
    {
        closest = FindClosestInteractable();

        if (closest != null && Input.GetKeyDown(KeyCode.E))
        {
            closest.Interact();
        }

        CloseLootIfOutOfRange();
    }

    private IInteractable FindClosestInteractable()
    {
        Vector2 origin = transform.position;
        int count = Physics2D.OverlapCircle(origin, interactRange, contactFilter, overlapResults);

        IInteractable best = null;
        float bestSqrDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = overlapResults[i];
            if (col == null) continue;

            // GetComponentInParent so the collider can live on a child of the interactable
            // root (matches how LootContainerBase's collider sits on the same object as
            // LootContainer today, but doesn't force that layout for future interactables).
            IInteractable interactable = col.GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            float sqrDist = (origin - (Vector2)col.transform.position).sqrMagnitude;
            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                best = interactable;
            }
        }

        return best;
    }

    // Defensive, same as the old OnTriggerExit2D check it replaces: looting pauses time, so
    // walking out of range mid-loot shouldn't normally be reachable - this is a safety net
    // in case that ever changes. Checked against the specific container that's open (not
    // "closest") since the player could be closer to some other interactable than to it.
    private void CloseLootIfOutOfRange()
    {
        InventoryUIController ui = InventoryUIController.Instance;
        if (ui == null || !InventoryUIController.IsOpen) return;

        LootContainer looting = ui.CurrentLootContainer;
        if (looting == null) return;

        float dist = Vector2.Distance(transform.position, looting.transform.position);
        if (dist > interactRange) ui.Close();
    }
}
