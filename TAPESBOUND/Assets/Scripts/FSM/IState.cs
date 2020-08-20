using UnityEditorInternal;
using UnityEngine;

public abstract class BaseState : ScriptableObject
{
    protected NPC npc = null;
    protected StateMachine stateMachine = null;

    // OnStateEnter is called upon switching states.
    public virtual void OnStateEnter() {}

    // OnStateTick is called once per frame
    public abstract void OnStateTick();

    // OnStateExit is called before switching states.
    public virtual void OnStateExit() {}

    public virtual void SetStateMachine(StateMachine a_stateMachine) 
    {
        stateMachine = a_stateMachine;
    }

    public virtual void SetNPC(NPC a_npc) 
    {
        npc = a_npc;
    }
}