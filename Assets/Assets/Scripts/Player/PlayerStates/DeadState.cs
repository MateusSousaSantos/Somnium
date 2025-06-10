using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : PlayerState
{   
    private Animator animator;
    public override void EnterState(PlayerStateController playerMovmentController)
    {
        base.EnterState(playerMovmentController);
        Debug.Log("Dead State");
        animator = playerMovmentController.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("dead"); // Set animation parameter
        }
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Dead State");
    }

    public override void UpdateState()
    {
        Debug.Log("Updating Dead State");
    }
}