using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class DialogueBase : ScriptableObject
{
    [System.Serializable]
    public class Info
    {
        [TextArea(4, 8)]
        public string text;
    }

    [Header("Add dialogue text.")]
    public Info[] dialogueInfo;
}
