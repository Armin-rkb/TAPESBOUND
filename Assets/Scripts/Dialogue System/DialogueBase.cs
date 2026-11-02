using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Text", menuName = "Dialogue/Dialogue Text", order = 1)]
public class DialogueBase : ScriptableObject
{
    [System.Serializable]
    public class Info
    {
        [TextArea(4, 8)]
        public string text;
        [Tooltip("Runs when this line appears: give items, start quests, set flags...")]
        public List<DialogueCommand> commands = new List<DialogueCommand>();
    }

    [Header("Add dialogue text. Tokens: {player} {money}")]
    public Info[] dialogueInfo;
}
