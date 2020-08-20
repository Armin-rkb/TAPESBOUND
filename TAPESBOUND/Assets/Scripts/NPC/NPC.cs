using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class NPC : MonoBehaviour
{
    private StateMachine stateMachine;

    // Start is called before the first frame update
    void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
