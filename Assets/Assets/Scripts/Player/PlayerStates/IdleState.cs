using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IdleState : PlayerState
{

    private Vector2 moveInput;
    private Animator animator;
    public override void EnterState(PlayerStateController playerMovmentController)
    {
        base.EnterState(playerMovmentController);
        animator = playerMovmentController.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("idle"); // Set animation parameter
        }
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        if (moveInput != Vector2.zero)
        {   
            if(Input.GetKey(KeyCode.LeftShift))
            {
                playerMovmentController.transitionToState(playerMovmentController.crouchState);
            }
            else
            {
                playerMovmentController.transitionToState(playerMovmentController.walkingState);
            }
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerMovmentController.transitionToState(playerMovmentController.crouchState);
        }

    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}