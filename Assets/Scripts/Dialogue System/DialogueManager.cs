using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
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

    [Header("Dialogue Options")]
    [SerializeField]
    private float dialogueDelay = 0.01f;
    [Tooltip("Put in front of every line. Earthbound uses \"@\".")]
    [SerializeField]
    private string linePrefix = "";

    [Header("Text Sound (optional)")]
    [SerializeField] private AudioClip textBlip = null;
    [SerializeField] private int charactersPerBlip = 2;
    [SerializeField] private AudioSource audioSource = null;

    [Header("System Messages")]
    [SerializeField] private SystemMessages systemMessages = new SystemMessages();

    [Header("UI References")]
    public GameObject dialogueBox = null;
    public Text dialogueText = null;
    public GameObject dialogueOptionUI = null;
    public GameObject[] optionButtons = null;
    [Tooltip("Optional: blinks when a line is done and waits for the player.")]
    [SerializeField] private GameObject continueIndicator = null;

    // One line in the text box.
    private class Line
    {
        public string text;
        public DialogueBase source;
        public int lineNumber;                  // 1 based, matches DialogueEvent.eventLine.
        public List<DialogueCommand> commands;
        public DialogueOptions options;         // The options appear after this line is typed.
    }

    // Dialogue variables.
    private readonly List<Line> pendingLines = new List<Line>();
    private Line currentLine = null;
    private int insertIndex = 0;
    private bool isOpen = false;
    private bool isFinishing = false;
    private bool isTimelineDialogue = false;

    private DialogueOptions activeOptions = null;
    private bool isShowingOptions = false;

    private bool isCurrentlyTyping = false;
    private bool isWaitingForInput = false;
    private string completeText = "";
    private Coroutine typingRoutine = null;

    private bool isBuffering = false;
    private float dialogueBufferTime = 0.1f;
    private Coroutine bufferRoutine = null;

    private string speakingNPC = null;

    private static readonly string[] richTextTags = { "b", "i", "size", "color" };
    private static readonly SystemMessages defaultMessages = new SystemMessages();

    public bool IsOpen => isOpen;
    public static SystemMessages Messages => instance != null && instance.systemMessages != null ? instance.systemMessages : defaultMessages;

    // Static Events
    public delegate void OnDialogueEnter();
    public static event OnDialogueEnter onDialogueEnter;
    public delegate void OnDialogueExit();
    public static event OnDialogueExit onDialogueExit;
    public delegate void OnSpokenWithNPC(string a_npc_id);
    public static event OnSpokenWithNPC onSpokenWithNPC;

    private void Update()
    {
        if (isWaitingForInput && continueIndicator != null)
        {
            continueIndicator.SetActive(Mathf.Repeat(Time.unscaledTime, 0.8f) < 0.5f);
        }
    }

    private void OnDisable()
    {
        // Coroutines stop when disabled, don't leave the flags stuck.
        isBuffering = false;
        bufferRoutine = null;
        typingRoutine = null;
        if (isCurrentlyTyping)
        {
            isCurrentlyTyping = false;
            if (dialogueText != null)
            {
                dialogueText.text = completeText;
            }
        }
    }

    /// <summary>
    /// Open the text box. Lines are added with EnqueueDialogue.
    /// </summary>
    public void OpenDialogue(string a_NPC)
    {
        if (isOpen)
        {
            // Already talking (e.g. EnqueueDialogue was called first), only remember who we talk to.
            if (!string.IsNullOrEmpty(a_NPC))
            {
                speakingNPC = a_NPC;
            }
            return;
        }

        isOpen = true;
        speakingNPC = a_NPC;
        pendingLines.Clear();
        currentLine = null;
        insertIndex = 0;

        GameAudio.Play(GameSound.DialogueOpen);
        dialogueBox.SetActive(true);
        dialogueText.text = "";
        CloseOptions();

        onDialogueEnter?.Invoke();
    }

    /// <summary>
    /// Play a dialogue. Opens the text box when needed, during a conversation it plays right after the current line.
    /// </summary>
    public void EnqueueDialogue(DialogueBase a_dialogueBase)
    {
        if (a_dialogueBase == null)
        {
            return;
        }

        OpenDialogue(null);

        // Pause the timeline while its dialogue is on screen.
        if (a_dialogueBase is DialogueTimeline && TimelineManager.instance != null)
        {
            isTimelineDialogue = true;
            TimelineManager.instance.PauseTimeline();
        }

        InsertLines(BuildLines(a_dialogueBase));
        StartBuffer();

        if (isShowingOptions)
        {
            // An option was handled from outside (e.g. UnityEventHandler), continue with the new lines.
            CloseOptions();
            ShowNextLine();
        }
        else if (currentLine == null && !isFinishing)
        {
            ShowNextLine();
        }
    }

    /// <summary>
    /// Show a message from the game itself (rewards, quests...). During a conversation it shows after the current line.
    /// </summary>
    public void ShowMessage(string a_text)
    {
        if (string.IsNullOrEmpty(a_text))
        {
            return;
        }

        OpenDialogue(null);
        InsertLines(new List<Line> { new Line { text = a_text } });

        if (currentLine == null && !isFinishing && !isShowingOptions)
        {
            StartBuffer();
            ShowNextLine();
        }
    }

    /// <summary>
    /// Show a message, or log it when there is no DialogueManager in the scene.
    /// </summary>
    public static void ShowSystemMessage(string a_text)
    {
        if (instance != null)
        {
            instance.ShowMessage(a_text);
        }
        else
        {
            Debug.Log(FormatText(a_text));
        }
    }

    /// <summary>
    /// Called when the player presses interact: finish typing, pick the selected option or show the next line.
    /// </summary>
    public void DequeueDialogue()
    {
        if (!isOpen || isBuffering)
        {
            return;
        }

        if (isCurrentlyTyping)
        {
            CompleteText();
            return;
        }

        if (isShowingOptions)
        {
            ChooseOption(GetSelectedOptionIndex());
            return;
        }

        if (pendingLines.Count > 0)
        {
            GameAudio.Play(GameSound.TextAdvance);
        }
        ShowNextLine();
    }

    /// <summary>
    /// Pick one of the options on screen.
    /// </summary>
    public void ChooseOption(int a_index)
    {
        if (!isShowingOptions || activeOptions == null || a_index < 0 || a_index >= activeOptions.optionsInfo.Length)
        {
            return;
        }

        DialogueOptions.Options option = activeOptions.optionsInfo[a_index];
        CloseOptions();
        GameAudio.Play(GameSound.Confirm);

        // What the option does shows first, then the dialogue it leads to.
        insertIndex = 0;
        try
        {
            option.buttonEvent?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
        DialogueCommand.ExecuteAll(option.commands);
        EnqueueDialogue(option.nextDialogue);

        StartBuffer();
        ShowNextLine();
    }

    public void CloseOptions()
    {
        isShowingOptions = false;
        activeOptions = null;

        if (optionButtons != null)
        {
            foreach (GameObject optionButton in optionButtons)
            {
                Button button = optionButton != null ? optionButton.GetComponent<Button>() : null;
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }

        if (dialogueOptionUI != null)
        {
            dialogueOptionUI.SetActive(false);
        }
    }

    /// <summary>
    /// Replace the tokens {player} and {money} in a text.
    /// </summary>
    public static string FormatText(string a_text)
    {
        if (string.IsNullOrEmpty(a_text))
        {
            return "";
        }

        string result = a_text;
        if (result.Contains("{player}"))
        {
            result = result.Replace("{player}", PlayerController.PlayerName);
        }
        if (result.Contains("{money}"))
        {
            PlayerInventory inventory = PlayerInventory.Current;
            result = result.Replace("{money}", inventory != null ? inventory.Money.ToString() : "0");
        }
        return result;
    }

    public static void PlaySound(AudioClip a_clip)
    {
        if (a_clip == null)
        {
            return;
        }

        if (instance != null)
        {
            instance.GetAudioSource().PlayOneShot(a_clip);
        }
        else if (Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(a_clip, Camera.main.transform.position);
        }
    }

    private AudioSource GetAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        return audioSource;
    }

    private List<Line> BuildLines(DialogueBase a_dialogueBase)
    {
        List<Line> lines = new List<Line>();
        DialogueBase.Info[] infos = a_dialogueBase.dialogueInfo ?? new DialogueBase.Info[0];

        for (int i = 0; i < infos.Length; i++)
        {
            lines.Add(new Line
            {
                text = infos[i].text,
                source = a_dialogueBase,
                lineNumber = i + 1,
                commands = infos[i].commands
            });
        }

        // The options show up after the last line.
        if (a_dialogueBase is DialogueOptions dialogueOptions && dialogueOptions.optionsInfo != null && dialogueOptions.optionsInfo.Length > 0)
        {
            if (lines.Count == 0)
            {
                lines.Add(new Line { text = "", source = a_dialogueBase, lineNumber = 1 });
            }
            lines[lines.Count - 1].options = dialogueOptions;
        }

        return lines;
    }

    private void InsertLines(List<Line> a_lines)
    {
        int index = Mathf.Clamp(insertIndex, 0, pendingLines.Count);
        pendingLines.InsertRange(index, a_lines);
        insertIndex = index + a_lines.Count;
    }

    private void ShowNextLine()
    {
        if (pendingLines.Count == 0)
        {
            FinishConversation();
            return;
        }

        Line line = pendingLines[0];
        pendingLines.RemoveAt(0);
        currentLine = line;
        insertIndex = 0;

        // Events and commands run first, lines they add show up right after this one.
        RunLineEvents(line);
        DialogueCommand.ExecuteAll(line.commands);

        if (currentLine != line)
        {
            return;
        }

        TypeLine(line);
    }

    private void RunLineEvents(Line a_line)
    {
        if (a_line.source is DialogueEvent dialogueEvent && dialogueEvent.dialogueEvents != null)
        {
            foreach (DialogueEvent.Event lineEvent in dialogueEvent.dialogueEvents)
            {
                if (Mathf.Max(1, lineEvent.eventLine) != a_line.lineNumber)
                {
                    continue;
                }

                try
                {
                    lineEvent.myEvent?.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }
    }

    private void TypeLine(Line a_line)
    {
        completeText = string.IsNullOrEmpty(a_line.text) ? "" : linePrefix + FormatText(a_line.text);
        dialogueText.text = "";
        SetWaitingForInput(false);

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
        }
        typingRoutine = StartCoroutine(TypeText(completeText));
    }

    private IEnumerator TypeText(string a_text)
    {
        isCurrentlyTyping = true;

        StringBuilder typed = new StringBuilder();
        List<string> openTags = new List<string>();
        int visibleCharacters = 0;

        for (int i = 0; i < a_text.Length; i++)
        {
            // Rich text tags (<color=red>) are added at once so they never show half typed.
            if (a_text[i] == '<')
            {
                int tagEnd = a_text.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    string tag = a_text.Substring(i, tagEnd - i + 1);
                    TrackRichTextTag(tag, openTags);
                    typed.Append(tag);
                    i = tagEnd;
                    continue;
                }
            }

            typed.Append(a_text[i]);
            dialogueText.text = typed + ClosingTags(openTags);

            if (!char.IsWhiteSpace(a_text[i]))
            {
                PlayBlip(visibleCharacters);
                visibleCharacters++;
            }

            yield return new WaitForSeconds(dialogueDelay);
        }

        typingRoutine = null;
        dialogueText.text = a_text;
        isCurrentlyTyping = false;
        OnLineTyped();
    }

    private void CompleteText()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        dialogueText.text = completeText;
        isCurrentlyTyping = false;
        OnLineTyped();
    }

    private void OnLineTyped()
    {
        if (currentLine != null && currentLine.options != null)
        {
            ShowOptions(currentLine.options);
        }
        else
        {
            SetWaitingForInput(true);
        }
    }

    private void ShowOptions(DialogueOptions a_options)
    {
        activeOptions = a_options;
        isShowingOptions = true;
        SetWaitingForInput(false);

        int optionsAmount = Mathf.Min(a_options.optionsInfo.Length, optionButtons.Length);
        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool isUsed = i < optionsAmount;
            optionButtons[i].SetActive(isUsed);

            Button button = optionButtons[i].GetComponent<Button>();
            if (!isUsed || button == null)
            {
                continue;
            }

            optionButtons[i].transform.GetChild(0).GetComponent<Text>().text = FormatText(a_options.optionsInfo[i].buttonName);
            int index = i;
            button.onClick.AddListener(() => ChooseOption(index));
        }

        dialogueOptionUI.SetActive(true);

        // Select the first option by default.
        if (optionsAmount > 0 && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionButtons[0]);
            SelectableUI selectable = optionButtons[0].GetComponent<SelectableUI>();
            if (selectable != null)
            {
                selectable.OnSelect(new BaseEventData(EventSystem.current));
            }
        }
    }

    private int GetSelectedOptionIndex()
    {
        if (EventSystem.current != null)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == selected)
                {
                    return i;
                }
            }
        }
        return 0;
    }

    private void FinishConversation()
    {
        currentLine = null;
        insertIndex = pendingLines.Count;

        // Let listeners (like quests) react while the box is still open, they can add more lines.
        if (!string.IsNullOrEmpty(speakingNPC))
        {
            string npc = speakingNPC;
            speakingNPC = null;

            isFinishing = true;
            try
            {
                onSpokenWithNPC?.Invoke(npc);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            isFinishing = false;
        }

        if (pendingLines.Count > 0)
        {
            ShowNextLine();
            return;
        }

        CloseDialogue();
    }

    private void CloseDialogue()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        isCurrentlyTyping = false;
        pendingLines.Clear();
        currentLine = null;
        speakingNPC = null;
        CloseOptions();
        SetWaitingForInput(false);

        GameAudio.Play(GameSound.DialogueClose);
        dialogueBox.SetActive(false);
        isOpen = false;

        if (isTimelineDialogue)
        {
            isTimelineDialogue = false;
            if (TimelineManager.instance != null)
            {
                TimelineManager.instance.ResumeTimeline();
            }
        }

        onDialogueExit?.Invoke();
    }

    private void StartBuffer()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (bufferRoutine != null)
        {
            StopCoroutine(bufferRoutine);
        }
        bufferRoutine = StartCoroutine(BufferTimer());
    }

    private IEnumerator BufferTimer()
    {
        isBuffering = true;
        yield return new WaitForSeconds(dialogueBufferTime);
        isBuffering = false;
        bufferRoutine = null;
    }

    private void SetWaitingForInput(bool a_isWaiting)
    {
        isWaitingForInput = a_isWaiting;
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(a_isWaiting);
        }
    }

    private void PlayBlip(int a_characterIndex)
    {
        if (a_characterIndex % Mathf.Max(1, charactersPerBlip) != 0)
        {
            return;
        }

        if (textBlip != null)
        {
            GetAudioSource().PlayOneShot(textBlip);
        }
        else
        {
            GameAudio.Play(GameSound.TextBlip);
        }
    }

    private static void TrackRichTextTag(string a_tag, List<string> a_openTags)
    {
        bool isClosing = a_tag.StartsWith("</");
        string tagName = a_tag.Trim('<', '>', '/');
        int valueStart = tagName.IndexOf('=');
        if (valueStart != -1)
        {
            tagName = tagName.Substring(0, valueStart);
        }

        if (Array.IndexOf(richTextTags, tagName) == -1)
        {
            return;
        }

        if (isClosing)
        {
            int last = a_openTags.LastIndexOf(tagName);
            if (last != -1)
            {
                a_openTags.RemoveAt(last);
            }
        }
        else
        {
            a_openTags.Add(tagName);
        }
    }

    private static string ClosingTags(List<string> a_openTags)
    {
        if (a_openTags.Count == 0)
        {
            return "";
        }

        StringBuilder closing = new StringBuilder();
        for (int i = a_openTags.Count - 1; i >= 0; i--)
        {
            closing.Append("</").Append(a_openTags[i]).Append('>');
        }
        return closing.ToString();
    }
}
