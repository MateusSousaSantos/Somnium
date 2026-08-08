using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Somnium/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string itemDescription;
    public Sprite itemIcon;      // used for both grid cells and the detail panel's big icon

    public ItemType itemType = ItemType.Generic;
    public bool isStackable = false;
    public int maxStackSize = 1;

    // Occupied cell offsets in the item's default (0 degree) orientation,
    // normalized so the minimum x and y are 0 (top-left anchored).
    public List<Vector2Int> shape = new List<Vector2Int> { Vector2Int.zero };

    // Reserved for a future per-item docked view in ContextWindow (e.g. a weapon's stats
    // panel) - InventoryUIController.ShowItemView would instantiate this instead of the
    // default ItemDetailPanel when set. Not consumed anywhere yet; leave null for default behavior.
    public GameObject customItemViewPrefab;

    // The gun GameObject to instantiate under the player's gun mount while this item is
    // equipped in the weapon EquipmentSlot (see PlayerWeaponController). Only meaningful for
    // ItemType.Weapon items; leave null for non-weapons.
    public GameObject weaponPrefab;
}

public enum ItemType
{
    Generic,
    Ammo,
    Weapon,
    Key
}
