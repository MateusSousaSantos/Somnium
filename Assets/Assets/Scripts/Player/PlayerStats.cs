using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    private PlayerStateController playerStateController;
    private DeadState deadState;
    public int currentHealth = 100;
    public int maxHealth = 100;

    void Awake()
    {
        playerStateController = GetComponent<PlayerStateController>();
        deadState = GetComponent<DeadState>();
        currentHealth = maxHealth;
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
