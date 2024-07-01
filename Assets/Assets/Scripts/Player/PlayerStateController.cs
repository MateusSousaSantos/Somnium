using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    public PlayerState currentState;
    
    public IdleState idleState;
    public WalkingState walkingState;
    public CrouchState crouchState;
    //public DeadState deadState;

    private void StartComponents()
    {

       idleState = GetComponent<IdleState>();
        walkingState = GetComponent<WalkingState>();
        crouchState = GetComponent<CrouchState>();
        //deadState = GetComponent<DeadState>();
    }

    void Start()
    {
        StartComponents();
        TransitionToState(idleState);
    }

    void Update()
    {
        
    }

    public void TransitionToState(PlayerState state)
    {
        currentState?.ExitState();
        currentState = state;
        currentState.EnterState(this);
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
