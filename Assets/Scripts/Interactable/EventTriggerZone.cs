using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Runs an event when the player walks into this trigger: play a dialogue, run commands (start a quest,
/// set a flag for a "reach this place" objective...) and/or a UnityEvent.
/// Party members played by other players trigger it too, the ones walking in line don't.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EventTriggerZone : MonoBehaviour
{
    [Tooltip("All conditions must be true to trigger.")]
    [SerializeField] private List<DialogueCondition> conditions = new List<DialogueCondition>();

    [Header("What happens")]
    [SerializeField] private DialogueBase dialogue = null;
    [SerializeField] private List<DialogueCommand> commands = new List<DialogueCommand>();
    [SerializeField] private UnityEvent onTriggered = null;

    [Header("Only once")]
    [Tooltip("Set when triggered, the zone won't trigger again while this flag is set. Empty = every time.")]
    [SerializeField] private string onceFlag = "";

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        if (a_collision == null || !IsPlayer(a_collision))
        {
            return;
        }

        if (GameFlags.IsSet(onceFlag) || !DialogueCondition.AllMet(conditions))
        {
            return;
        }

        Trigger();
    }

    public void Trigger()
    {
        GameFlags.Set(onceFlag);

        if (dialogue != null && DialogueManager.instance != null)
        {
            DialogueManager.instance.EnqueueDialogue(dialogue);
        }
        DialogueCommand.ExecuteAll(commands);
        onTriggered?.Invoke();
    }

    private static bool IsPlayer(Collider2D a_collision)
    {
        return a_collision.CompareTag(Tags.Player) || (a_collision.TryGetComponent(out Follower follower) && follower.IsControlled);
    }
}
