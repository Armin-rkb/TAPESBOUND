using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [Tooltip("First state of the State Machine (None will result in the first one of available states.)")]
    [SerializeField]
    private BaseState startState = null;
    public BaseState currentState { get; private set; } = null;

    [SerializeField]
    List<BaseState> availableStates = null;
    Dictionary<Type, BaseState> fsmStates = null;

    private void Awake()
    {
        fsmStates = new Dictionary<Type, BaseState>();

        NPC npc = GetComponent<NPC>();
        Animator anim = GetComponent<Animator>();
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        foreach (BaseState state in availableStates)
        {
            if (state != null)
            {
                state.SetStateMachine(this);
                state.SetNPC(npc);
                state.SetAnimator(anim);
                state.SetSpriteRenderer(spriteRenderer);
                fsmStates.Add(state.GetType(), state);
            }
            else
            {
                Debug.LogError(npc.gameObject.name + " has an invalid state machine state.");
            }
        }
    }

    public void Start()
    {
        if (startState != null)
        {
            currentState = startState;
            currentState.OnStateEnter();
        } 
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.OnStateTick();
        }
    }

    public void EnterState(BaseState a_nextState)
    {
        if (a_nextState == null)
        {
            return;
        }
        a_nextState.GetType();

        currentState.OnStateExit();
        currentState = a_nextState;
        currentState.OnStateEnter();
    }
    
    public void EnterState(Type a_nextState)
    {
        if (a_nextState == null)
        {
            return;
        }

        currentState.OnStateExit();
        currentState = fsmStates[a_nextState];
        currentState.OnStateEnter();
    }
}