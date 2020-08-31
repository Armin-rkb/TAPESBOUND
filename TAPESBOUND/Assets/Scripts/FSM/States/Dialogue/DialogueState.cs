using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu (fileName = "DialogueState", menuName = "FSM/States/Dialogue", order = 3)]
public class DialogueState : BaseState
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
