using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RevolverScript : GunBase
{
    [SerializeField] GameObject bullet;
    private Transform barrel;

    private float fireRate = 1f;

    private Animator animator;

    private int maxAmmo = 6;
    [SerializeField] private int currentAmmo;

    [SerializeField] private bool canShoot = true;
    private GameObject lanter; // Add this line



    protected override void Start()
    {
        base.Start();

        //retirar codigo abaixo, quando inventario estiver feito
        currentAmmo = maxAmmo;
        barrel = transform.Find("Barrel");
        canShoot = true;
        animator = GetComponent<Animator>();
        lanter = transform.Find("Lanter").gameObject;
    }
    protected override void Update()
    {
        base.Update(); // handles the InventoryUIController.IsOpen guard + aiming
        if (InventoryUIController.IsOpen) return;

        RevolverShoot();
        RevolverReload();

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (!lanter.activeSelf)
            {
                lanter.SetActive(true);
            }
            else
            {
                lanter.SetActive(false);
            }
        }

        // Change game speed with keys
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Set normal speed
        {
            Time.timeScale = 1f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // Slow down the game
        {
            Time.timeScale = 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) // Speed up the game
        {
            Time.timeScale = 2f;
        }

    }

    private void RevolverShoot()
    {

        if (Input.GetMouseButtonDown(0) && canShoot == true)
        {
            if (currentAmmo > 0)
            {
                // Get accuracy and convert to randomness factor (higher accuracy = lower randomness)
                float accuracy = GetAccuracy();
                float randomness = 1f - accuracy; // Invert: 1 = max randomness, 0 = no randomness
                float maxRandomOffset = 10f;
                float randomOffset = UnityEngine.Random.Range(-maxRandomOffset * randomness, maxRandomOffset * randomness);

                Quaternion rotation = barrel.rotation * Quaternion.Euler(barrel.rotation.x, barrel.rotation.y, barrel.rotation.z + randomOffset);
                animator.SetTrigger("Shoot");
                Instantiate(bullet, barrel.position, rotation);
                StartCoroutine(Cooldown(fireRate));
                currentAmmo--;
            }
            else
            {
                Debug.Log("Out of Ammo");
            }
        }
    }

    private void RevolverReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            animator.SetTrigger("Reload");
            int missingAmmo = maxAmmo - currentAmmo;
            StartCoroutine(ReloadCooldown(0.5f, missingAmmo));
        }
    }

    public IEnumerator Cooldown(float time)
    {
        if (canShoot)
        {
            canShoot = false;
            yield return new WaitForSeconds(time);
            canShoot = true;
        }
    }

    public IEnumerator ReloadCooldown(float timePerBullet, int missingAmmo)
    {

        if (canShoot)
        {
            float totalReloadTime = timePerBullet * missingAmmo; // Step 1
            float defaultAnimationLength = 1.0f; // This should be the actual length of your reload animation in seconds. Adjust accordingly.
            float newAnimationSpeed = defaultAnimationLength / totalReloadTime; // Step 3

            animator.speed = newAnimationSpeed; // Step 4
            animator.SetTrigger("Reload");
            canShoot = false;

            for (int i = 0; i < missingAmmo; i++)
            {
                currentAmmo++;
                yield return new WaitForSeconds(timePerBullet);
            }

            canShoot = true;
            animator.speed = 1.0f; // Step 5: Reset the animator speed to default
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (playerStats == null)
            return;

        Transform playerTransform = playerStats.transform;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;

        Vector3 direction = mousePosition - playerTransform.position;

        // Clamp the direction to AccuracySystem.MaxDistance
        if (direction.magnitude > AccuracySystem.MaxDistance)
        {
            direction = direction.normalized * AccuracySystem.MaxDistance;
        }

        Vector3 endPoint = playerTransform.position + direction;

        Gizmos.DrawLine(barrel.position, endPoint);
    }

}
