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

    private Vector2 movementDirection;

    private Coroutine bufferTimer;

    public override void OnStateEnter()
    {
        base.OnStateEnter();

        if (startPos == Vector2.zero)
        {
            startPos = npc.transform.position;
        }

        isWaiting = false;
        isWalking = false;

        RandomPositionInRadius();
    }

    public override void OnStateTick()
    {
        if (!isWaiting)
        {
            Move();
            FlipAnimation();
            Animate();
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        isWalking = false;
        isWaiting = false;
        npc.animator.SetBool("IsWalking", isWalking);
        npc.StopCoroutine(bufferTimer);
    }

    public void Move()
    {
        isWalking = true;
        npc.transform.position = Vector2.MoveTowards(npc.transform.position, targetPos, movementSpeed * Time.deltaTime);

        if (Vector2.Distance(targetPos, npc.transform.position) < 0.05f
            && !isWaiting)
        {
            bufferTimer = npc.StartCoroutine(BufferTimer());
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
    }

    private void RandomPositionInRadius()
    {
        targetPos = startPos + (minDistance * Vector2.one + UnityEngine.Random.insideUnitCircle * maxDistance);
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
    }

    private void Animate()
    {
        npc.animator.SetFloat("Horizontal", movementDirection.x);
        npc.animator.SetFloat("Vertical", movementDirection.y);
        npc.animator.SetBool("IsWalking", isWalking);
    }
}
