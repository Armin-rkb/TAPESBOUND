using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovementBase : MonoBehaviour, IMove
{
    [Header("Speed")]
    [SerializeField]
    protected float movementSpeed = 2f;

    protected Vector2 targetPos;

    public virtual void Move() {}
}
