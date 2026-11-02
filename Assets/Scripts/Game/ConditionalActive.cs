using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows this object only while the conditions are met. E.g. hide an NPC once they joined the party
/// (InParty + Not), or show someone after a quest is done.
/// Checked when the scene loads and whenever the game state changes, but never in the middle of a conversation.
/// Leave the object active in the scene, it hides itself.
/// </summary>
public class ConditionalActive : MonoBehaviour
{
    [Tooltip("All conditions must be true to show this object.")]
    [SerializeField] private List<DialogueCondition> conditions = new List<DialogueCondition>();

    // Subscribed in Awake instead of OnEnable, a hidden object still has to hear when to show again.
    private void Awake()
    {
        DialogueManager.onDialogueExit += Refresh;
        PartyManager.onPartyChanged += HandlePartyChanged;
        GameFlags.onFlagChanged += HandleFlagChanged;
        QuestManager.onQuestStarted += HandleQuestChanged;
        QuestManager.onQuestUpdated += HandleQuestChanged;
        QuestManager.onQuestCompleted += HandleQuestChanged;

        Refresh();
    }

    private void OnDestroy()
    {
        DialogueManager.onDialogueExit -= Refresh;
        PartyManager.onPartyChanged -= HandlePartyChanged;
        GameFlags.onFlagChanged -= HandleFlagChanged;
        QuestManager.onQuestStarted -= HandleQuestChanged;
        QuestManager.onQuestUpdated -= HandleQuestChanged;
        QuestManager.onQuestCompleted -= HandleQuestChanged;
    }

    public void Refresh()
    {
        // Don't let someone vanish while you talk to them, this runs again when the conversation ends.
        if (this == null || (DialogueManager.instance != null && DialogueManager.instance.IsOpen))
        {
            return;
        }

        gameObject.SetActive(DialogueCondition.AllMet(conditions));
    }

    private void HandlePartyChanged(CharacterProfile a_character, bool a_hasJoined)
    {
        Refresh();
    }

    private void HandleFlagChanged(string a_flag, bool a_isSet)
    {
        Refresh();
    }

    private void HandleQuestChanged(QuestProgress a_progress)
    {
        Refresh();
    }
}
