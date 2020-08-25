using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu (fileName = "IdleState", menuName = "FSM/States/Idle", order = 1)]
public class IdleState : BaseState
{
    public override void OnStateEnter()
    {
        base.OnStateEnter();
    }

    public override void OnStateTick()
    {
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
    }
}
