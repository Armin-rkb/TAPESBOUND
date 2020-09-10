using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerState : MonoBehaviour
{
    public static Vector3 GetPosition => GetInstance().transform.position;

    private static PlayerState GetInstance() 
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PlayerState>();
        }

        return instance;
    }

    private static PlayerState instance;

    public CharacterProfile playerCharacterProfile;

    [SerializeField]
    private PlayerMovement playerMovement = null;

    public bool inDialogue = false;
    public IInteractable currentInteractable = null;

    private void Update()
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
