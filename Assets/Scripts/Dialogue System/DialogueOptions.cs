using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Options", menuName = "Dialogue/Dialogue Options", order = 3)]
public class DialogueOptions : DialogueEvent
{
    [System.Serializable]
    public class Options
    {
        public string buttonName = "";
        [Tooltip("Plays after picking this option. Empty ends the conversation.")]
        public DialogueBase nextDialogue;
        [Tooltip("Runs when this option is picked, before the next dialogue.")]
        public List<DialogueCommand> commands = new List<DialogueCommand>();
        [Header("Button OnClick Event.")]
        public UnityEvent buttonEvent;
    }

    private const int MAX_SIZE = 2;
    public Options[] optionsInfo = new Options[MAX_SIZE];

    private void OnValidate()
    {
        if (optionsInfo.Length > MAX_SIZE)
        {
            Debug.LogWarning("Maximum limit of options reached!");
            Array.Resize(ref optionsInfo, MAX_SIZE);
        }
    }
}
