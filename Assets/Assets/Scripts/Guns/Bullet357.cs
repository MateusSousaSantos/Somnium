using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet357 : MonoBehaviour
{
    private int bulletVelocity = -40;
    public GameObject bulletImpactPrefab;
    void Update()
    {
        transform.Translate(Vector2.right * bulletVelocity * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.tag != "Player")
        {
            //transform.rotation ao  contrario
            transform.rotation = Quaternion.Euler(0, 0, 180);
            Instantiate(bulletImpactPrefab, transform.position, transform.rotation);
            Destroy(gameObject);
            
        }
    }
}
