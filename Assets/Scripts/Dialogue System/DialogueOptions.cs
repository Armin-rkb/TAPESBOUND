using System;
using UnityEngine.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Options", menuName = "Dialogue/Dialogue Options", order = 3)]
public class DialogueOptions : DialogueEvent
{
    [System.Serializable]
    public class Options
    {
        public string buttonName = "";
        public DialogueBase nextDialogue;
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
