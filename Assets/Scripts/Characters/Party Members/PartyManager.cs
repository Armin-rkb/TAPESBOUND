using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// The party: who travels with the player. Members walk in line behind the leader like in Earthbound.
/// Other players can play a member with their own controller (Start to join, Select to leave), everybody
/// stays on the same screen.
/// The party itself is static, so it stays the same when another scene loads. Every scene with a player
/// gets a PartyManager, add one yourself to change the settings.
/// </summary>
public class PartyManager : MonoBehaviour
{
    public static PartyManager instance;

    // Earthbound's party is four characters: the leader and three members.
    public const int MaxMembers = 3;

    private static readonly List<CharacterProfile> members = new List<CharacterProfile>();
    private static readonly HashSet<CharacterProfile> joinedBefore = new HashSet<CharacterProfile>();

    // Static Events
    public delegate void OnPartyChanged(CharacterProfile a_character, bool a_hasJoined);
    public static event OnPartyChanged onPartyChanged;

    [Header("Characters that join when this scene loads (once per game).")]
    [SerializeField] private List<CharacterProfile> startingMembers = new List<CharacterProfile>();

    [Header("Walking in line")]
    [Tooltip("Distance between party members in the line, in units.")]
    [SerializeField] private float lineSpacing = 0.5f;
    [Tooltip("Optional, used for members that aren't placed in the scene. Empty = a sprite with an animator.")]
    [SerializeField] private Follower memberPrefab = null;

    [Header("Playing together")]
    [Tooltip("Space kept between a player and the edge of the screen, in units.")]
    [SerializeField] private Vector2 screenMargin = new Vector2(0.5f, 0.75f);

    [Header("Feedback")]
    [Tooltip("Print \"Paula joined the party!\" in the text box.")]
    [SerializeField] private bool announceInDialogue = true;
    [Tooltip("Move the camera so every player fits on the screen (when the camera follows the player).")]
    [SerializeField] private bool createPartyCamera = true;
    [Tooltip("Create the player tags and join messages when the scene doesn't have a CoopUI.")]
    [SerializeField] private bool createCoopUI = true;

    // One pixel, the leader's path is remembered at this detail.
    private const float TrailPointSpacing = 1f / 32f;
    // Moving further than this in one frame is a teleport (cutscene, door), the line starts over.
    private const float TeleportDistance = 1f;

    private PlayerController leader = null;
    private BoxCollider2D leaderCollider = null;
    private readonly List<Follower> followers = new List<Follower>();
    private readonly Dictionary<CharacterProfile, Follower> placedFollowers = new Dictionary<CharacterProfile, Follower>();
    private readonly List<Vector2> trail = new List<Vector2>();
    private readonly List<InputDevice> controllers = new List<InputDevice>();
    private readonly List<Transform> screenPlayers = new List<Transform>();

    public static IReadOnlyList<CharacterProfile> Members => members;
    public PlayerController Leader => leader;
    public IReadOnlyList<Follower> Followers => followers;

    /// <summary>
    /// Co-op players can only move when the leader can: not during dialogue, menus, cutscenes or loading.
    /// </summary>
    public static bool CanPlayersMove => instance != null && instance.leader != null && instance.leader.CanMove;
    public static float MovementSpeed => instance != null && instance.leader != null ? instance.leader.MovementSpeed : 2.5f;

    // Start every play session with an empty party (also when domain reload is disabled).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetAll()
    {
        members.Clear();
        joinedBefore.Clear();
    }

    public static bool IsInParty(CharacterProfile a_character)
    {
        return a_character != null && members.Contains(a_character);
    }

