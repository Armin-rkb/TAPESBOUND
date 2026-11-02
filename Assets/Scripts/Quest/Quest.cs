using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The definition of a quest. Its progress lives in the QuestManager, never in this asset.
/// </summary>
[CreateAssetMenu(fileName = "New Quest", menuName = "Quest/Quest", order = 0)]
public class Quest : ScriptableObject, IQuest
{
    [Header("Info")]
    [SerializeField] protected new string name;
    [TextArea(2, 5)]
    [SerializeField] protected string description;

    [Header("Objectives")]
    [Tooltip("Objectives are done one after another, later ones stay hidden in the quest log.")]
    [SerializeField] protected bool objectivesInOrder = true;
    [SerializeField] protected List<QuestObjective> objectives = new List<QuestObjective>();

    [Header("Turn in")]
    [Tooltip("When all objectives are done the quest waits as 'Ready to turn in' until a CompleteQuest command runs (e.g. in the quest giver's dialogue).")]
    [SerializeField] protected bool requiresTurnIn = false;
    [Tooltip("Shown in the quest log while ready to turn in, e.g. \"Bring the liquor to Shoenice\".")]
    [SerializeField] protected string turnInText = "";

    [Header("Rewards")]
    [SerializeField] protected int moneyReward = 0;
    [SerializeField] protected List<Item> itemRewards = new List<Item>();
    [Tooltip("Extra reward text for the quest log, these are not given automatically.")]
    [SerializeField] protected List<string> questRewards = new List<string>();
    [Tooltip("Runs when the quest completes, e.g. set a flag or start the next quest.")]
    [SerializeField] protected List<DialogueCommand> onCompleteCommands = new List<DialogueCommand>();
    [Tooltip("Plays when the quest completes, replaces the default \"Quest complete\" message.")]
    [SerializeField] protected DialogueBase completeDialogue;

    public string Title => string.IsNullOrEmpty(name) ? base.name : name;
    public string Description => description;
    public bool ObjectivesInOrder => objectivesInOrder;
    public virtual IReadOnlyList<QuestObjective> Objectives => objectives;
    public bool RequiresTurnIn => requiresTurnIn;
    public string TurnInText => turnInText;
    public int MoneyReward => moneyReward;
    public IReadOnlyList<Item> ItemRewards => itemRewards;
    public IReadOnlyList<string> RewardNotes => questRewards;
    public IList<DialogueCommand> OnCompleteCommands => onCompleteCommands;
    public DialogueBase CompleteDialogue => completeDialogue;

    public virtual void AcceptQuest()
    {
        QuestManager.StartQuest(this);
    }

    public virtual void Complete()
    {
        QuestManager.CompleteQuest(this);
    }
}
