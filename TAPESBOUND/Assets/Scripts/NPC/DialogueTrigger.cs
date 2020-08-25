using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NPC))]
public class DialogueTrigger : Interactable
{
    [SerializeField] 
    private DialogueBase dialogue = null;
    [SerializeField]
    private NPC npc = null;
    private bool isTriggered = false;

    void Awake()
    {
        if (npc == null)
        {
            npc = GetComponent<NPC>();
        }
    }

    public override void Interact()
    {
        base.Interact();
        
        if (isTriggered)
        {
            DialogueManager.instance.DequeueDialogue();
        }
        else
        {
            npc?.LookAtPlayer((Vector2)playerRef.transform.position);
            DialogueManager.instance.OpenDialogue();
            DialogueManager.instance.EnqueueDialogue(dialogue);
            isTriggered = true;
        }
    }

    private void ResetDialogue()
    {
        isTriggered = false;
    }

    private void OnEnable()
    {
        DialogueManager.onDialogueExit += ResetDialogue;
    }

    private void OnDisable()
    {
        DialogueManager.onDialogueExit -= ResetDialogue;
    }
}
