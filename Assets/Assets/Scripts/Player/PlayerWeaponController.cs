using UnityEngine;

// Keeps the player's actual gun GameObject in sync with whatever's currently equipped in
// weaponSlot: instantiates itemData.weaponPrefab under gunMount on equip, destroys the
// previous instance on unequip or swap. Also keeps PlayerStats.currentGun pointed at
// whichever instance (if any) is live, since DynamicCircleCursor reads accuracy through
// that reference.
[RequireComponent(typeof(PlayerStats))]
public class PlayerWeaponController : MonoBehaviour
{
    public EquipmentSlot weaponSlot;
    public Transform gunMount;

    private PlayerStats playerStats;
    private GameObject currentGunInstance;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
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
