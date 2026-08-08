using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WalkingState : PlayerState
{
    #region variables
    private Vector2 moveInput;
    new Rigidbody2D rigidbody;
    private Coroutine stepsCoroutine;
    public GameObject step;

    private Animator animator; 
    private PlayerStats playerStats;
    #endregion

    public override void EnterState(PlayerStateController playerMovmentController)
    {

        base.EnterState(playerMovmentController);
        playerStats = playerMovmentController.GetComponent<PlayerStats>();
        playerStats.speed = 5; 
        playerStats.concentration -= 50f; 
        rigidbody = playerMovmentController.GetComponent<Rigidbody2D>();
        animator = playerMovmentController.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetTrigger("walk"); 
        }

        stepsCoroutine = playerMovmentController.StartCoroutine(CreateStepAtIntervals());

    }

    public override void ExitState()
    {
        if (stepsCoroutine != null)
        {
            playerMovmentController.StopCoroutine(stepsCoroutine);
        }
        playerStats.concentration += 50f; // Reset concentration when exiting walking state
    }

    public override void UpdateState()
    {
        // else-if: these two used to be independent ifs, so stopping and pressing crouch in
        // the same frame fired both transitions (Walking -> Idle -> Crouch within one Update),
        // double-calling EnterState/ExitState. Shift is checked first so that simultaneous
        // case still lands on Crouch (matching player intent) instead of losing the
        // GetKeyDown(Shift) edge to the Idle transition and missing the crouch entirely.
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerMovmentController.transitionToState(playerMovmentController.crouchState);
        }
        else if (moveInput == Vector2.zero)
        {
            playerMovmentController.transitionToState(playerMovmentController.idleState);
        }

        // Kept after the transition checks (matching the original order) so a same-frame
        // transition's new speed (e.g. Crouch's slower speed) is reflected immediately -
        // rigidbody/playerStats are the same shared components regardless of which state
        // object is currently mid-UpdateState.
        rigidbody.linearVelocity = moveInput * playerStats.speed;
    }

    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }

    private IEnumerator CreateStepAtIntervals()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            CreateTemporaryObject();
        }
    }

    private void CreateTemporaryObject()
    {
        GameObject newObject = Instantiate(step, playerMovmentController.transform.position, Quaternion.identity);
        Destroy(newObject, 2f);
    }
}