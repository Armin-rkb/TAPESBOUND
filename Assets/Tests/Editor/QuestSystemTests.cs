using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class QuestSystemTests
{
    private readonly List<Object> createdObjects = new List<Object>();
    private PlayerInventory inventory;

    [SetUp]
    public void SetUp()
    {
        QuestManager.ResetAll();
        GameFlags.ClearAll();
        inventory = Track(new GameObject("Player")).AddComponent<PlayerInventory>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (Object createdObject in createdObjects)
        {
            Object.DestroyImmediate(createdObject);
        }
        createdObjects.Clear();
        QuestManager.ResetAll();
        GameFlags.ClearAll();
    }

    [Test]
    public void CollectQuest_WaitsForTurnIn_ThenGivesRewards()
    {
        Item liquor = CreateItem("Liquor");
        Item reward = CreateItem("Signed Shoe");
        Quest quest = CreateQuest(true, new QuestObjective { type = QuestObjectiveType.CollectItem, item = liquor });
        SetField(quest, "moneyReward", 20);
        SetField(quest, "itemRewards", new List<Item> { reward });

        Assert.AreEqual(QuestState.NotStarted, QuestManager.GetState(quest));
        Assert.IsTrue(QuestManager.StartQuest(quest, false));
        Assert.AreEqual(QuestState.Active, QuestManager.GetState(quest));

        inventory.TryAddItem(liquor);
        Assert.AreEqual(QuestState.ReadyToTurnIn, QuestManager.GetState(quest));

        QuestManager.CompleteQuest(quest);
        Assert.AreEqual(QuestState.Completed, QuestManager.GetState(quest));
        Assert.AreEqual(20, inventory.Money);
        Assert.IsTrue(inventory.HasItem(reward));
    }

    [Test]
    public void LosingTheItem_MovesQuestBackToActive()
    {
        Item liquor = CreateItem("Liquor");
        Quest quest = CreateQuest(true, new QuestObjective { type = QuestObjectiveType.CollectItem, item = liquor });

        inventory.TryAddItem(liquor);
        QuestManager.StartQuest(quest, false);
        Assert.AreEqual(QuestState.ReadyToTurnIn, QuestManager.GetState(quest), "Already carrying the item counts.");

        inventory.RemoveFromInventory(liquor);
        Assert.AreEqual(QuestState.Active, QuestManager.GetState(quest));
    }

    [Test]
    public void InOrderObjectives_OnlyProgressTheCurrentStep()
    {
        Quest quest = CreateQuest(false,
            new QuestObjective { type = QuestObjectiveType.TalkTo, target = "npc_a" },
            new QuestObjective { type = QuestObjectiveType.TalkTo, target = "npc_b" });
        QuestManager.StartQuest(quest, false);
        QuestProgress progress = QuestManager.GetProgress(quest);

        QuestManager.AdvanceObjective(quest, "npc_b");
        Assert.AreEqual(0, progress.GetProgress(1), "Step 2 can't be done before step 1.");
        Assert.IsFalse(progress.IsObjectiveVisible(1));

        QuestManager.AdvanceObjective(quest, "npc_a");
        Assert.IsTrue(progress.IsObjectiveVisible(1));

        QuestManager.AdvanceObjective(quest, "npc_b");
        Assert.AreEqual(QuestState.Completed, QuestManager.GetState(quest), "Completes by itself without turn in.");
    }

    [Test]
    public void FlagObjective_CompletesWhenFlagIsSet()
    {
        Quest quest = CreateQuest(false, new QuestObjective { type = QuestObjectiveType.Flag, target = "reached_mall" });
        QuestManager.StartQuest(quest, false);

        GameFlags.Set("reached_mall");
        Assert.AreEqual(QuestState.Completed, QuestManager.GetState(quest));
    }

    [Test]
    public void Commands_GiveItemsMoneyAndQuests()
    {
        Item liquor = CreateItem("Liquor");
        Quest quest = CreateQuest(true);

        DialogueCommand.ExecuteAll(new List<DialogueCommand>
        {
            new DialogueCommand { type = DialogueCommandType.GiveItem, item = liquor, amount = 2 },
            new DialogueCommand { type = DialogueCommandType.GiveMoney, amount = 15 },
            new DialogueCommand { type = DialogueCommandType.StartQuest, quest = quest },
            new DialogueCommand { type = DialogueCommandType.SetFlag, key = "met_shoenice" },
            new DialogueCommand { type = DialogueCommandType.TakeItem, item = liquor }
        });

        Assert.AreEqual(1, inventory.CountItem(liquor));
        Assert.AreEqual(15, inventory.Money);
        Assert.AreEqual(QuestState.ReadyToTurnIn, QuestManager.GetState(quest), "No objectives means ready to turn in right away.");
        Assert.IsTrue(GameFlags.IsSet("met_shoenice"));
    }

    [Test]
    public void Conditions_CheckQuestStateFlagsAndItems()
    {
        Item liquor = CreateItem("Liquor");
        Quest quest = CreateQuest(true);

        DialogueCondition notStarted = new DialogueCondition { type = DialogueConditionType.QuestState, quest = quest, questState = QuestState.NotStarted };
        DialogueCondition flagNotSet = new DialogueCondition { type = DialogueConditionType.Flag, flag = "door_open", not = true };
        DialogueCondition hasLiquor = new DialogueCondition { type = DialogueConditionType.HasItem, item = liquor };

        Assert.IsTrue(notStarted.IsMet());
        Assert.IsTrue(flagNotSet.IsMet());
        Assert.IsFalse(hasLiquor.IsMet());

        QuestManager.StartQuest(quest, false);
        GameFlags.Set("door_open");
        inventory.TryAddItem(liquor);

        Assert.IsFalse(notStarted.IsMet());
        Assert.IsFalse(flagNotSet.IsMet());
        Assert.IsTrue(hasLiquor.IsMet());
        Assert.IsFalse(DialogueCondition.AllMet(new List<DialogueCondition> { hasLiquor, flagNotSet }));
    }

    [Test]
    public void DialogueTrigger_UsesFirstMatchingConditionalDialogue()
    {
        Quest quest = CreateQuest(true);
        DialogueBase defaultDialogue = Track(ScriptableObject.CreateInstance<DialogueBase>());
        DialogueBase questDialogue = Track(ScriptableObject.CreateInstance<DialogueBase>());

        DialogueTrigger trigger = Track(new GameObject("NPC")).AddComponent<DialogueTrigger>();
        SetField(trigger, "dialogue", new[] { defaultDialogue });
        SetField(trigger, "conditionalDialogue", new List<DialogueTrigger.ConditionalDialogue>
        {
            new DialogueTrigger.ConditionalDialogue
            {
                conditions = new List<DialogueCondition>
                {
                    new DialogueCondition { type = DialogueConditionType.QuestState, quest = quest, questState = QuestState.ReadyToTurnIn }
                },
                dialogue = new[] { questDialogue }
            }
        });

        Assert.AreSame(defaultDialogue, trigger.GetNextDialogue());
        QuestManager.StartQuest(quest, false);
        Assert.AreSame(questDialogue, trigger.GetNextDialogue());
    }

    private T Track<T>(T a_object) where T : Object
    {
        createdObjects.Add(a_object);
        return a_object;
    }

    private Item CreateItem(string a_name)
    {
        Item item = Track(ScriptableObject.CreateInstance<Item>());
        item.name = a_name;
        return item;
    }

    private Quest CreateQuest(bool a_requiresTurnIn, params QuestObjective[] a_objectives)
    {
        Quest quest = Track(ScriptableObject.CreateInstance<Quest>());
        SetField(quest, "requiresTurnIn", a_requiresTurnIn);
        SetField(quest, "objectives", new List<QuestObjective>(a_objectives));
        return quest;
    }

    private static void SetField(object a_target, string a_field, object a_value)
    {
        FieldInfo field = null;
        for (System.Type type = a_target.GetType(); field == null && type != null; type = type.BaseType)
        {
            field = type.GetField(a_field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        Assert.IsNotNull(field, "Missing field " + a_field);
        field.SetValue(a_target, a_value);
    }
}
