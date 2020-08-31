using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Text", menuName = "Dialogue/Dialogue Text", order = 1)]
public class Dialogue : ScriptableObject
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