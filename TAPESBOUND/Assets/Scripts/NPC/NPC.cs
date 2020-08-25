using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class NPC : MonoBehaviour
{
    public Animator animator = null;
    public SpriteRenderer spriteRenderer = null;
    private StateMachine stateMachine;

    public void LookAtPlayer(Vector2 a_playerPos)
    {
        Vector2 playerDirection = (a_playerPos - (Vector2)transform.position).normalized;
        animator.SetFloat("Horizontal", playerDirection.x);
        animator.SetFloat("Vertical", playerDirection.y);

        if (playerDirection.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}
