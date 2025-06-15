using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WalkingState : PlayerState
{
    #region variables
    private Vector2 moveInput;
    new Rigidbody2D rigidbody;
    private Coroutine createObjectCoroutine;
    public GameObject objectToCreate;

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

        createObjectCoroutine = playerMovmentController.StartCoroutine(CreateObjectAtIntervals());

    }

    public override void ExitState()
    {
        if (createObjectCoroutine != null)
        {
            playerMovmentController.StopCoroutine(createObjectCoroutine);
        }
        playerStats.concentration += 50f; // Reset concentration when exiting walking state
    }

    public override void UpdateState()
    {
        if (moveInput == Vector2.zero)
        {
            playerMovmentController.transitionToState(playerMovmentController.idleState);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            playerMovmentController.transitionToState(playerMovmentController.crouchState);
        }
        rigidbody.velocity = moveInput * playerStats.speed;


    }

    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }

    private IEnumerator CreateObjectAtIntervals()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            CreateTemporaryObject();
        }
    }

    private void CreateTemporaryObject()
    {
        GameObject newObject = Instantiate(objectToCreate, playerMovmentController.transform.position, Quaternion.identity);
        Destroy(newObject, 2f);
    }
}