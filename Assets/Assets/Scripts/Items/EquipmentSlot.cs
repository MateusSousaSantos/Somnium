using System;
using UnityEngine;

// A single, non-grid equip slot (e.g. the player's weapon slot). Unlike Inventory/InventoryGrid,
// an item's tetris shape is irrelevant here - any item of the right ItemType just fills this
// one slot, so different-shaped weapons can be slid in and out without the slot ever caring
// about a footprint or occupancy grid.
public class EquipmentSlot : MonoBehaviour
{
    public ItemType allowedType = ItemType.Weapon;

    public PlacedItem EquippedItem { get; private set; }

    // Fired whenever EquippedItem changes (Equip or Unequip) - lets systems like
    // PlayerWeaponController react without polling. EquipmentSlotView still calls Refresh()
    // itself right after mutating, for the UI half of this same state change.
    public event Action Changed;

    // Negative, separate from InventoryGrid's own nextInstanceId counters (which start at 1
    // and are per-grid) so ids handed out here can never collide with a grid's PlacedItem ids.
    private int nextInstanceId = -1;

    public bool CanAccept(ItemData itemData)
    {
        return EquippedItem == null && itemData.itemType == allowedType;
    }

    public PlacedItem Equip(ItemData itemData, int quantity)
    {
        if (!CanAccept(itemData)) return null;

        EquippedItem = new PlacedItem
        {
            instanceId = nextInstanceId--,
            itemData = itemData,
            quantity = quantity,
            rotationSteps = 0,
            gridX = 0,
            gridY = 0
        };
        Changed?.Invoke();
        return EquippedItem;
    }

    public void Unequip()
    {
        EquippedItem = null;
        Changed?.Invoke();
    }
}
