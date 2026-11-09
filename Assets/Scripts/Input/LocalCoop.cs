using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Someone playing along on the same screen with their own controller, as one of the party members.
/// </summary>
public class CoopPlayer
{
    // 2, 3 or 4. Player 1 always plays the leader.
    public int Number { get; }
    public InputDevice Device { get; }
    public CharacterProfile Character { get; }

    public CoopPlayer(int a_number, InputDevice a_device, CharacterProfile a_character)
    {
        Number = a_number;
        Device = a_device;
        Character = a_character;
    }

    public ControlState ReadControls()
    {
        return ControllerInput.Read(Device);
    }
}

/// <summary>
/// Keeps track of who plays with which controller.
/// Player 1 plays the leader with the keyboard and the first controller they use. Any other controller
/// joins with Start and plays a party member, Select leaves again.
/// Static, so everybody stays in when another scene loads.
/// </summary>
public static class LocalCoop
{
    public const int MaxPlayers = 4;

    private static readonly List<CoopPlayer> players = new List<CoopPlayer>();
    private static InputDevice leaderDevice = null;
    private static bool isListening = false;

    // Static Events
    public delegate void OnPlayerChanged(CoopPlayer a_player);
    public static event OnPlayerChanged onPlayerJoined;
    public static event OnPlayerChanged onPlayerLeft;

    /// <summary>
    /// Players 2 to 4, sorted by number.
    /// </summary>
    public static IReadOnlyList<CoopPlayer> Players => players;
    public static bool IsActive => players.Count > 0;

    /// <summary>
    /// The controller of player 1, null while they only use the keyboard.
    /// </summary>
    public static InputDevice LeaderDevice => leaderDevice;

    // Start every play session with nobody joined (also when domain reload is disabled).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ResetAll()
    {
        players.Clear();
        leaderDevice = null;
    }

    /// <summary>
    /// Drop players whose controller gets unplugged.
    /// </summary>
    public static void ListenForDevices()
    {
        if (!isListening)
        {
            isListening = true;
            InputSystem.onDeviceChange += HandleDeviceChange;
        }
    }

    public static CoopPlayer GetPlayer(InputDevice a_device)
    {
        return a_device == null ? null : players.Find(a_player => a_player.Device == a_device);
    }

    public static CoopPlayer GetPlayer(CharacterProfile a_character)
    {
        return a_character == null ? null : players.Find(a_player => a_player.Character == a_character);
    }

    public static bool IsInUse(InputDevice a_device)
    {
        return a_device != null && (a_device == leaderDevice || GetPlayer(a_device) != null);
    }

    /// <summary>
    /// Give player 1 a controller. Called when they first use one.
    /// </summary>
    public static void SetLeaderDevice(InputDevice a_device)
    {
        if (a_device != null && GetPlayer(a_device) != null)
        {
            return;
        }
        leaderDevice = a_device;
    }

    /// <summary>
    /// Let a controller play a party member. Returns null when the controller or character is already taken.
    /// </summary>
    public static CoopPlayer Join(InputDevice a_device, CharacterProfile a_character)
    {
        if (a_device == null || a_character == null || IsInUse(a_device) || GetPlayer(a_character) != null || players.Count >= MaxPlayers - 1)
        {
            return null;
        }

        CoopPlayer player = new CoopPlayer(GetFreeNumber(), a_device, a_character);
        players.Add(player);
        players.Sort((a_first, a_second) => a_first.Number.CompareTo(a_second.Number));

        GameAudio.Play(GameSound.PartyJoin);
        onPlayerJoined?.Invoke(player);
        return player;
    }

    public static void Leave(CoopPlayer a_player)
    {
        if (a_player == null || !players.Remove(a_player))
        {
            return;
        }

        GameAudio.Play(GameSound.PartyLeave);
        onPlayerLeft?.Invoke(a_player);
    }

    private static int GetFreeNumber()
    {
        int number = 2;
        while (players.Exists(a_player => a_player.Number == number))
        {
            number++;
        }
        return number;
    }

    private static void HandleDeviceChange(InputDevice a_device, InputDeviceChange a_change)
    {
        if (a_change != InputDeviceChange.Removed && a_change != InputDeviceChange.Disconnected)
        {
            return;
        }

        if (a_device == leaderDevice)
        {
            leaderDevice = null;
        }
        Leave(GetPlayer(a_device));
    }
}
