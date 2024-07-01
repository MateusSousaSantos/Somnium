using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : PlayerState
{
    public override void EnterState(PlayerStateController playerMovmentController)
    {
        base.EnterState(playerMovmentController);
        Debug.Log("Walking State");
    }

    public override void ExitState()
    {
        Debug.Log("Walking State");
    }

    public override void UpdateState()
    {
        Debug.Log("Walking State");
    }
}