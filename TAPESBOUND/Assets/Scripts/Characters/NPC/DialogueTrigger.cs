using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NPC))]
public class DialogueTrigger : Interactable
{
    [SerializeField] 
    private DialogueBase[] dialogue = null;
    private int dialogueIndex = 0;

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
            isTriggered = true;
         
            npc?.LookAtPlayer((Vector2)playerRef.transform.position);
            DialogueManager.instance.OpenDialogue();
            DialogueManager.instance.EnqueueDialogue(dialogue[dialogueIndex]);

            CountNextDialogue();
        }
    }

    private void CountNextDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex >= dialogue.Length)
        {
            dialogueIndex = 0;
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
