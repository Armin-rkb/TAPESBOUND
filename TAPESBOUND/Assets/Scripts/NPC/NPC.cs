using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class NPC : MonoBehaviour
{
    public Animator animator = null;
    public SpriteRenderer spriteRenderer = null;
    private StateMachine stateMachine;
}
