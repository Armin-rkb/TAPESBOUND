using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Options", menuName = "Dialogue/Dialogue Options", order = 2)]
public class DialogueOptions : DialogueBase
{
    [System.Serializable]
    public class Options
    {
        public string buttonName = "";
        public DialogueBase nextDialogue;
        public UnityEvent myEvent;
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