    /// <summary>
    /// Add a character to the party, they show up behind the leader.
    /// </summary>
    public static bool Join(CharacterProfile a_character, bool a_announce = true)
    {
        if (a_character == null || members.Contains(a_character))
        {
            return false;
        }
        if (instance != null && instance.leader != null && instance.leader.playerCharacterProfile == a_character)
        {
            return false;
        }
        if (members.Count >= MaxMembers)
        {
            Debug.LogWarning("The party is full, " + a_character.DisplayName + " can't join.");
            return false;
        }

        members.Add(a_character);
        joinedBefore.Add(a_character);
        if (instance != null)
        {
            instance.AddFollower(a_character);
        }

        if (ShouldAnnounce(a_announce))
        {
            DialogueManager.ShowSystemMessage(SystemMessages.Format(DialogueManager.Messages.partyJoined, a_character: a_character));
        }
        onPartyChanged?.Invoke(a_character, true);
        return true;
    }

    /// <summary>
    /// Remove a character from the party. Whoever played them goes back to watching.
    /// </summary>
    public static bool Leave(CharacterProfile a_character, bool a_announce = true)
    {
        if (a_character == null || !members.Remove(a_character))
        {
            return false;
        }

        LocalCoop.Leave(LocalCoop.GetPlayer(a_character));
        if (instance != null)
        {
            instance.RemoveFollower(a_character);
        }

        if (ShouldAnnounce(a_announce))
        {
            DialogueManager.ShowSystemMessage(SystemMessages.Format(DialogueManager.Messages.partyLeft, a_character: a_character));
        }
        onPartyChanged?.Invoke(a_character, false);
        return true;
    }

    private static bool ShouldAnnounce(bool a_announce)
    {
        return a_announce && (instance == null || instance.announceInDialogue);
    }

