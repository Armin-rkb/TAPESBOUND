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
    }

    public override void OnStateTick()
    {
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
    }

    private void EnterDialogue()
    {
        previousState = stateMachine.currentState;
        stateMachine.EnterState(typeof(DialogueState));
    }
    
    private void ExitDialogue()
    {
        stateMachine.EnterState(previousState);
        previousState = null;
    }

    private void OnEnable()
    {
        DialogueManager.onDialogueEnter += EnterDialogue;
        DialogueManager.onDialogueExit += ExitDialogue;
    }
    private void OnDisable()
    {
        DialogueManager.onDialogueEnter -= EnterDialogue;
        DialogueManager.onDialogueExit -= ExitDialogue;
    }
}
