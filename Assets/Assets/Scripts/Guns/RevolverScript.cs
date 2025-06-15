using System.Collections;
using System.Xml.Schema;
using Unity.Mathematics;
using UnityEngine;

public class RevolverScript : MonoBehaviour
{
    [SerializeField] GameObject bullet;
    private Transform barrel;
    private float fireRate = 1f;

    private Animator animator;

    private int maxAmmo = 6;
    [SerializeField] private int currentAmmo;

    [SerializeField] private bool canShoot = true;
    private GameObject lanter; // Add this line

    private DynamicCircleCursor dynamicCircleCursor;


    private void Start()
    {
        //retirar codigo abaixo, quando inventario estiver feito
        dynamicCircleCursor = FindObjectOfType<DynamicCircleCursor>();
        currentAmmo = maxAmmo;
        barrel = transform.Find("Barrel");
        canShoot = true;
        animator = GetComponent<Animator>();
        lanter = transform.Find("Lanter").gameObject;
    }
    private void Update()
    {
        RevolverAim();
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
                float x = dynamicCircleCursor.targetRadius;
                float randomOffset = UnityEngine.Random.Range(-10 , 10);
                randomOffset *= (x - 0.2f) * 2;

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

    private void RevolverAim()
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


}
