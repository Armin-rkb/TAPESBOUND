using UnityEngine;

[RequireComponent(typeof(StateMachine))]
public class NPC : MonoBehaviour
{
    public Animator animator = null;
    public SpriteRenderer spriteRenderer = null;
    public StateMachine stateMachine;

    private BaseState previousState = null;

    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();
    }

    public void LookAtPlayer(Vector2 a_playerPos)
    {
        if (animator != null)
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

    private void EnterDialogue()
    {
        previousState = stateMachine.currentState;
        stateMachine.EnterState(typeof(DialogueState));
    }

    private void ExitDialogue()
    {
        stateMachine.EnterState(previousState);
        previousState = null;
    }

    private void OnEnable()
    {
        DialogueManager.onDialogueEnter += EnterDialogue;
        DialogueManager.onDialogueExit += ExitDialogue;
    }
    private void OnDisable()
    {
        DialogueManager.onDialogueEnter -= EnterDialogue;
        DialogueManager.onDialogueExit -= ExitDialogue;
    }
}
