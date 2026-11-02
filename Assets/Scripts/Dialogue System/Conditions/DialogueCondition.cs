using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueConditionType
{
    QuestState = 0,
    Flag = 1,
    HasItem = 2,
    HasMoney = 3,
    InParty = 4
}

/// <summary>
/// A check against the game state, used to pick which dialogue an NPC says.
/// </summary>
[Serializable]
public class DialogueCondition
{
    public DialogueConditionType type = DialogueConditionType.QuestState;
    public Quest quest = null;
    public QuestState questState = QuestState.NotStarted;
    public string flag = "";
    public Item item = null;
    public int amount = 1;
    public CharacterProfile character = null;
    [Tooltip("Flip the result, e.g. 'flag is NOT set' or 'does NOT have the item'.")]
    public bool not = false;

    public bool IsMet()
    {
        bool result = Evaluate();
        return not ? !result : result;
    }

    private bool Evaluate()
    {
        PlayerInventory inventory = PlayerInventory.Current;

        switch (type)
        {
            case DialogueConditionType.QuestState:
                return quest != null && QuestManager.GetState(quest) == questState;
            case DialogueConditionType.Flag:
                return GameFlags.IsSet(flag);
            case DialogueConditionType.HasItem:
                return inventory != null && inventory.HasItem(item, Mathf.Max(1, amount));
            case DialogueConditionType.HasMoney:
                return inventory != null && inventory.Money >= amount;
            case DialogueConditionType.InParty:
                return PartyManager.IsInParty(character);
        }
        return false;
    }

    /// <summary>
    /// True when every condition is met (or there are none).
    /// </summary>
    public static bool AllMet(IList<DialogueCondition> a_conditions)
    {
        if (a_conditions == null)
        {
            return true;
        }

        foreach (DialogueCondition condition in a_conditions)
        {
            if (condition != null && !condition.IsMet())
            {
                return false;
            }
        }
        return true;
    }
}
