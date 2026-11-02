using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global story flags, like the event flags Earthbound uses to remember what happened.
/// Flags are plain strings ("met_shoenice", "door_unlocked") and survive scene loads.
/// </summary>
public static class GameFlags
{
    private static readonly HashSet<string> flags = new HashSet<string>();

    // Static Events
    public delegate void OnFlagChanged(string a_flag, bool a_isSet);
    public static event OnFlagChanged onFlagChanged;

    public static IEnumerable<string> All => flags;

    // Start every play session with a clean slate (also when domain reload is disabled).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void ClearAll()
    {
        flags.Clear();
    }

    public static bool IsSet(string a_flag)
    {
        return !string.IsNullOrEmpty(a_flag) && flags.Contains(a_flag);
    }

    public static void Set(string a_flag, bool a_value = true)
    {
        if (string.IsNullOrEmpty(a_flag))
        {
            return;
        }

        bool changed = a_value ? flags.Add(a_flag) : flags.Remove(a_flag);
        if (changed)
        {
            onFlagChanged?.Invoke(a_flag, a_value);
        }
    }

    public static void Clear(string a_flag)
    {
        Set(a_flag, false);
    }
}
