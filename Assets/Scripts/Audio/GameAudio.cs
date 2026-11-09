using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Every name matches a clip in Assets/Resources/GameAudio. Replace a file there to change the sound.
public enum GameSound
{
    Cursor,
    Confirm,
    MenuOpen,
    MenuClose,
    TextBlip,
    TextAdvance,
    DialogueOpen,
    DialogueClose,
    Error,
    ItemGet,
    ItemGive,
    MoneyGet,
    MoneyPay,
    QuestStart,
    QuestUpdate,
    QuestComplete,
    PartyJoin,
    PartyLeave,
    Door,
    Transition,
    StepWood,
    StepStreet,
    StepMetal
}

/// <summary>
/// The sounds every scene shares: interface, rewards and footsteps. Creates itself, so scenes need no setup.
/// </summary>
public class GameAudio : MonoBehaviour
{
    public const float StepInterval = 0.3f;

    private const float RepeatGuard = 0.05f;
    private const float TextBlipInterval = 0.06f;
    private const float OverlapWindow = 0.12f;

    private static GameAudio instance;

    private readonly Dictionary<GameSound, AudioClip> clips = new Dictionary<GameSound, AudioClip>();
    private readonly Dictionary<GameSound, float> lastPlayed = new Dictionary<GameSound, float>();
    private AudioSource effects = null;
    private AudioSource footsteps = null;
    private float lastInterfaceSound = float.NegativeInfinity;
    private float lastJingle = float.NegativeInfinity;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        GameObject root = new GameObject("GameAudio");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<GameAudio>();

        // Automated runs stay silent.
        if (Application.isBatchMode)
        {
            AudioListener.volume = 0f;
        }
    }

    private void Awake()
    {
        effects = CreateSource();
        footsteps = CreateSource();
    }

    public static void Play(GameSound a_sound)
    {
        if (instance != null)
        {
            instance.PlaySound(a_sound);
        }
    }

    /// <summary>
    /// Play a_clip when it is set, otherwise the shared sound.
    /// </summary>
    public static void Play(AudioClip a_clip, GameSound a_fallback)
    {
        if (a_clip != null)
        {
            DialogueManager.PlaySound(a_clip);
        }
        else
        {
            Play(a_fallback);
        }
    }

    /// <summary>
    /// One footstep on the floor of the current scene.
    /// </summary>
    public static void PlayStep()
    {
        if (instance != null)
        {
            instance.footsteps.pitch = Random.Range(0.88f, 1.12f);
            instance.PlaySound(GetStepSound(SceneManager.GetActiveScene().name), instance.footsteps);
        }
    }

    public static GameSound GetStepSound(string a_sceneName)
    {
        if (a_sceneName.Contains("City"))
        {
            return GameSound.StepStreet;
        }
        return a_sceneName.Contains("Train") ? GameSound.StepMetal : GameSound.StepWood;
    }

    private void PlaySound(GameSound a_sound, AudioSource a_source = null)
    {
        float now = Time.unscaledTime;
        float minimumGap = a_sound == GameSound.TextBlip ? TextBlipInterval : RepeatGuard;
        if (lastPlayed.TryGetValue(a_sound, out float last) && now - last < minimumGap)
        {
            return;
        }

        // Moving the cursor into place when a menu opens is not a cursor move.
        if (a_sound == GameSound.Cursor && now - lastInterfaceSound < OverlapWindow)
        {
            return;
        }

        // One event can hand out several rewards at once, only the first jingle plays.
        if (IsJingle(a_sound))
        {
            if (now - lastJingle < OverlapWindow)
            {
                return;
            }
            lastJingle = now;
        }

        if (a_sound == GameSound.MenuOpen || a_sound == GameSound.MenuClose || a_sound == GameSound.Confirm)
        {
            lastInterfaceSound = now;
        }

        AudioClip clip = GetClip(a_sound);
        if (clip != null)
        {
            lastPlayed[a_sound] = now;
            (a_source != null ? a_source : effects).PlayOneShot(clip);
        }
    }

    private static bool IsJingle(GameSound a_sound)
    {
        switch (a_sound)
        {
            case GameSound.ItemGet:
            case GameSound.ItemGive:
            case GameSound.MoneyGet:
            case GameSound.MoneyPay:
            case GameSound.QuestStart:
            case GameSound.QuestUpdate:
            case GameSound.QuestComplete:
            case GameSound.PartyJoin:
            case GameSound.PartyLeave:
                return true;
            default:
                return false;
        }
    }

    private AudioClip GetClip(GameSound a_sound)
    {
        if (!clips.TryGetValue(a_sound, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>("GameAudio/" + a_sound);
            if (clip == null)
            {
                Debug.LogWarning("Missing sound: Resources/GameAudio/" + a_sound);
            }
            clips[a_sound] = clip;
        }
        return clip;
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }
}
