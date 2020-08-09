using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [SerializeField]
    private PlayerMovement playerMovement = null;

    public bool inDialogue = false;
    public iInteractable currentInteractable = null;

    void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        if (currentInteractable != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentInteractable.Interact();
            }
        }
    }

    private void DialogueEnter()
    {
        inDialogue = true;

        if (playerMovement.enabled)
        {
            playerMovement.StopMove();
            playerMovement.enabled = false;
            playerMovement.StopAnimation();
        }
    }
    
    private void DialogueExit()
    {
        inDialogue = false;

        if (!playerMovement.enabled)
        {
            playerMovement.StartAnimation();
            playerMovement.enabled = true;
        }
    }

    private void OnEnable()
    {
        DialogueManager.onDialogueEnter += DialogueEnter;
        DialogueManager.onDialogueExit += DialogueExit;
    }

    private void OnDisable()
    {
        DialogueManager.onDialogueEnter -= DialogueEnter;
        DialogueManager.onDialogueExit -= DialogueExit;
    }
}
