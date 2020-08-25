using System.Collections;
using UnityEngine;

[CreateAssetMenu (fileName = "PatrolState", menuName = "FSM/States/PatrolRandomPos", order = 2)]
public class PatrolRandomPosState : BaseState, IMove
{
    [Header("Patrol Speed")] 
    public float movementSpeed = 2;

    [Header("Distance the NPC will travel")]
    [SerializeField]
    private float minDistance = 1f;
    [SerializeField]
    private float maxDistance = 3f;

    [Header("Stop Time")]
    [SerializeField]
    private float waitTime = 1f;
    private bool isWaiting = false;
    private bool isWalking = false;

    private Vector2 startPos;
    private Vector2 targetPos;

    private Vector2 lastDirection;
    private Vector2 movementDirection;

    private void OnEnable()
    {
        startPos = npc.transform.position;
    }

    public override void OnStateEnter()
    {
        base.OnStateEnter();
        isWaiting = false;
        isWalking = false;

        RandomPositionInRadius();
    }

    public override void OnStateTick()
    {
        Move();
        FlipAnimation();
        Animate();
    }

    public void Move()
    {
        npc.transform.position = Vector2.MoveTowards(npc.transform.position, targetPos, movementSpeed * Time.deltaTime);

        if (Vector2.Distance(targetPos, npc.transform.position) < 0.05f
            && !isWaiting)
        {
            npc.StartCoroutine(BufferTimer());
        }
    }

    // Wait and get a new position.
    private IEnumerator BufferTimer()
    {
        isWaiting = true;
        isWalking = false;
        yield return new WaitForSeconds(waitTime);
        RandomPositionInRadius();
        isWaiting = false;
        isWalking = true;
    }

    private void RandomPositionInRadius()
    {
        targetPos = startPos + (minDistance * Vector2.one + UnityEngine.Random.insideUnitCircle * maxDistance);
        lastDirection = movementDirection;
        movementDirection = (targetPos - (Vector2) npc.transform.position).normalized;
    }

    private void FlipAnimation()
    {
        if (isWalking)
        {
            if (movementDirection.x > 0)
            {
                npc.spriteRenderer.flipX = true;
            }
            else
            {
                npc.spriteRenderer.flipX = false;
            }
        }
        else
        {
            if (lastDirection.x > 0)
            {
                npc.spriteRenderer.flipX = true;
            }
            else
            {
                npc.spriteRenderer.flipX = false;
            }
        }
    }

    private void Animate()
    {
        npc.animator.SetFloat("Horizontal", movementDirection.x);
        npc.animator.SetFloat("Vertical", movementDirection.y);
        npc.animator.SetFloat("LastHorizontal", lastDirection.x);
        npc.animator.SetFloat("LastVertical", lastDirection.y);
        npc.animator.SetBool("IsWalking", isWalking);
    }
}
