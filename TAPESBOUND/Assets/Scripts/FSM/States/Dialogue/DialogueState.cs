using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu (fileName = "DialogueState", menuName = "FSM/States/Dialogue", order = 3)]
public class DialogueState : BaseState
{
    private BaseState previousState = null;

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        Debug.Log("Entered Dialogue State");
    }

    public override void OnStateTick()
    {
        Debug.Log("Updating Dialogue State");

        stateMachine.EnterState(typeof(PatrolState));
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        Debug.Log("Exited Dialogue State");
    }

    private void OnEnable()
    {
        DialogueManager.onDialogueEnter += () => stateMachine.EnterState(typeof(DialogueState));
        DialogueManager.onDialogueExit += () => stateMachine.EnterState(typeof(DialogueState));
    }
    private void OnDisable()
    {
        DialogueManager.onDialogueEnter -= () => stateMachine.EnterState(typeof(DialogueState));
        DialogueManager.onDialogueExit -= () => stateMachine.EnterState(typeof(DialogueState));
    }
}
