using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerStateController : MonoBehaviour
{
    public PlayerState currentState;
    public IdleState idleState;
    public WalkingState walkingState;
    public CrouchState crouchState;
    public CrouchIdleState crouchIdleState;
    public DeadState deadState;
    private SpriteRenderer spriteRenderer;

    private void StartComponents()
    {
        idleState = GetComponent<IdleState>();
        walkingState = GetComponent<WalkingState>();
        crouchState = GetComponent<CrouchState>();
        crouchIdleState = GetComponent<CrouchIdleState>();
        deadState = GetComponent<DeadState>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        StartComponents();
        transitionToState(idleState);
    }

    void Update()
    {
        currentState.UpdateState();

        UpdateFacingDirection();
    }
    private void OnDrawGizmos()
    {
    }

    private void OnGUI()
    {
        string stateName = currentState.GetType().Name;
        GUI.Label(new Rect(10, 0, 200, 20), "State: " + stateName);
    }

    public void transitionToState(PlayerState state)
    {
        currentState?.ExitState();
        currentState = state;
        currentState.EnterState(this);
    }


    private void UpdateFacingDirection()
    {
        // Get the mouse position in world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Compare the mouse position with the player's position
        if (mousePosition.x > transform.position.x)
        {
            spriteRenderer.flipX = true; // Facing right
        }
        else if (mousePosition.x < transform.position.x)
        {
            spriteRenderer.flipX = false; // Facing left
        }
    }

}

public abstract class PlayerState : MonoBehaviour
{
    protected PlayerStateController playerMovmentController;


    public virtual void EnterState(PlayerStateController playerMovmentController)
    {
        this.playerMovmentController = playerMovmentController;
    }

    public abstract void ExitState();
    public abstract void UpdateState();


}

