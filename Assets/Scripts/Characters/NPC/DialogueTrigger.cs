using UnityEngine;

[RequireComponent(typeof(NPC))]
public class DialogueTrigger : Interactable
{
    public bool lookAtPlayer = true;

    [SerializeField] 
    private DialogueBase[] dialogue = null;
    private int dialogueIndex = 0;

    [SerializeField]
    private NPC npc = null;

    private void Awake()
    {
        if (npc == null)
        {
            npc = GetComponent<NPC>();
        }
    }

    public override void Interact()
    {
        base.Interact();
        if (dialogue[0] == null)
        {
            Debug.LogWarning("NPC Doesn't have a Dialogue attached!");
            return;
        }
        
        if (lookAtPlayer)
        {
            npc?.LookAtPlayer(playerRef.transform.position);
        }

        // Open & load in the dialogue.
        DialogueManager.instance.OpenDialogue(npc?.GetId());
        DialogueManager.instance.EnqueueDialogue(dialogue[dialogueIndex]);

        CountNextDialogue();
    }

    private void CountNextDialogue()
    {
        dialogueIndex++;

        if (dialogueIndex >= dialogue.Length)
        {
            dialogueIndex = 0;
        }
    }
}
