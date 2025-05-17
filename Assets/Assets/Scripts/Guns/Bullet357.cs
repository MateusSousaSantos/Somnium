using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet357 : MonoBehaviour
{
    private int bulletVelocity = -40;
    void Update()
    {
        transform.Translate(Vector2.right * bulletVelocity * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if(hitInfo.tag != "Player"){
            Destroy(gameObject);
        }
    }
}
