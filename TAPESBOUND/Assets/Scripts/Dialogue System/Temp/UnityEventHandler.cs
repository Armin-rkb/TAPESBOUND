using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UnityEventHandler : MonoBehaviour, IPointerDownHandler
{
    public UnityEvent eventHandler;
    public DialogueBase nextDialogue;

    public void OnPointerDown(PointerEventData a_pointerEventData)
    {
        eventHandler?.Invoke();

        if (nextDialogue != null)
        {
            DialogueManager.instance.EnqueueDialogue(nextDialogue);
        }
        DialogueManager.instance.CloseOptions();
    }
}
