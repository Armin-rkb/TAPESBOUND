using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Earthbound style quest windows: a tracker in the corner and a quest log (opened with the quest log key).
/// Builds its own UI and copies the font and window sprite of the dialogue box, so it matches by default.
/// </summary>
public class QuestUI : MonoBehaviour
{
    public static QuestUI instance;

    [Header("Style (empty = copied from the dialogue box)")]
    [SerializeField] private Font font = null;
    [SerializeField] private int fontSize = 0;
    [SerializeField] private Sprite windowSprite = null;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.35f);
    [SerializeField] private Color doneColor = new Color(0.55f, 0.55f, 0.55f);

    [Header("Layout (canvas units)")]
    [SerializeField] private int sortingOrder = 10;
    [SerializeField] private float trackerWidth = 112;
    [SerializeField] private Vector2 logSize = new Vector2(244, 200);
    [SerializeField] private float listWidth = 88;
    [SerializeField] private int listMaxCharacters = 15;
    [SerializeField] private Vector2 padding = new Vector2(8, 6);

    [Header("Text")]
    [SerializeField] private string logTitle = "QUESTS";
    [SerializeField] private string emptyLogText = "No quests yet.";
    [SerializeField] private string hintText = "Q: Close\nSpace: Track";
    [SerializeField] private string newQuestNotice = "New quest!";
    [SerializeField] private string updatedNotice = "Quest updated!";
    [SerializeField] private string completedNotice = "Quest complete!";

    [Header("Tracker")]
    [SerializeField] private bool showTracker = true;
    [Tooltip("How long the tracker shows notices like \"Quest complete!\".")]
    [SerializeField] private float noticeTime = 3f;

    private CanvasScaler sceneScaler = null;
    private float windowPixelsPerUnit = 1f;

    private RectTransform trackerWindow = null;
    private Text trackerText = null;
    private GameObject logRoot = null;
    private Text listText = null;
    private Text detailText = null;

    private readonly List<QuestProgress> logEntries = new List<QuestProgress>();
    private int selectedIndex = 0;
    private Quest trackedQuest = null;
    private string notice = "";
    private float noticeTimer = 0f;
    private bool isDialogueOpen = false;

    public bool IsLogOpen => logRoot != null && logRoot.activeSelf;

    /// <summary>
    /// Create a QuestUI with the default look.
    /// </summary>
    public static QuestUI Create()
    {
        return new GameObject("Quest UI").AddComponent<QuestUI>();
    }

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

    // Built in Start so the DialogueManager is ready to copy the style from.
    private void Start()
    {
        ResolveStyle();
        BuildUI();

        if (trackedQuest == null)
        {
            trackedQuest = FindActiveQuest();
        }
        RefreshTracker();
    }

    private void OnEnable()
    {
        QuestManager.onQuestStarted += HandleQuestStarted;
        QuestManager.onQuestUpdated += HandleQuestUpdated;
        QuestManager.onQuestCompleted += HandleQuestCompleted;
        DialogueManager.onDialogueEnter += HandleDialogueEnter;
        DialogueManager.onDialogueExit += HandleDialogueExit;
    }

    private void OnDisable()
    {
        QuestManager.onQuestStarted -= HandleQuestStarted;
        QuestManager.onQuestUpdated -= HandleQuestUpdated;
        QuestManager.onQuestCompleted -= HandleQuestCompleted;
        DialogueManager.onDialogueEnter -= HandleDialogueEnter;
        DialogueManager.onDialogueExit -= HandleDialogueExit;
    }

    private void Update()
    {
        // Notices count down only while you can see them.
        if (noticeTimer > 0f && !isDialogueOpen && !IsLogOpen)
        {
            noticeTimer -= Time.unscaledDeltaTime;
            if (noticeTimer <= 0f)
            {
                notice = "";
                if (trackedQuest != null && QuestManager.GetState(trackedQuest) == QuestState.Completed)
                {
                    trackedQuest = FindActiveQuest();
                }
                RefreshTracker();
            }
        }
    }

    public void OpenLog()
    {
        if (logRoot == null)
        {
            return;
        }

        RebuildLogEntries();
        selectedIndex = Mathf.Max(0, logEntries.FindIndex(a_entry => a_entry.Quest == trackedQuest));
        logRoot.SetActive(true);
        RefreshLog();
        RefreshTracker();
    }

    public void CloseLog()
    {
        if (logRoot == null)
        {
            return;
        }

        logRoot.SetActive(false);
        RefreshTracker();
    }

    public void MoveSelection(int a_direction)
    {
        if (!IsLogOpen || logEntries.Count == 0)
        {
            return;
        }

        selectedIndex = (selectedIndex + a_direction + logEntries.Count) % logEntries.Count;
        RefreshLog();
    }

    /// <summary>
    /// Show the selected quest in the tracker.
    /// </summary>
    public void TrackSelected()
    {
        if (!IsLogOpen || selectedIndex >= logEntries.Count || logEntries[selectedIndex].State == QuestState.Completed)
        {
            return;
        }

        trackedQuest = logEntries[selectedIndex].Quest;
        RefreshLog();
    }

    private void HandleQuestStarted(QuestProgress a_progress)
    {
        trackedQuest = a_progress.Quest;
        ShowNotice(newQuestNotice);
    }

    private void HandleQuestUpdated(QuestProgress a_progress)
    {
        if (a_progress.State == QuestState.Completed)
        {
            return;
        }

        // Keep "New quest!" when a quest is already partly done the moment it starts.
        if (trackedQuest == a_progress.Quest && notice == newQuestNotice && noticeTimer > 0f)
        {
            RefreshTracker();
            return;
        }

        trackedQuest = a_progress.Quest;
        ShowNotice(updatedNotice);
    }

    private void HandleQuestCompleted(QuestProgress a_progress)
    {
        trackedQuest = a_progress.Quest;
        ShowNotice(completedNotice);
    }

    private void HandleDialogueEnter()
    {
        isDialogueOpen = true;
        RefreshTracker();
    }

    private void HandleDialogueExit()
    {
        isDialogueOpen = false;
        RefreshTracker();
    }

    private void ShowNotice(string a_notice)
    {
        notice = a_notice;
        noticeTimer = noticeTime;
        RefreshTracker();
        if (IsLogOpen)
        {
            RebuildLogEntries();
            RefreshLog();
        }
    }

    private Quest FindActiveQuest()
    {
        foreach (QuestProgress progress in QuestManager.Quests)
        {
            if (progress.State != QuestState.Completed)
            {
                return progress.Quest;
            }
        }
        return null;
    }

    private void RebuildLogEntries()
    {
        // Ongoing quests first, then the completed ones.
        logEntries.Clear();
        foreach (QuestProgress progress in QuestManager.Quests)
        {
            if (progress.State != QuestState.Completed)
            {
                logEntries.Add(progress);
            }
        }
        foreach (QuestProgress progress in QuestManager.Quests)
        {
            if (progress.State == QuestState.Completed)
            {
                logEntries.Add(progress);
            }
        }
        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, logEntries.Count - 1));
    }

    private void RefreshTracker()
    {
        if (trackerWindow == null)
        {
            return;
        }

        QuestProgress progress = QuestManager.GetProgress(trackedQuest);
        bool hasContent = progress != null && (progress.State != QuestState.Completed || noticeTimer > 0f);
        bool isVisible = showTracker && hasContent && !isDialogueOpen && !IsLogOpen;
        trackerWindow.gameObject.SetActive(isVisible);
        if (!isVisible)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        if (noticeTimer > 0f && !string.IsNullOrEmpty(notice))
        {
            builder.AppendLine(Colored(notice, highlightColor));
        }
        builder.Append(Colored(progress.Quest.Title, progress.State == QuestState.Completed ? doneColor : textColor));
        AppendObjectives(builder, progress, true);

        trackerText.text = builder.ToString();
        trackerWindow.sizeDelta = new Vector2(trackerWidth, Mathf.Ceil(trackerText.preferredHeight) + padding.y * 2f);
    }

    private void RefreshLog()
    {
        StringBuilder list = new StringBuilder();
        list.AppendLine(logTitle);
        if (logEntries.Count == 0)
        {
            list.AppendLine().Append(Colored(emptyLogText, doneColor));
        }

        for (int i = 0; i < logEntries.Count; i++)
        {
            QuestProgress entry = logEntries[i];
            string title = Truncate(entry.Quest.Title, listMaxCharacters);
            string line = (i == selectedIndex ? "> " : "  ") + title;

            Color color = i == selectedIndex ? highlightColor : textColor;
            if (entry.State == QuestState.Completed && i != selectedIndex)
            {
                color = doneColor;
            }
            list.AppendLine().Append(Colored(line, color));
        }
        listText.text = list.ToString();

        detailText.text = logEntries.Count > 0 ? BuildDetails(logEntries[selectedIndex]) : "";
    }

    private string BuildDetails(QuestProgress a_progress)
    {
        Quest quest = a_progress.Quest;
        StringBuilder builder = new StringBuilder();

        builder.Append(Colored(quest.Title, highlightColor));
        if (quest == trackedQuest && a_progress.State != QuestState.Completed)
        {
            builder.Append(Colored(" *", highlightColor));
        }
        builder.AppendLine();

        if (!string.IsNullOrEmpty(quest.Description))
        {
            builder.AppendLine(DialogueManager.FormatText(quest.Description));
        }

        builder.AppendLine();
        if (a_progress.State == QuestState.Completed)
        {
            builder.AppendLine(Colored(completedNotice, doneColor));
        }
        AppendObjectives(builder, a_progress, false);

        string rewards = BuildRewards(quest);
        if (!string.IsNullOrEmpty(rewards))
        {
            builder.AppendLine().AppendLine().Append("Reward: ").Append(rewards);
        }
        return builder.ToString();
    }

    private void AppendObjectives(StringBuilder a_builder, QuestProgress a_progress, bool a_onlyOpen)
    {
        Quest quest = a_progress.Quest;
        if (a_progress.State == QuestState.ReadyToTurnIn)
        {
            string turnIn = string.IsNullOrEmpty(quest.TurnInText) ? "Report back" : quest.TurnInText;
            a_builder.AppendLine().Append(Colored("- " + turnIn, highlightColor));
        }

        if (a_onlyOpen && a_progress.State != QuestState.Active)
        {
            return;
        }

        for (int i = 0; i < quest.Objectives.Count; i++)
        {
            if (!a_progress.IsObjectiveVisible(i))
            {
                continue;
            }

            bool isDone = a_progress.IsObjectiveComplete(i);
            if (a_onlyOpen && isDone)
            {
                continue;
            }

            QuestObjective objective = quest.Objectives[i];
            string line = "- " + objective.GetDescription();
            if (objective.RequiredAmount > 1)
            {
                line += " " + a_progress.GetProgress(i) + "/" + objective.RequiredAmount;
            }
            a_builder.AppendLine().Append(Colored(line, isDone ? doneColor : textColor));

            // The tracker only shows the current step of an in order quest.
            if (a_onlyOpen && quest.ObjectivesInOrder)
            {
                break;
            }
        }
    }

    private static string BuildRewards(Quest a_quest)
    {
        List<string> rewards = new List<string>();
        if (a_quest.MoneyReward > 0)
        {
            rewards.Add("$" + a_quest.MoneyReward);
        }
        foreach (Item item in a_quest.ItemRewards)
        {
            if (item != null)
            {
                rewards.Add(item.DisplayName);
            }
        }
        if (a_quest.RewardNotes != null)
        {
            foreach (string note in a_quest.RewardNotes)
            {
                if (!string.IsNullOrEmpty(note))
                {
                    rewards.Add(note);
                }
            }
        }
        return string.Join(", ", rewards);
    }

    private static string Colored(string a_text, Color a_color)
    {
        return "<color=#" + ColorUtility.ToHtmlStringRGB(a_color) + ">" + a_text + "</color>";
    }

    private static string Truncate(string a_text, int a_maxCharacters)
    {
        if (a_text.Length <= a_maxCharacters)
        {
            return a_text;
        }
        return a_text.Substring(0, Mathf.Max(0, a_maxCharacters - 2)) + "..";
    }

    // Copy the font, window sprite and scaling of the dialogue box.
    private void ResolveStyle()
    {
        DialogueManager dialogueManager = DialogueManager.instance;
        if (dialogueManager != null && dialogueManager.dialogueText != null)
        {
            if (font == null)
            {
                font = dialogueManager.dialogueText.font;
            }
            if (fontSize <= 0)
            {
                fontSize = dialogueManager.dialogueText.fontSize;
            }
        }

        if (dialogueManager != null && dialogueManager.dialogueBox != null)
        {
            Image box = dialogueManager.dialogueBox.GetComponent<Image>();
            if (windowSprite == null && box != null)
            {
                windowSprite = box.sprite;
                windowPixelsPerUnit = box.pixelsPerUnitMultiplier;
            }
            sceneScaler = dialogueManager.dialogueBox.GetComponentInParent<CanvasScaler>();
        }

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        if (fontSize <= 0)
        {
            fontSize = 8;
        }
    }

    private void BuildUI()
    {
        GameObject canvasObject = new GameObject("Quest Canvas", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(256, 224);
        scaler.referencePixelsPerUnit = 32;
        scaler.matchWidthOrHeight = 0.5f;
        if (sceneScaler != null)
        {
            scaler.referenceResolution = sceneScaler.referenceResolution;
            scaler.referencePixelsPerUnit = sceneScaler.referencePixelsPerUnit;
            scaler.screenMatchMode = sceneScaler.screenMatchMode;
            scaler.matchWidthOrHeight = sceneScaler.matchWidthOrHeight;
        }

        // Tracker, top left.
        trackerWindow = CreateWindow("Quest Tracker", canvasObject.transform, new Vector2(0f, 1f), new Vector2(6f, -6f), new Vector2(trackerWidth, 32f));
        trackerText = CreateText(trackerWindow, TextAnchor.UpperLeft);

        // Quest log, centered: list on the left, details on the right.
        RectTransform logRect = new GameObject("Quest Log", typeof(RectTransform)).GetComponent<RectTransform>();
        logRect.SetParent(canvasObject.transform, false);
        logRect.sizeDelta = logSize;
        logRoot = logRect.gameObject;

        RectTransform listWindow = CreateWindow("Quest List", logRect, new Vector2(0f, 1f), Vector2.zero, new Vector2(listWidth, logSize.y));
        listText = CreateText(listWindow, TextAnchor.UpperLeft);
        Text hint = CreateText(listWindow, TextAnchor.LowerLeft);
        hint.text = Colored(hintText, doneColor);

        float detailWidth = logSize.x - listWidth - 4f;
        RectTransform detailWindow = CreateWindow("Quest Details", logRect, new Vector2(1f, 1f), Vector2.zero, new Vector2(detailWidth, logSize.y));
        detailText = CreateText(detailWindow, TextAnchor.UpperLeft);

        logRoot.SetActive(false);
    }

    private RectTransform CreateWindow(string a_name, Transform a_parent, Vector2 a_corner, Vector2 a_position, Vector2 a_size)
    {
        RectTransform window = new GameObject(a_name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        window.SetParent(a_parent, false);
        window.anchorMin = a_corner;
        window.anchorMax = a_corner;
        window.pivot = a_corner;
        window.anchoredPosition = a_position;
        window.sizeDelta = a_size;

        Image image = window.GetComponent<Image>();
        image.raycastTarget = false;
        if (windowSprite != null)
        {
            image.sprite = windowSprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = windowPixelsPerUnit;
        }
        else
        {
            image.color = new Color(0f, 0f, 0f, 0.9f);
        }
        return window;
    }

    private Text CreateText(RectTransform a_window, TextAnchor a_alignment)
    {
        RectTransform rect = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<RectTransform>();
        rect.SetParent(a_window, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = padding;
        rect.offsetMax = -padding;

        Text text = rect.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = a_alignment;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }
}