    // Every scene with a player gets a PartyManager, so the party follows you everywhere.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AddToEveryScene()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        EnsureInScene();
    }

    private static void HandleSceneLoaded(Scene a_scene, LoadSceneMode a_mode)
    {
        EnsureInScene();
    }

    private static void EnsureInScene()
    {
        if (instance != null || FindAnyObjectByType<PartyManager>() != null)
        {
            return;
        }

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.gameObject.AddComponent<PartyManager>();
        }
    }

    private void Awake()
    {
        // The newest one wins, the old scene may not be unloaded yet.
        instance = this;

        leader = GetComponent<PlayerController>();
        if (leader == null)
        {
            leader = FindAnyObjectByType<PlayerController>();
        }
        if (leader != null)
        {
            leaderCollider = leader.GetComponent<BoxCollider2D>();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void OnEnable()
    {
        LocalCoop.ListenForDevices();
        LocalCoop.onPlayerJoined += HandlePlayerJoined;
        LocalCoop.onPlayerLeft += HandlePlayerLeft;
    }

    private void OnDisable()
    {
        LocalCoop.onPlayerJoined -= HandlePlayerJoined;
        LocalCoop.onPlayerLeft -= HandlePlayerLeft;
    }

    private void Start()
    {
        ResetTrail();
        CollectPlacedFollowers();

        // Members from earlier scenes first, so the line keeps its order.
        foreach (CharacterProfile member in members)
        {
            AddFollower(member);
        }
        foreach (CharacterProfile character in startingMembers)
        {
            if (character != null && !joinedBefore.Contains(character))
            {
                Join(character, false);
            }
        }

        // Players who joined in another scene keep playing their character.
        foreach (CoopPlayer player in new List<CoopPlayer>(LocalCoop.Players))
        {
            Follower follower = GetFollower(player.Character);
            if (follower != null)
            {
                TakeControl(follower, player);
            }
            else
            {
                LocalCoop.Leave(player);
            }
        }

        if (createPartyCamera && leader != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.transform.IsChildOf(leader.transform))
            {
                PartyCamera.Attach(mainCamera, leader.transform);
            }
        }

        if (createCoopUI && CoopUI.instance == null)
        {
            CoopUI.Create();
        }
    }

    private void Update()
    {
        PollControllers();
    }

    private void LateUpdate()
    {
        if (leader == null)
        {
            return;
        }

        RecordTrail();
        followers.RemoveAll(a_follower => a_follower == null);

        // Members nobody plays walk in line, in party order.
        int slot = 0;
        foreach (Follower follower in followers)
        {
            if (follower.IsControlled)
            {
                KeepNearLeader(follower);
                continue;
            }

            slot++;
            follower.FollowTo(GetLinePosition(slot), Time.deltaTime);
        }
    }

    public Follower GetFollower(CharacterProfile a_character)
    {
        return a_character == null ? null : followers.Find(a_follower => a_follower != null && a_follower.characterProfile == a_character);
    }

    /// <summary>
    /// Let a controller play the first party member nobody plays yet.
    /// </summary>
    public CoopPlayer TryJoin(InputDevice a_device)
    {
        foreach (Follower follower in followers)
        {
            if (!follower.IsControlled && LocalCoop.GetPlayer(follower.characterProfile) == null)
            {
                // HandlePlayerJoined gives them the character.
                return LocalCoop.Join(a_device, follower.characterProfile);
            }
        }

        if (CoopUI.instance != null)
        {
            CoopUI.instance.ShowNobodyFree();
        }
        return null;
    }

    /// <summary>
    /// Where the party member in line spot a_slot (1 = right behind the leader) stands.
    /// </summary>
    public Vector2 GetLinePosition(int a_slot)
    {
        Vector2 start = leader != null ? (Vector2)leader.transform.position : (Vector2)transform.position;
        return GetTrailPoint(start, trail, a_slot * lineSpacing);
    }

    /// <summary>
    /// The point a_distance back along a path. a_trail goes from new to old, a_start is the newest point.
    /// Stops at the end of the path.
    /// </summary>
    public static Vector2 GetTrailPoint(Vector2 a_start, IList<Vector2> a_trail, float a_distance)
    {
        Vector2 previous = a_start;
        float remaining = a_distance;
        foreach (Vector2 point in a_trail)
        {
            float segment = Vector2.Distance(previous, point);
            if (segment > 0f && segment >= remaining)
            {
                return Vector2.Lerp(previous, point, remaining / segment);
            }
            remaining -= segment;
            previous = point;
        }
        return previous;
    }

    /// <summary>
    /// Everybody the camera keeps on screen: the leader and the members other players play.
    /// </summary>
    public List<Transform> GetScreenPlayers()
    {
        screenPlayers.Clear();
        if (leader == null)
        {
            return screenPlayers;
        }

        screenPlayers.Add(leader.transform);
        // Cutscenes are about the leader.
        if (leader.currentPlayerState != PlayerState.cutscene)
        {
            foreach (Follower follower in followers)
            {
                if (follower.IsControlled && follower.isActiveAndEnabled)
                {
                    screenPlayers.Add(follower.transform);
                }
            }
        }
        return screenPlayers;
    }

    /// <summary>
    /// The middle of all players, where the camera looks.
    /// </summary>
    public Vector2 GetFocusPoint()
    {
        List<Transform> players = GetScreenPlayers();
        if (players.Count == 0)
        {
            return transform.position;
        }

        Vector2 min = players[0].position;
        Vector2 max = min;
        foreach (Transform player in players)
        {
            min = Vector2.Min(min, player.position);
            max = Vector2.Max(max, player.position);
        }
        return (min + max) * 0.5f;
    }

    /// <summary>
    /// Limit a velocity so all players stay on the same screen. Only does something while others play along.
    /// </summary>
    public static Vector2 KeepOnScreen(Transform a_self, Vector2 a_position, Vector2 a_velocity, float a_deltaTime)
    {
        if (instance == null || !LocalCoop.IsActive || a_deltaTime <= 0f)
        {
            return a_velocity;
        }

        Vector2 span = instance.GetScreenSpan();
        if (span == Vector2.zero)
        {
            return a_velocity;
        }

        bool hasOthers = false;
        Vector2 min = Vector2.positiveInfinity;
        Vector2 max = Vector2.negativeInfinity;
        foreach (Transform player in instance.GetScreenPlayers())
        {
            if (player == a_self)
            {
                continue;
            }
            hasOthers = true;
            min = Vector2.Min(min, player.position);
            max = Vector2.Max(max, player.position);
        }

        return hasOthers ? LimitVelocity(a_position, a_velocity, a_deltaTime, max - span, min + span) : a_velocity;
    }

    /// <summary>
    /// Stop a velocity at the edges of an area. Moving back into the area is always allowed.
    /// </summary>
    public static Vector2 LimitVelocity(Vector2 a_position, Vector2 a_velocity, float a_deltaTime, Vector2 a_min, Vector2 a_max)
    {
        Vector2 next = a_position + a_velocity * a_deltaTime;
        if (a_velocity.x > 0f && next.x > a_max.x)
        {
            a_velocity.x = Mathf.Max(0f, (a_max.x - a_position.x) / a_deltaTime);
        }
        else if (a_velocity.x < 0f && next.x < a_min.x)
        {
            a_velocity.x = Mathf.Min(0f, (a_min.x - a_position.x) / a_deltaTime);
        }

        if (a_velocity.y > 0f && next.y > a_max.y)
        {
            a_velocity.y = Mathf.Max(0f, (a_max.y - a_position.y) / a_deltaTime);
        }
        else if (a_velocity.y < 0f && next.y < a_min.y)
        {
            a_velocity.y = Mathf.Min(0f, (a_min.y - a_position.y) / a_deltaTime);
        }
        return a_velocity;
    }

    // How far apart players can be and still all fit on the screen.
    private Vector2 GetScreenSpan()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic)
        {
            return Vector2.zero;
        }

        float height = mainCamera.orthographicSize * 2f;
        Vector2 view = new Vector2(height * mainCamera.aspect, height);
        return Vector2.Max(view - screenMargin * 2f, Vector2.zero);
    }

    private void PollControllers()
    {
        foreach (InputDevice controller in ControllerInput.GetControllers(controllers))
        {
            ControlState controls = ControllerInput.Read(controller);
            CoopPlayer player = LocalCoop.GetPlayer(controller);
            if (player != null)
            {
                if (controls.leave)
                {
                    LocalCoop.Leave(player);
                }
            }
            else if (controls.join && controller != LocalCoop.LeaderDevice)
            {
                TryJoin(controller);
            }
        }
    }

    private void HandlePlayerJoined(CoopPlayer a_player)
    {
        Follower follower = GetFollower(a_player.Character);
        if (follower != null)
        {
            TakeControl(follower, a_player);
        }
    }

    private void HandlePlayerLeft(CoopPlayer a_player)
    {
        Follower follower = GetFollower(a_player.Character);
        if (follower != null && follower.Controller == a_player)
        {
            follower.SetController(null);
        }
    }

    private void TakeControl(Follower a_follower, CoopPlayer a_player)
    {
        a_follower.SetController(a_player, leaderCollider);
        if (leader == null)
        {
            return;
        }

        // Same layer as the leader, so walls stop them the same way.
        a_follower.gameObject.layer = leader.gameObject.layer;

        // Party members walk through each other, like in Earthbound.
        Collider2D body = a_follower.BodyCollider;
        foreach (Collider2D collider in leader.GetComponents<Collider2D>())
        {
            Physics2D.IgnoreCollision(body, collider);
        }
        foreach (Follower other in followers)
        {
            if (other != a_follower && other.BodyCollider != null)
            {
                Physics2D.IgnoreCollision(body, other.BodyCollider);
            }
        }
    }

    // After a cutscene or a teleport a player can end up far away, bring them back behind the leader.
    private void KeepNearLeader(Follower a_follower)
    {
        if (!CanPlayersMove)
        {
            return;
        }

        Vector2 span = GetScreenSpan();
        if (span == Vector2.zero)
        {
            return;
        }

        Vector2 offset = a_follower.transform.position - leader.transform.position;
        if (Mathf.Abs(offset.x) > span.x + 1f || Mathf.Abs(offset.y) > span.y + 1f)
        {
            a_follower.Warp(GetLinePosition(1));
        }
    }

    private void RecordTrail()
    {
        Vector2 position = leader.transform.position;
        if (trail.Count == 0)
        {
            trail.Add(position);
            return;
        }

        float distance = Vector2.Distance(position, trail[0]);
        if (distance > TeleportDistance)
        {
            ResetTrail();
            return;
        }
        if (distance >= TrailPointSpacing)
        {
            trail.Insert(0, position);
        }

        // Forget the path further back than the last member.
        float maxLength = lineSpacing * (MaxMembers + 1) + 1f;
        float length = Vector2.Distance(position, trail[0]);
        for (int i = 1; i < trail.Count; i++)
        {
            length += Vector2.Distance(trail[i - 1], trail[i]);
            if (length > maxLength)
            {
                trail.RemoveRange(i + 1, trail.Count - i - 1);
                break;
            }
        }
    }

    private void ResetTrail()
    {
        trail.Clear();
        if (leader != null)
        {
            trail.Add(leader.transform.position);
        }
    }

    // Party members placed in the scene are used for their character, and hidden until that character joins.
    private void CollectPlacedFollowers()
    {
        foreach (Follower follower in FindObjectsByType<Follower>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (follower.characterProfile == null || follower.gameObject.scene != gameObject.scene || followers.Contains(follower))
            {
                continue;
            }

            if (!placedFollowers.ContainsKey(follower.characterProfile))
            {
                placedFollowers.Add(follower.characterProfile, follower);
            }
            follower.gameObject.SetActive(false);
        }
    }

    private Follower AddFollower(CharacterProfile a_character)
    {
        Follower follower = GetFollower(a_character);
        if (follower != null)
        {
            return follower;
        }

        if (placedFollowers.TryGetValue(a_character, out follower) && follower != null)
        {
            follower.gameObject.SetActive(true);
        }
        else
        {
            follower = CreateFollower(a_character);
        }

        followers.Add(follower);
        followers.Sort((a_first, a_second) => members.IndexOf(a_first.characterProfile).CompareTo(members.IndexOf(a_second.characterProfile)));

        // New members appear behind the leader.
        follower.Warp(GetLinePosition(followers.IndexOf(follower) + 1));
        return follower;
    }

    private void RemoveFollower(CharacterProfile a_character)
    {
        Follower follower = GetFollower(a_character);
        if (follower == null)
        {
            return;
        }

        followers.Remove(follower);
        follower.SetController(null);
        if (placedFollowers.ContainsValue(follower))
        {
            follower.gameObject.SetActive(false);
        }
        else
        {
            Destroy(follower.gameObject);
        }
    }

    private Follower CreateFollower(CharacterProfile a_character)
    {
        Follower follower;
        if (memberPrefab != null)
        {
            follower = Instantiate(memberPrefab);
            follower.name = a_character.DisplayName;
            follower.SetCharacter(a_character);
        }
        else
        {
            // Inactive while building, so Awake sees the character.
            GameObject body = new GameObject(a_character.DisplayName);
            body.SetActive(false);

            SpriteRenderer spriteRenderer = body.AddComponent<SpriteRenderer>();
            if (leader != null && leader.spriteRenderer != null)
            {
                spriteRenderer.sortingLayerID = leader.spriteRenderer.sortingLayerID;
                spriteRenderer.sortingOrder = leader.spriteRenderer.sortingOrder;
            }
            body.AddComponent<Animator>();
            follower = body.AddComponent<Follower>();
            follower.characterProfile = a_character;
            body.SetActive(true);
        }

        if (leader != null && follower.gameObject.scene != leader.gameObject.scene)
        {
            SceneManager.MoveGameObjectToScene(follower.gameObject, leader.gameObject.scene);
        }
        return follower;
    }
}
