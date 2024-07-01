using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : PlayerState
{
    public override void EnterState(PlayerStateController playerMovmentController)
    {
        base.EnterState(playerMovmentController);
        Debug.Log("Idle State");
    }

    public override void ExitState()
    {
        Debug.Log("Exiting Idle State");
    }

    public override void UpdateState()
    {
        Debug.Log("Updating Idle State");
    }
}