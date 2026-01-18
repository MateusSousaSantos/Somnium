using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrouchIdleState : PlayerState
{
    #region variables
    private Vector2 moveInput;
    new Rigidbody2D rigidbody;
    private Animator animator;

    private PlayerStats playerStats;
    #endregion

    public override void EnterState(PlayerStateController playerMovmentController)
    {
        playerStats = playerMovmentController.GetComponent<PlayerStats>();
        playerStats.speed = 2; // Set speed to a lower value when crouching
        base.EnterState(playerMovmentController);
        rigidbody = playerMovmentController.GetComponent<Rigidbody2D>();
        animator = playerMovmentController.GetComponent<Animator>();
        rigidbody.linearVelocity = 0 * moveInput; // Reset velocity to zero when entering crouch state
        if (animator != null)
        {
            animator.SetTrigger("crouchIdle"); // Set animation parameter
        }
    }

    public override void ExitState()
    {
        playerStats.concentration += 25f; // Reset concentration when exiting crouch state
    }

    public override void UpdateState()
    {
        rigidbody.linearVelocity = moveInput * playerStats.speed; 
        
        // If player starts moving while still holding Shift, transition to crouch moving state
        if (moveInput != Vector2.zero && Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetTrigger("crouchMoving");
            playerMovmentController.transitionToState(playerMovmentController.crouchState);
        }
        
        // If player releases Shift while idle, go back to idle
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            playerMovmentController.transitionToState(playerMovmentController.idleState);
        }
    }

    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }
}

