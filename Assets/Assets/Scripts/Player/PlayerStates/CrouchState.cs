using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrouchState : PlayerState
{
    #region variables
    private Vector2 moveInput;
    new Rigidbody2D rigidbody;
    private float speed = 2.5F;
    #endregion

    public override void EnterState(PlayerStateController playerMovmentController)
    {

        base.EnterState(playerMovmentController);
        rigidbody = playerMovmentController.GetComponent<Rigidbody2D>();
    }

    public override void ExitState()
    {

    }

    public override void UpdateState()
    {
        rigidbody.velocity = moveInput * speed;
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
