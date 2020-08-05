using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [Header("Stats:")]
    [SerializeField] private float movementSpeed = 0;
    private bool isWalking;
    private Vector2 lastDirection;
    private Vector2 movementDirection;

    [Header("References:")]
    [SerializeField] private SpriteRenderer spriteRenderer = null;
    [SerializeField] private Animator animator = null;
    [SerializeField] private Rigidbody2D rb = null;

    void Update()
    {
        FlipAnimation();
        Animate();
    }

    void FixedUpdate()
    {
        GetInput();
        Move();
    }

    private void GetInput()
    {
        movementDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        movementDirection.Normalize();
        isWalking = false;

        if (movementDirection.x != 0 || movementDirection.y != 0)
        {
            isWalking = true;
            lastDirection = movementDirection;
        }
    }

    private void Move()
    {
        rb.velocity = movementDirection * movementSpeed;
    }

    private void FlipAnimation()
    {
        if (isWalking)
        {
            if (movementDirection.x > 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            if (lastDirection.x > 0)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
    }

    private void Animate()
    {
        animator.SetFloat("Horizontal", movementDirection.x);
        animator.SetFloat("Vertical", movementDirection.y);
        animator.SetFloat("LastHorizontal", lastDirection.x);
        animator.SetFloat("LastVertical", lastDirection.y);
        animator.SetBool("IsWalking", isWalking);
    }
}
