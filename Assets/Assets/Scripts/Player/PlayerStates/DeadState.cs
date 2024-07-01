using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : PlayerState
{
    public override void EnterState(PlayerStateController playerMovmentController)
    {
        base.EnterState(playerMovmentController);
        Debug.Log("Dead State");
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