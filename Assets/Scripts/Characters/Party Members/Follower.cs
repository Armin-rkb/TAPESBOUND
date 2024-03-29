﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public CharacterProfile characterProfile;
    public GameObject leader; // the game object to follow - assign in inspector
    public int steps; // number of steps to stay behind - assign in inspector

    private Queue<Vector3> leaderMovement = new Queue<Vector3>();
    private Vector3 lastRecord;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isWalking = false;
    private Vector2 movementDirection;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = characterProfile.animatorController;
    }

    void Update()
    {
        // remove last position from the record and use it for our own
        if (leaderMovement.Count > steps)
        {
            Vector3 newPos = leaderMovement.Dequeue();
            movementDirection = (newPos - transform.position).normalized;
            transform.position = newPos;
        }

        FlipAnimation();
        Animate();
    }

    private void OnEnable()
    {
        PlayerMovement.onPlayerMove += RecordLeaderPos;
        PlayerMovement.onPlayerStop += StopWalking;
    }

    private void OnDisable()
    {
        PlayerMovement.onPlayerMove -= RecordLeaderPos;
        PlayerMovement.onPlayerStop -= StopWalking;
    }

    private void StopWalking()
    {
        isWalking = false;
    }

    private void RecordLeaderPos()
    {
        // record position of leader
        leaderMovement.Enqueue(leader.transform.position);
        isWalking = true;
    }

    private void FlipAnimation()
    {
        if (!isWalking) return;

        spriteRenderer.flipX = movementDirection.x > 0;
    }

    private void Animate()
    {
        animator.SetFloat("Horizontal", movementDirection.x);
        animator.SetFloat("Vertical", movementDirection.y);
        animator.SetBool("IsWalking", isWalking);
    }
}
