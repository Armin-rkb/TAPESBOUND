using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveToPlayerState", menuName = "FSM/States/MoveToPlayer", order = 3)]
public class MoveToPlayerState : BaseState, IMove
{
    [Header("Patrol Speed")]
    public float movementSpeed = 2;

    [Header("Radius within to spot the player")]
    [SerializeField]
    private float radius = 2f;

    [Header("Distance from target to stop")]
    [SerializeField]
    private float offset = 0.65f;

    private Collider2D col = null;
    private LayerMask playerLayerMask;

    public Vector2 targetPos { get; set; }

    private void OnEnable()
    {
        col = null;
        playerLayerMask = LayerMask.GetMask("Player");
    }

    public override void OnStateTick()
    {
        if (CheckForPlayer())
        {
            GetTargetPos();
            Move();
        }
    }

    public void Move()
    {
        npc.transform.position = Vector2.MoveTowards(npc.transform.position, targetPos, movementSpeed * Time.deltaTime);
    }

    private bool CheckForPlayer()
    {
        col = Physics2D.OverlapCircle(npc.transform.position, radius, playerLayerMask);
        return col != null;
    }

    private void GetTargetPos()
    {
        Vector2 direction = (npc.transform.position - col.transform.position).normalized;
        targetPos = (Vector2)col.transform.position + direction * offset;
    }
}
