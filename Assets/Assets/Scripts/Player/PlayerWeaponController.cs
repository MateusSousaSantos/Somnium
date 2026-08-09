using UnityEngine;

// Keeps the player's actual gun GameObject in sync with whatever's currently equipped in
// weaponSlot: instantiates itemData.weaponPrefab under gunMount on equip, destroys the
// previous instance on unequip or swap. Also keeps PlayerStats.currentGun pointed at
// whichever instance (if any) is live, since DynamicCircleCursor reads accuracy through
// that reference.
//
// weaponSlot is resolved via GetComponent rather than an Inspector reference - EquipmentSlot
// lives on this same Player GameObject (alongside Inventory; see PlayerInteractor's header
// comment for the "player owns the check" idiom), not on the equip slot's UI object, so a
// prefab-asset-safe same-object reference is all that's needed here.
[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(EquipmentSlot))]
public class PlayerWeaponController : MonoBehaviour
{
    public Transform gunMount;

    private EquipmentSlot weaponSlot;
    private PlayerStats playerStats;
    private GameObject currentGunInstance;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        weaponSlot = GetComponent<EquipmentSlot>();
    }

    void OnEnable()
    {
        if (weaponSlot != null) weaponSlot.Changed += SyncGun;
    }

    void OnDisable()
    {
        if (weaponSlot != null) weaponSlot.Changed -= SyncGun;
    }

    void Start()
    {
        SyncGun();
    }

    private void SyncGun()
    {
        if (currentGunInstance != null)
        {
            Destroy(currentGunInstance);
            currentGunInstance = null;
        }

        GameObject prefab = weaponSlot != null ? weaponSlot.EquippedItem?.itemData.weaponPrefab : null;
        if (prefab == null)
        {
            playerStats.currentGun = null;
            return;
        }

        currentGunInstance = Instantiate(prefab, gunMount);
        playerStats.currentGun = currentGunInstance;
    }
}
