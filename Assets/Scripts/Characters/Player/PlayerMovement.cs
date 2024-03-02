using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerController playerController = null;

    [Header("Stats:")]
    [SerializeField] private float movementSpeed = 0;
    private bool isWalking;
    private Vector2 lastDirection;

    public delegate void OnPlayerMove();
    public static event OnPlayerMove onPlayerMove;
    public delegate void OnPlayerStop();
    public static event OnPlayerStop onPlayerStop;

    void Update()
    {
        FlipAnimation();
        Animate();
        GetInput();
    }

    void FixedUpdate()
    {
        if (isWalking)
        {
            Move();
        }
        else
        {
            StopMove();
        }
    }

    private void GetInput()
    {
        isWalking = false;

        if (playerController.playerInput.movementDirection.x != 0 || 
            playerController.playerInput.movementDirection.y != 0)
        {
            isWalking = true;
            lastDirection = playerController.playerInput.movementDirection;
        }
    }

    private void Move()
    {
        playerController.rb.velocity = playerController.playerInput.movementDirection * movementSpeed;
        onPlayerMove?.Invoke();
    }

    public void StopMove()
    {
        playerController.rb.velocity = new Vector2(0, 0);
        onPlayerStop?.Invoke();
    }

    private void FlipAnimation()
    {
        if (isWalking)
        {
            if (playerController.playerInput.movementDirection.x > 0)
            {
                playerController.spriteRenderer.flipX = true;
            }
            else
            {
                playerController.spriteRenderer.flipX = false;
            }
        }
        else
        {
            if (lastDirection.x > 0)
            {
                playerController.spriteRenderer.flipX = true;
            }
            else
            {
                playerController.spriteRenderer.flipX = false;
            }
        }
    }

    private void Animate()
    {
        playerController.animator.SetFloat("Horizontal", playerController.playerInput.movementDirection.x);
        playerController.animator.SetFloat("Vertical", playerController.playerInput.movementDirection.y);
        playerController.animator.SetFloat("LastHorizontal", lastDirection.x);
        playerController.animator.SetFloat("LastVertical", lastDirection.y);
        playerController.animator.SetBool("IsWalking", isWalking);
    }
}
