using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrouchState : PlayerState
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
        playerStats.concentration -= 25f; // Decrease concentration when crouching
        base.EnterState(playerMovmentController);
        rigidbody = playerMovmentController.GetComponent<Rigidbody2D>();
        animator = playerMovmentController.GetComponent<Animator>();
        rigidbody.velocity = 0 * moveInput; // Reset velocity to zero when entering crouch state
        if (animator != null)
        {
            animator.SetTrigger("crouch"); // Set animation parameter
        }
    }

    public override void ExitState()
    {
        playerStats.concentration += 25f; // Reset concentration when exiting crouch state
    }

    public override void UpdateState()
    {


        rigidbody.velocity = moveInput * playerStats.speed; 
        if (moveInput != Vector2.zero)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                animator.SetTrigger("crouchMoving");
            }
            else
            {
                playerMovmentController.transitionToState(playerMovmentController.walkingState);
            }
        }
        if (moveInput == Vector2.zero && Input.anyKey == false)
        {
            playerMovmentController.transitionToState(playerMovmentController.idleState);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            playerMovmentController.transitionToState(playerMovmentController.walkingState);
        }

    }

    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }
}
