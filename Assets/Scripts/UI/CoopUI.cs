using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows who plays which character ("P2" above a party member) and a short message when someone joins or leaves.
/// Builds its own UI and copies the font of the dialogue box, like the QuestUI.
/// </summary>
public class CoopUI : MonoBehaviour
{
    public static CoopUI instance;

    [Header("Style (empty = copied from the dialogue box)")]
    [SerializeField] private Font font = null;
    [SerializeField] private int fontSize = 0;
    [SerializeField] private Color outlineColor = Color.black;
    [Tooltip("Player 1 to 4.")]
    [SerializeField] private Color[] playerColors =
    {
        new Color(1f, 0.4f, 0.4f),
        new Color(0.45f, 0.65f, 1f),
        new Color(0.45f, 0.9f, 0.45f),
        new Color(1f, 0.85f, 0.3f)
    };

    [Header("Layout")]
    [SerializeField] private int sortingOrder = 11;
    [Tooltip("Height of the tag above a character, in units.")]
    [SerializeField] private float tagHeight = 0.85f;

    [Header("Text")]
    [SerializeField] private string playerTag = "P{number}";
    [SerializeField] private string joinedNotice = "P{number} joined as {character}!";
    [SerializeField] private string leftNotice = "P{number} left.";
    [SerializeField] private string nobodyFreeNotice = "No party member to play.";
    [SerializeField] private float noticeTime = 2.5f;

    private CanvasScaler sceneScaler = null;
    private Text noticeText = null;
    private float noticeTimer = 0f;
    private readonly Dictionary<int, Transform> tags = new Dictionary<int, Transform>();

    /// <summary>
    /// Create a CoopUI with the default look.
    /// </summary>
    public static CoopUI Create()
    {
        return new GameObject("Coop UI").AddComponent<CoopUI>();
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
    }

    private void OnEnable()
    {
        LocalCoop.onPlayerJoined += HandlePlayerJoined;
        LocalCoop.onPlayerLeft += HandlePlayerLeft;
    }

    private void OnDisable()
    {
        LocalCoop.onPlayerJoined -= HandlePlayerJoined;
        LocalCoop.onPlayerLeft -= HandlePlayerLeft;
    }

    private void LateUpdate()
    {
        UpdateTags();

        if (noticeTimer > 0f)
        {
            noticeTimer -= Time.unscaledDeltaTime;
            if (noticeTimer <= 0f && noticeText != null)
            {
                noticeText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowNotice(string a_text, Color a_color)
    {
        if (noticeText == null || string.IsNullOrEmpty(a_text))
        {
            return;
        }

        noticeText.text = a_text;
        noticeText.color = a_color;
        noticeText.gameObject.SetActive(true);
        noticeTimer = noticeTime;
    }

    public void ShowNobodyFree()
    {
        ShowNotice(nobodyFreeNotice, Color.white);
    }

    public Color GetPlayerColor(int a_number)
    {
        if (playerColors == null || playerColors.Length == 0)
        {
            return Color.white;
        }
        return playerColors[Mathf.Clamp(a_number - 1, 0, playerColors.Length - 1)];
    }

    private void HandlePlayerJoined(CoopPlayer a_player)
    {
        ShowNotice(Format(joinedNotice, a_player), GetPlayerColor(a_player.Number));
    }

    private void HandlePlayerLeft(CoopPlayer a_player)
    {
        ShowNotice(Format(leftNotice, a_player), GetPlayerColor(a_player.Number));
    }

    private static string Format(string a_template, CoopPlayer a_player)
    {
        string character = a_player.Character != null ? a_player.Character.DisplayName : "";
        return a_template.Replace("{number}", a_player.Number.ToString()).Replace("{character}", character);
    }

    // Tags only show while more than one person plays, and not during cutscenes.
    private void UpdateTags()
    {
        PartyManager party = PartyManager.instance;
        PlayerController leader = party != null ? party.Leader : null;
        bool show = LocalCoop.IsActive && leader != null && leader.currentPlayerState != PlayerState.cutscene;

        UpdateTag(1, show ? leader.transform : null);
        for (int number = 2; number <= LocalCoop.MaxPlayers; number++)
        {
            Transform target = null;
            if (show)
            {
                CoopPlayer player = FindPlayer(number);
                Follower follower = player != null ? party.GetFollower(player.Character) : null;
                if (follower != null && follower.IsControlled)
                {
                    target = follower.transform;
                }
            }
            UpdateTag(number, target);
        }
    }

    private static CoopPlayer FindPlayer(int a_number)
    {
        foreach (CoopPlayer player in LocalCoop.Players)
        {
            if (player.Number == a_number)
            {
                return player;
            }
        }
        return null;
    }

    private void UpdateTag(int a_number, Transform a_target)
    {
        tags.TryGetValue(a_number, out Transform tag);
        if (a_target == null)
        {
            if (tag != null)
            {
                tag.gameObject.SetActive(false);
            }
            return;
        }

        // The tag is a child of the character, it gets destroyed with them.
        if (tag == null)
        {
            tag = CreateTag(a_number);
        }

        if (tag.parent != a_target)
        {
            tag.SetParent(a_target, false);
            tag.localPosition = new Vector3(0f, tagHeight, 0f);
        }
        tag.gameObject.SetActive(true);
    }

    private Transform CreateTag(int a_number)
    {
        GameObject tagObject = new GameObject("P" + a_number + " Tag", typeof(RectTransform));
        Canvas canvas = tagObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = sortingOrder;

        // One canvas unit is one pixel of the game.
        RectTransform rect = tagObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(32f, 16f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.localScale = Vector3.one / GetPixelsPerUnit();

        Text text = CreateText(rect, TextAnchor.LowerCenter);
        text.text = playerTag.Replace("{number}", a_number.ToString());
        text.color = GetPlayerColor(a_number);

        tags[a_number] = rect;
        return rect;
    }

    private float GetPixelsPerUnit()
    {
        PlayerController leader = PartyManager.instance != null ? PartyManager.instance.Leader : null;
        if (leader != null && leader.spriteRenderer != null && leader.spriteRenderer.sprite != null)
        {
            return leader.spriteRenderer.sprite.pixelsPerUnit;
        }
        return 32f;
    }

    // Copy the font and scaling of the dialogue box.
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
            sceneScaler = dialogueManager.dialogueBox.GetComponentInParent<CanvasScaler>();
        }

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        if (fontSize <= 0)
        {
            fontSize = 8;
        }
    }

    private void BuildUI()
    {
        GameObject canvasObject = new GameObject("Coop Canvas", typeof(RectTransform));
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

        // Top right, the quest tracker is on the left.
        RectTransform noticeRect = new GameObject("Notice", typeof(RectTransform)).GetComponent<RectTransform>();
        noticeRect.SetParent(canvasObject.transform, false);
        noticeRect.anchorMin = Vector2.one;
        noticeRect.anchorMax = Vector2.one;
        noticeRect.pivot = Vector2.one;
        noticeRect.anchoredPosition = new Vector2(-6f, -6f);
        noticeRect.sizeDelta = new Vector2(160f, 16f);

        noticeText = CreateText(noticeRect, TextAnchor.UpperRight);
        noticeText.gameObject.SetActive(false);
    }

    private Text CreateText(RectTransform a_parent, TextAnchor a_alignment)
    {
        RectTransform rect = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline)).GetComponent<RectTransform>();
        rect.SetParent(a_parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text text = rect.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = a_alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.supportRichText = false;
        text.raycastTarget = false;

        Outline outline = rect.GetComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(1f, -1f);
        return text;
    }
}
