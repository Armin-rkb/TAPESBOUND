using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPlayerInRadius : NPCMovementBase
{
    [Header("Radius within to spot the player")]
    [SerializeField]
    private float radius = 3f;

    [Header("Distance from target to stop")]
    [SerializeField]
    private float offset = 0.2f;

    public Collider2D col = null;

    private const int playerLayerMask = 9;

    void Update()
    {
        CheckForPlayer();

        if (col != null)
        {
            GetTargetPos();
            Move();
        }
    }

    public override void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, movementSpeed * Time.deltaTime);
    }

    private void CheckForPlayer()
    {
        col = Physics2D.OverlapCircle(transform.position, radius, ~playerLayerMask);
    }

    private void GetTargetPos()
    {
        Vector2 direction = (transform.position - col.transform.position).normalized;
        targetPos = (Vector2)col.transform.position + direction * offset;
    }
}
