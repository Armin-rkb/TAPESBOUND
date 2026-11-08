using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A door that only opens after knocking a few times. Every knock plays its sounds and its own dialogue,
/// when the dialogue of the last knock is read On Opened runs. Add or remove knocks to change how many it takes.
/// </summary>
public class KnockingDoor : Interactable
{
    [System.Serializable]
    public class Knock
    {
        [Tooltip("Played one after the other.")]
        public AudioClip[] sounds = null;
        public DialogueBase dialogue = null;
    }

    [SerializeField] private Knock[] knocks = null;
    [SerializeField] private UnityEvent onOpened = null;

    private int knockCount = 0;

    public int KnockCount => knockCount;

    public override void Interact()
    {
        base.Interact();

        if (knocks == null || knockCount >= knocks.Length)
        {
            return;
        }

        Knock knock = knocks[knockCount];
        knockCount++;

        if (knockCount == knocks.Length)
        {
            DialogueManager.onDialogueExit += HandleLastKnockRead;
        }

        if (isActiveAndEnabled)
        {
            StartCoroutine(PlaySounds(knock.sounds));
        }
        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.EnqueueDialogue(knock.dialogue);
        }
    }

    private void OnDestroy()
    {
        DialogueManager.onDialogueExit -= HandleLastKnockRead;
    }

    private void HandleLastKnockRead()
    {
        DialogueManager.onDialogueExit -= HandleLastKnockRead;
        onOpened?.Invoke();
    }

    private static IEnumerator PlaySounds(AudioClip[] a_sounds)
    {
        if (a_sounds == null)
        {
            yield break;
        }

        foreach (AudioClip sound in a_sounds)
        {
            if (sound != null)
            {
                DialogueManager.PlaySound(sound);
                yield return new WaitForSeconds(sound.length);
            }
        }
    }
}
