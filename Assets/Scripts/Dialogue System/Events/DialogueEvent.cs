using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Dialogue with Event", menuName = "Dialogue/Dialogue Event", order = 2)]
public class DialogueEvent : DialogueBase
{
    [System.Serializable]
    public class Event
    {
        public int eventLine;
        public UnityEvent myEvent;
    }

    public Event[] dialogueEvents;

    private void OnValidate()
    {
        foreach (Event a_event in dialogueEvents)
        {
            if (a_event.eventLine > dialogueInfo.Length)
            {
                Debug.LogWarning("Even line doesn't match!");
                a_event.eventLine = dialogueInfo.Length;
            }
        }
    }
}