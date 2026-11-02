using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum QuestState
{
    NotStarted = 0,
    Active = 1,
    ReadyToTurnIn = 2,
    Completed = 3
}

/// <summary>
/// Runtime progress of one quest.
/// </summary>
public class QuestProgress
{
    public Quest Quest { get; }
    public QuestState State { get; internal set; }

    private int[] objectiveProgress;

    public QuestProgress(Quest a_quest)
    {
        Quest = a_quest;
        State = QuestState.Active;
        objectiveProgress = new int[a_quest.Objectives.Count];
    }

    public int GetProgress(int a_objectiveIndex)
    {
        if (State == QuestState.Completed)
        {
            return Quest.Objectives[a_objectiveIndex].RequiredAmount;
        }
        return a_objectiveIndex < objectiveProgress.Length ? objectiveProgress[a_objectiveIndex] : 0;
    }

    internal void SetProgress(int a_objectiveIndex, int a_value)
    {
        // The quest asset can change while playing in the editor.
        if (objectiveProgress.Length != Quest.Objectives.Count)
        {
            System.Array.Resize(ref objectiveProgress, Quest.Objectives.Count);
        }
        objectiveProgress[a_objectiveIndex] = Mathf.Clamp(a_value, 0, Quest.Objectives[a_objectiveIndex].RequiredAmount);
    }

    public bool IsObjectiveComplete(int a_objectiveIndex)
    {
        return GetProgress(a_objectiveIndex) >= Quest.Objectives[a_objectiveIndex].RequiredAmount;
    }

    /// <summary>
    /// Index of the first unfinished objective, -1 when all are done.
    /// </summary>
    public int CurrentObjectiveIndex
    {
        get
        {
            for (int i = 0; i < Quest.Objectives.Count; i++)
            {
                if (!IsObjectiveComplete(i))
                {
                    return i;
                }
            }
            return -1;
        }
    }

    // In order quests only show what has been reached so far.
    public bool IsObjectiveVisible(int a_objectiveIndex)
    {
        int current = CurrentObjectiveIndex;
        return !Quest.ObjectivesInOrder || current == -1 || a_objectiveIndex <= current;
    }

    // In order quests only progress their current objective.
    public bool CanProgress(int a_objectiveIndex)
    {
        return !Quest.ObjectivesInOrder || a_objectiveIndex == CurrentObjectiveIndex;
    }
}

