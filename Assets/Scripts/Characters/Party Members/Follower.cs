using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A party member in the world. Walks in line behind the leader like in Earthbound, or is played by
/// someone else with their own controller (see LocalCoop).
/// The PartyManager creates these, or place one in the scene to use it for that character (it stays hidden
/// until the character is in the party).
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class Follower : MonoBehaviour
{
    public CharacterProfile characterProfile;

    [Tooltip("Speed when walking back into line after the other player stops playing.")]
    [SerializeField] private float catchUpSpeed = 5f;

    // What the co-op player can talk to or check right now.
    [HideInInspector] public IInteractable currentInteractable = null;

    private SpriteRenderer spriteRenderer = null;
    private Animator animator = null;
    private Rigidbody2D rb = null;
    private BoxCollider2D bodyCollider = null;

    private readonly HashSet<string> animatorParameters = new HashSet<string>();
    private bool hasCachedParameters = false;

    private CoopPlayer controller = null;
    private bool isReturning = false;
    private bool isWalking = false;
    private Vector2 moveInput;
    private Vector2 facing = Vector2.down;

    private const string HorizontalParam = "Horizontal";
    private const string VerticalParam = "Vertical";
    private const string LastHorizontalParam = "LastHorizontal";
    private const string LastVerticalParam = "LastVertical";
    private const string IsWalkingParam = "IsWalking";

    public CoopPlayer Controller => controller;
    public bool IsControlled => controller != null;
    // On the way back into line after a player stopped playing.
    public bool IsReturning => isReturning;
    public Collider2D BodyCollider => bodyCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        SetCharacter(characterProfile);
    }

    public void SetCharacter(CharacterProfile a_character)
    {
        characterProfile = a_character;
        if (characterProfile != null && characterProfile.animatorController != null)
        {
            animator.runtimeAnimatorController = characterProfile.animatorController;
        }
        hasCachedParameters = false;
    }

    /// <summary>
    /// Let a co-op player play this character, or null to put it back in line.
    /// a_shape: collider to copy the size of, so it fits through the same gaps as the leader.
    /// </summary>
    public void SetController(CoopPlayer a_controller, BoxCollider2D a_shape = null)
    {
        if (controller == a_controller)
        {
            return;
        }

        bool wasControlled = controller != null;
        controller = a_controller;
        currentInteractable = null;
        moveInput = Vector2.zero;

        if (controller != null)
        {
            EnsureBody(a_shape);
            rb.simulated = true;
            isReturning = false;
        }
        else
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
            isReturning = wasControlled;
        }
    }

    /// <summary>
    /// Walk to a spot in the line. Called by the PartyManager every frame while nobody plays this character.
    /// </summary>
    public void FollowTo(Vector2 a_position, float a_deltaTime)
    {
        if (controller != null)
        {
            return;
        }

        Vector2 current = transform.position;
        Vector2 next = a_position;
        if (isReturning)
        {
            next = Vector2.MoveTowards(current, a_position, catchUpSpeed * a_deltaTime);
            isReturning = next != a_position;
        }

        Vector2 delta = next - current;
        isWalking = delta.sqrMagnitude > 0.000001f;
        if (isWalking)
        {
            facing = delta.normalized;
        }

        SetPosition(next);
        UpdateAnimation(isWalking ? facing : Vector2.zero);
    }

    /// <summary>
    /// Put the character somewhere right away.
    /// </summary>
    public void Warp(Vector2 a_position)
    {
        isReturning = false;
        SetPosition(a_position);
        if (rb != null)
        {
            rb.position = a_position;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        // While following, the PartyManager moves this character.
        if (controller == null)
        {
            return;
        }

        ControlState controls = controller.ReadControls();
        bool canMove = PartyManager.CanPlayersMove;
        moveInput = canMove ? controls.move : Vector2.zero;

        if (controls.interact)
        {
            if (canMove && currentInteractable != null)
            {
                Interactable.InteractAs(currentInteractable, gameObject);
            }
            else if (DialogueManager.instance != null && DialogueManager.instance.IsOpen)
            {
                // Everybody can read on.
                DialogueManager.instance.DequeueDialogue();
            }
        }

        isWalking = moveInput != Vector2.zero;
        if (isWalking)
        {
            facing = moveInput;
        }
        UpdateAnimation(moveInput);
    }

    private void FixedUpdate()
    {
        if (controller == null || rb == null)
        {
            return;
        }

        Vector2 velocity = moveInput * PartyManager.MovementSpeed;
        rb.linearVelocity = PartyManager.KeepOnScreen(transform, rb.position, velocity, Time.fixedDeltaTime);
    }

    private void EnsureBody(BoxCollider2D a_shape)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
            }
        }

        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<BoxCollider2D>();
            if (bodyCollider == null)
            {
                bodyCollider = gameObject.AddComponent<BoxCollider2D>();
                bodyCollider.size = a_shape != null ? a_shape.size : new Vector2(0.5f, 0.24f);
                bodyCollider.offset = a_shape != null ? a_shape.offset : new Vector2(0f, -0.24f);
            }
        }
    }

    private void SetPosition(Vector2 a_position)
    {
        transform.position = new Vector3(a_position.x, a_position.y, transform.position.z);
    }

    private void UpdateAnimation(Vector2 a_movement)
    {
        // The sprites look left, like the player.
        spriteRenderer.flipX = facing.x > 0f;

        if (animator == null || !animator.isInitialized || animator.runtimeAnimatorController == null)
        {
            return;
        }

        CacheParameters();

        // Without "LastHorizontal" the walk blend tree also decides which way the character stands.
        Vector2 direction = animatorParameters.Contains(LastHorizontalParam) ? a_movement : facing;
        SetFloat(HorizontalParam, direction.x);
        SetFloat(VerticalParam, direction.y);
        SetFloat(LastHorizontalParam, facing.x);
        SetFloat(LastVerticalParam, facing.y);
        if (animatorParameters.Contains(IsWalkingParam))
        {
            animator.SetBool(IsWalkingParam, isWalking);
        }
    }

    // Characters have different animators, only set the parameters they have.
    private void CacheParameters()
    {
        if (hasCachedParameters)
        {
            return;
        }

        animatorParameters.Clear();
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            animatorParameters.Add(parameter.name);
        }
        hasCachedParameters = true;
    }

    private void SetFloat(string a_parameter, float a_value)
    {
        if (animatorParameters.Contains(a_parameter))
        {
            animator.SetFloat(a_parameter, a_value);
        }
    }
}
