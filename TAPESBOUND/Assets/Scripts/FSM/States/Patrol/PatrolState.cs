using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu (fileName = "PatrolState", menuName = "FSM/States/Patrol", order = 2)]
public class PatrolState : BaseState
{
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        Debug.Log("Entered Patrol State");
    }

    public override void OnStateTick()
    {
        Debug.Log("Updating Patrol State");
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        Debug.Log("Exited Patrol State");
    }
}
