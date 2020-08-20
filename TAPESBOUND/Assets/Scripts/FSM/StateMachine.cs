using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    [SerializeField]
    private BaseState startState = null;
    private BaseState currentState = null;

    [SerializeField]
    List<BaseState> availableStates = null;
    Dictionary<Type, BaseState> fsmStates = null;

    private void Awake()
    {
        fsmStates = new Dictionary<Type, BaseState>();

        NPC npc = GetComponent<NPC>();

        foreach (BaseState state in availableStates)
        {
            state.SetStateMachine(this);
            state.SetNPC(npc);
            fsmStates.Add(state.GetType(), state);
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

        //currentState.OnStateExit();
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

/*
private Dictionary<Type, BaseState> availableStates = null;

public BaseState currentState = null;

// Update is called once per frame
void Update()
{
    if (currentState != null)
    {
        currentState.OnStateTick();
    }
}

void SetStates(Dictionary<Type, BaseState> a_states)
{
    availableStates = a_states;
}

public void SwitchToState(BaseState a_newState)
{
    currentState.OnStateExit();
    currentState = a_newState;
    currentState.OnStateEnter();
}
*/
