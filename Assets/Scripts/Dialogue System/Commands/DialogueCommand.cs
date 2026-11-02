using System;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueCommandType
{
    None = 0,
    GiveItem = 1,
    TakeItem = 2,
    GiveMoney = 3,
    TakeMoney = 4,
    StartQuest = 5,
    AdvanceQuest = 6,
    CompleteQuest = 7,
    SetFlag = 8,
    ClearFlag = 9,
    PlayDialogue = 10,
    PlaySound = 11
}

/// <summary>
/// A small action that runs from a dialogue line, a dialogue option, a quest or an event zone.
/// Pick a type in the inspector, only the fields that type uses are shown.
/// </summary>
[Serializable]
public class DialogueCommand
{
    public DialogueCommandType type = DialogueCommandType.None;
    public Item item = null;
    public Quest quest = null;
    public DialogueBase dialogue = null;
    public AudioClip sound = null;
    [Tooltip("SetFlag/ClearFlag: the flag name. AdvanceQuest: the objective key (empty = current objective).")]
    public string key = "";
    public int amount = 1;
    [Tooltip("Don't print the message in the text box (\"You got the Liquor!\").")]
    public bool silent = false;

    /// <summary>
    /// Run every command in order. A broken command is logged and skipped, so it can never lock up a conversation.
    /// </summary>
    public static void ExecuteAll(IList<DialogueCommand> a_commands)
    {
        if (a_commands == null)
        {
            return;
        }

        foreach (DialogueCommand command in a_commands)
        {
            try
            {
                command?.Execute();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    public void Execute()
    {
        int count = Mathf.Max(1, amount);

        switch (type)
        {
            case DialogueCommandType.GiveItem:
                GiveItem(item, count, silent);
                break;
            case DialogueCommandType.TakeItem:
                TakeItem(item, count, silent);
                break;
            case DialogueCommandType.GiveMoney:
                GiveMoney(amount, silent);
                break;
            case DialogueCommandType.TakeMoney:
                TakeMoney(amount, silent);
                break;
            case DialogueCommandType.StartQuest:
                QuestManager.StartQuest(quest, !silent);
                break;
            case DialogueCommandType.AdvanceQuest:
                QuestManager.AdvanceObjective(quest, key, count);
                break;
            case DialogueCommandType.CompleteQuest:
                QuestManager.CompleteQuest(quest, !silent);
                break;
            case DialogueCommandType.SetFlag:
                GameFlags.Set(key);
                break;
            case DialogueCommandType.ClearFlag:
                GameFlags.Clear(key);
                break;
            case DialogueCommandType.PlayDialogue:
                if (dialogue != null && DialogueManager.instance != null)
                {
                    DialogueManager.instance.EnqueueDialogue(dialogue);
                }
                break;
            case DialogueCommandType.PlaySound:
                DialogueManager.PlaySound(sound);
                break;
        }
    }

    public static void GiveItem(Item a_item, int a_count, bool a_silent)
    {
        PlayerInventory inventory = PlayerInventory.Current;
        if (a_item == null || inventory == null)
        {
            Debug.LogWarning("GiveItem: missing item or no PlayerInventory in the scene.");
            return;
        }

        for (int i = 0; i < a_count; i++)
        {
            bool added = inventory.TryAddItem(a_item);
            if (!a_silent)
            {
                SystemMessages messages = DialogueManager.Messages;
                DialogueManager.ShowSystemMessage(SystemMessages.Format(added ? messages.itemReceived : messages.inventoryFull, a_item));
            }

            if (!added)
            {
                break;
            }
        }
    }

    public static void TakeItem(Item a_item, int a_count, bool a_silent)
    {
        PlayerInventory inventory = PlayerInventory.Current;
        if (a_item == null || inventory == null)
        {
            return;
        }

        int removed = inventory.RemoveFromInventory(a_item, a_count);
        if (removed > 0 && !a_silent)
        {
            DialogueManager.ShowSystemMessage(SystemMessages.Format(DialogueManager.Messages.itemGiven, a_item, removed));
        }
    }

    public static void GiveMoney(int a_amount, bool a_silent)
    {
        PlayerInventory inventory = PlayerInventory.Current;
        if (a_amount <= 0 || inventory == null)
        {
            return;
        }

        inventory.AddMoney(a_amount);
        if (!a_silent)
        {
            DialogueManager.ShowSystemMessage(SystemMessages.Format(DialogueManager.Messages.moneyReceived, null, a_amount));
        }
    }

    public static void TakeMoney(int a_amount, bool a_silent)
    {
        PlayerInventory inventory = PlayerInventory.Current;
        if (a_amount <= 0 || inventory == null)
        {
            return;
        }

        if (inventory.SpendMoney(a_amount) && !a_silent)
        {
            DialogueManager.ShowSystemMessage(SystemMessages.Format(DialogueManager.Messages.moneyPaid, null, a_amount));
        }
    }
}
