using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private PlayerStateController playerStateController;
    private DeadState deadState;
    public int currentHealth = 100;
    public int maxHealth = 100;

    //current gun
    public GameObject currentGun;

    public int speed = 5; 
    public float concentration = 100f; // Concentration level of the player

    void Awake()
    {
        playerStateController = GetComponent<PlayerStateController>();
        deadState = GetComponent<DeadState>();
        currentHealth = maxHealth;
        currentGun = transform.Find("Revolver").gameObject;
    }

    private void Update(){
        
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            playerStateController.transitionToState(deadState);
        }
    }
}
