using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class IdleState : PlayerState
{

    private Vector2 moveInput;

    public override void EnterState(PlayerStateController playerMovmentController)
    {
        base.EnterState(playerMovmentController);
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        if (moveInput != Vector2.zero)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                playerMovmentController.transitionToState(playerMovmentController.crouchState);
            }
            playerMovmentController.transitionToState(playerMovmentController.walkingState);

        }

    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}