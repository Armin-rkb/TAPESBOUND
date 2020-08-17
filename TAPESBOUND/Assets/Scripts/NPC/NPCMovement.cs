using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [Header("Distance the NPC will travel")]
    [SerializeField]
    private float minDistance = 1f;
    [SerializeField]
    private float maxDistance = 5f;

    [Header("Speed")]
    [SerializeField]
    private float movementSpeed = 5f;
    [Header("Stop Time")]
    [SerializeField]
    private float waitTime = 1f;

    private Vector2 targetPos;
    private Vector2 startPos;

    private bool isWaiting = false;

    void Start()
    {
        startPos = transform.position;

        RandomPositionInRadius();
    }

    void Update()
    {
        float step = movementSpeed * Time.deltaTime; // calculate distance to move
        transform.position = Vector2.MoveTowards(transform.position, targetPos, step);

        if (Vector2.Distance(targetPos, transform.position) < 0.05f
            && !isWaiting)
        {
            StartCoroutine(BufferTimer());
        }
    }

    // Wait and get a new position.
    private IEnumerator BufferTimer()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        RandomPositionInRadius();
        isWaiting = false;
    }

    private void RandomPositionInRadius()
    {
        targetPos = startPos + ((minDistance * Vector2.one) + (Random.insideUnitCircle * maxDistance));
    }
}
