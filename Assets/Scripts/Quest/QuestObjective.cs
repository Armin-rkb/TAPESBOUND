using System;
using UnityEngine;

public enum QuestObjectiveType
{
    TalkTo = 0,
    CollectItem = 1,
    Flag = 2,
    Custom = 3
}

/// <summary>
/// One step of a quest.
/// TalkTo: talk to an NPC (by ObjectId). CollectItem: carry an item. Flag: a GameFlag is set.
/// Custom: advanced by an AdvanceQuest command with the same key.
/// </summary>
[Serializable]
public class QuestObjective
{
    [Tooltip("Shown in the quest log, e.g. \"Find the shop up north\". Empty generates one.")]
    public string description = "";
    public QuestObjectiveType type = QuestObjectiveType.TalkTo;
    [Tooltip("TalkTo: the NPC's ObjectId. Flag: the flag name. Custom: the key used by AdvanceQuest commands.")]
    public string target = "";
    public Item item = null;
    public int requiredAmount = 1;

    public int RequiredAmount => Mathf.Max(1, requiredAmount);

    // These objectives follow the game state instead of counting events.
    public bool IsStateBased => type == QuestObjectiveType.CollectItem || type == QuestObjectiveType.Flag;

    /// <summary>
    /// Current progress of a state based objective.
    /// </summary>
    public int EvaluateState()
    {
        switch (type)
        {
            case QuestObjectiveType.CollectItem:
                PlayerInventory inventory = PlayerInventory.Current;
                return inventory != null ? Mathf.Min(inventory.CountItem(item), RequiredAmount) : 0;
            case QuestObjectiveType.Flag:
                return GameFlags.IsSet(target) ? RequiredAmount : 0;
        }
        return 0;
    }

    public string GetDescription()
    {
        if (!string.IsNullOrEmpty(description))
        {
            return description;
        }

        switch (type)
        {
            case QuestObjectiveType.TalkTo:
                return "Talk to someone";
            case QuestObjectiveType.CollectItem:
                return "Get the " + (item != null ? item.DisplayName : "item");
            default:
                return "???";
        }
    }
}
