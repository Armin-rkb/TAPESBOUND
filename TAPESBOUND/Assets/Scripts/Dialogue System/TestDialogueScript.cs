using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueScript : MonoBehaviour
{
    public DialogueBase dialogue;
    bool isTriggered = false;

    public void TriggerDialogue()
    {
        DialogueManager.instance.EnqueueDialogue(dialogue);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTriggered)
            {
                DialogueManager.instance.DequeueDialogue();
            }
            else
            {
               TriggerDialogue();
               isTriggered = true;
            }
        }
    }
}
