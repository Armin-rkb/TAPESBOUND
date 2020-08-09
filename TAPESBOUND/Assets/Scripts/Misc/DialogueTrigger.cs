using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : Interactable
{
    [SerializeField] 
    private DialogueBase dialogue = null;
    private bool isTriggered = false;

    public override void Interact()
    {
        base.Interact();
        
        if (isTriggered)
        {
            DialogueManager.instance.DequeueDialogue();
        }
        else
        {
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
