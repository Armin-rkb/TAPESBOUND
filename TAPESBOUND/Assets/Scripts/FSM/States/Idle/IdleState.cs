using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu (fileName = "IdleState", menuName = "FSM/States/Idle", order = 1)]
public class IdleState : BaseState
{
    public int i = 0;
    public override void OnStateEnter()
    {
        i = 0;
        base.OnStateEnter();
        Debug.Log("Entered Idle State");
    }

    public override void OnStateTick()
    {
        Debug.Log("Updating Idle State");

        i++;
        if (i > 300)
        {
            stateMachine.EnterState(typeof(PatrolState));
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        Debug.Log("Exited Idle State");
    }
}