/// <summary>
/// Keeps track of every quest. The progress is static, so it survives loading another scene.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Create instance!");
        }
        else
        {
            instance = this;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    [Header("Quests that start when this scene loads.")]
    [FormerlySerializedAs("activeQuests")]
    [SerializeField] private List<Quest> startingQuests = new List<Quest>();

    [Header("Feedback")]
    [Tooltip("Print \"New quest\" / \"Quest complete\" and rewards in the text box.")]
    [SerializeField] private bool announceInDialogue = true;
    [Tooltip("Create the quest log and tracker when the scene doesn't have a QuestUI.")]
    [SerializeField] private bool createQuestUI = true;

    // Static Events
    public delegate void OnQuestChanged(QuestProgress a_progress);
    public static event OnQuestChanged onQuestStarted;
    public static event OnQuestChanged onQuestUpdated;
    public static event OnQuestChanged onQuestCompleted;

    private static readonly Dictionary<Quest, QuestProgress> questLookup = new Dictionary<Quest, QuestProgress>();
    private static readonly List<QuestProgress> questLog = new List<QuestProgress>();
    private static bool isListening = false;

    private static bool Announce => instance == null || instance.announceInDialogue;

    /// <summary>
    /// Every quest that was started, in the order they were started.
    /// </summary>
    public static IReadOnlyList<QuestProgress> Quests => questLog;

    // Start is called before the first frame update
    void Start()
    {
        // UI first, so it hears about the starting quests.
        if (createQuestUI && FindObjectOfType<QuestUI>() == null)
        {
            QuestUI.Create();
        }

        foreach (Quest quest in startingQuests)
        {
            StartQuest(quest, false);
        }
    }

    // Start every play session with a clean slate (also when domain reload is disabled).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetAll()
    {
        questLookup.Clear();
        questLog.Clear();
    }

    private static void ListenToGame()
    {
        if (isListening)
        {
            return;
        }

        isListening = true;
        DialogueManager.onSpokenWithNPC += HandleSpokenWithNPC;
        PlayerInventory.onInventoryChanged += RefreshAll;
        GameFlags.onFlagChanged += HandleFlagChanged;
    }

    public static QuestProgress GetProgress(Quest a_quest)
    {
        if (a_quest == null)
        {
            return null;
        }

        questLookup.TryGetValue(a_quest, out QuestProgress progress);
        return progress;
    }

    public static QuestState GetState(Quest a_quest)
    {
        QuestProgress progress = GetProgress(a_quest);
        return progress != null ? progress.State : QuestState.NotStarted;
    }

    /// <summary>
    /// Start a quest, does nothing when it was already started or completed.
    /// </summary>
    public static bool StartQuest(Quest a_quest, bool a_announce = true)
    {
        if (a_quest == null || questLookup.ContainsKey(a_quest))
        {
            return false;
        }

        ListenToGame();

        QuestProgress progress = new QuestProgress(a_quest);
        questLookup.Add(a_quest, progress);
        questLog.Add(progress);

        if (a_announce && Announce)
        {
            DialogueManager.ShowSystemMessage(SystemMessages.Format(DialogueManager.Messages.questStarted, null, 0, a_quest));
        }
        onQuestStarted?.Invoke(progress);

        // You might already have what the quest asks for.
        Refresh(progress, false);
        return true;
    }

    /// <summary>
    /// Add progress to TalkTo / Custom objectives whose target matches a_key.
    /// An empty key advances the current objective.
    /// </summary>
    public static void AdvanceObjective(Quest a_quest, string a_key, int a_amount = 1)
    {
        QuestProgress progress = GetProgress(a_quest);
        if (progress == null || progress.State == QuestState.Completed)
        {
            return;
        }

        bool changed = false;
        IReadOnlyList<QuestObjective> objectives = a_quest.Objectives;
        for (int i = 0; i < objectives.Count; i++)
        {
            QuestObjective objective = objectives[i];
            bool isMatch = string.IsNullOrEmpty(a_key) ? i == progress.CurrentObjectiveIndex : objective.target == a_key;

            if (isMatch && !objective.IsStateBased && progress.CanProgress(i) && !progress.IsObjectiveComplete(i))
            {
                progress.SetProgress(i, progress.GetProgress(i) + a_amount);
                changed = true;
            }
        }

        Refresh(progress, changed);
    }

    /// <summary>
    /// Complete a quest and hand out its rewards. Works from any state, so dialogue can force it.
    /// </summary>
    public static void CompleteQuest(Quest a_quest, bool a_announce = true)
    {
        if (a_quest == null)
        {
            return;
        }

        QuestProgress progress = GetProgress(a_quest);
        if (progress == null)
        {
            StartQuest(a_quest, false);
            progress = GetProgress(a_quest);
        }

        if (progress.State == QuestState.Completed)
        {
            return;
        }

        progress.State = QuestState.Completed;

        bool announce = a_announce && Announce;
        if (a_quest.CompleteDialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.EnqueueDialogue(a_quest.CompleteDialogue);
        }
        else if (announce)
        {
            DialogueManager.ShowSystemMessage(SystemMessages.Format(DialogueManager.Messages.questCompleted, null, 0, a_quest));
        }

        DialogueCommand.GiveMoney(a_quest.MoneyReward, !announce);
        foreach (Item item in a_quest.ItemRewards)
        {
            DialogueCommand.GiveItem(item, 1, !announce);
        }
        DialogueCommand.ExecuteAll(a_quest.OnCompleteCommands);

        onQuestCompleted?.Invoke(progress);
        onQuestUpdated?.Invoke(progress);
    }

    private static void HandleSpokenWithNPC(string a_npc_id)
    {
        foreach (QuestProgress progress in questLog.ToArray())
        {
            if (progress.State == QuestState.Completed)
            {
                continue;
            }

            bool changed = false;
            IReadOnlyList<QuestObjective> objectives = progress.Quest.Objectives;
            for (int i = 0; i < objectives.Count; i++)
            {
                if (objectives[i].type == QuestObjectiveType.TalkTo && objectives[i].target == a_npc_id &&
                    progress.CanProgress(i) && !progress.IsObjectiveComplete(i))
                {
                    progress.SetProgress(i, progress.GetProgress(i) + 1);
                    changed = true;
                }
            }

            Refresh(progress, changed);
        }
    }

    private static void HandleFlagChanged(string a_flag, bool a_isSet)
    {
        RefreshAll();
    }

    private static void RefreshAll()
    {
        foreach (QuestProgress progress in questLog.ToArray())
        {
            Refresh(progress, false);
        }
    }

    // Update state based objectives and move the quest to the right state.
    private static void Refresh(QuestProgress a_progress, bool a_changed)
    {
        if (a_progress.State == QuestState.Completed)
        {
            return;
        }

        bool changed = a_changed;
        IReadOnlyList<QuestObjective> objectives = a_progress.Quest.Objectives;
        for (int i = 0; i < objectives.Count; i++)
        {
            if (!objectives[i].IsStateBased)
            {
                continue;
            }

            int value = objectives[i].EvaluateState();
            if (value != a_progress.GetProgress(i))
            {
                a_progress.SetProgress(i, value);
                changed = true;
            }
        }

        if (a_progress.CurrentObjectiveIndex == -1)
        {
            if (!a_progress.Quest.RequiresTurnIn)
            {
                CompleteQuest(a_progress.Quest);
                return;
            }

            if (a_progress.State != QuestState.ReadyToTurnIn)
            {
                a_progress.State = QuestState.ReadyToTurnIn;
                changed = true;
            }
        }
        else if (a_progress.State == QuestState.ReadyToTurnIn)
        {
            // Lost what the quest needed (e.g. gave the item away).
            a_progress.State = QuestState.Active;
            changed = true;
        }

        if (changed)
        {
            onQuestUpdated?.Invoke(a_progress);
        }
    }
}
