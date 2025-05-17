using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerStateController : MonoBehaviour
{
    public PlayerState currentState;
    private PlayerStats playerStats;
    public IdleState idleState;
    public WalkingState walkingState;
    public CrouchState crouchState;
    public DeadState deadState;


    private float detectionRange = 2F;

    private void StartComponents()
    {

        idleState = GetComponent<IdleState>();
        walkingState = GetComponent<WalkingState>();
        crouchState = GetComponent<CrouchState>();
        deadState = GetComponent<DeadState>();
        playerStats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        StartComponents();
        transitionToState(walkingState);
    }

    void Update()
    {
        currentState.UpdateState();
        if (Input.GetKeyDown(KeyCode.E))
        {
            checkForInteractable();
        }
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void OnGUI()
    {
        string stateName = currentState.GetType().Name;
        GUILayout.Label("Current State: " + stateName);
    }

    public void transitionToState(PlayerState state)
    {
        currentState?.ExitState();
        currentState = state;
        currentState.EnterState(this);
    }


    public void checkForInteractable()
    {

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, LayerMask.GetMask("Interactable"));

        var closestInteractable = hitColliders[0].transform;
        var closestDistance = Vector2.Distance(transform.position, closestInteractable.position);
        foreach (var hitCollider in hitColliders)
        {
            var distance = Vector2.Distance(transform.position, hitCollider.transform.position);   
            if (distance < closestDistance)
            {
                closestInteractable = hitCollider.transform;
                closestDistance = distance;
            }
        }
        if (closestInteractable != null)
        {
            closestInteractable.GetComponent<Interactable>().Interact();
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
