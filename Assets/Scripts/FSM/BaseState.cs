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

    public void SetStateMachine(StateMachine a_stateMachine) 
    {
        stateMachine = a_stateMachine;
    }

    public void SetNPC(NPC a_npc) 
    {
        npc = a_npc;
    }

    public void SetAnimator(Animator a_anim)
    {
        npc.animator = a_anim;
    }

    public void SetSpriteRenderer(SpriteRenderer a_spriteRenderer)
    {
        npc.spriteRenderer = a_spriteRenderer;
    }
}