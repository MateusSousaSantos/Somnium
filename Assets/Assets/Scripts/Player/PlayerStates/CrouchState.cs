using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchState : PlayerState
{
    public override void EnterState(PlayerStateController playerMovmentController)
    {
        base.EnterState(playerMovmentController);
        Debug.Log("Crouch State");
    }

    public override void ExitState()
    {
        Debug.Log("Crouch State");
    }

    public override void UpdateState()
    {
        Debug.Log("Crouch State");
    }
}
