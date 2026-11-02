using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Talk to an NPC (or check a sign, present, trash can...).
/// The first conditional dialogue whose conditions are met is used, otherwise the default dialogue.
/// </summary>
public class DialogueTrigger : Interactable
{
    [System.Serializable]
    public class ConditionalDialogue
    {
        [Tooltip("Only a note for yourself, e.g. \"Quest active\".")]
        public string note = "";
        [Tooltip("All conditions must be true.")]
        public List<DialogueCondition> conditions = new List<DialogueCondition>();
        [Tooltip("One per conversation, cycles through them.")]
        public DialogueBase[] dialogue = null;

        [System.NonSerialized] public int dialogueIndex = 0;
    }

    public bool lookAtPlayer = true;

    [Header("Default dialogue, cycles each time you talk.")]
    [SerializeField]
    private DialogueBase[] dialogue = null;
    private int dialogueIndex = 0;

    [SerializeField]
    private NPC npc = null;

    [Header("Checked top to bottom before the default dialogue.")]
    [SerializeField]
    private List<ConditionalDialogue> conditionalDialogue = new List<ConditionalDialogue>();

    private ObjectId objectId = null;

    private void Awake()
    {
        if (npc == null)
        {
            npc = GetComponent<NPC>();
        }
        objectId = GetComponent<ObjectId>();
    }

    public override void Interact()
    {
        base.Interact();

        DialogueBase nextDialogue = GetNextDialogue();
        if (nextDialogue == null)
        {
            Debug.LogWarning(name + " doesn't have a Dialogue for the current situation!", this);
            return;
        }

        if (lookAtPlayer && npc != null && playerRef != null)
        {
            npc.LookAtPlayer(playerRef.transform.position);
        }

        // Open & load in the dialogue.
        DialogueManager.instance.OpenDialogue(GetSpeakerId());
        DialogueManager.instance.EnqueueDialogue(nextDialogue);
    }

    /// <summary>
    /// The dialogue the next conversation will use.
    /// </summary>
    public DialogueBase GetNextDialogue()
    {
        foreach (ConditionalDialogue entry in conditionalDialogue)
        {
            if (entry.dialogue != null && entry.dialogue.Length > 0 && DialogueCondition.AllMet(entry.conditions))
            {
                return CountNextDialogue(entry.dialogue, ref entry.dialogueIndex);
            }
        }

        if (dialogue == null || dialogue.Length == 0)
        {
            return null;
        }
        return CountNextDialogue(dialogue, ref dialogueIndex);
    }

    private string GetSpeakerId()
    {
        if (npc != null)
        {
            return npc.GetId();
        }
        return objectId != null ? objectId.Id : null;
    }

    private static DialogueBase CountNextDialogue(DialogueBase[] a_dialogue, ref int a_index)
    {
        if (a_index >= a_dialogue.Length)
        {
            a_index = 0;
        }

        DialogueBase result = a_dialogue[a_index];
        a_index = (a_index + 1) % a_dialogue.Length;
        return result;
    }
}
