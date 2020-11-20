using UnityEngine;

[CreateAssetMenu(fileName = "Talk Quest", menuName = "Quest/TalkQuest", order = 0)]
public class TalkQuest : Quest
{
    [Header("ID of the NPC to talk to.")]
    public string NPC_ID;

    [Header("How often required to talk.")]
    public int speakAmount = 1;

    public DialogueBase completeDialogue;

    // Current speaking count.
    private int speakCount = 0;

    private const int MIN_TALK_AMOUNT = 1;

    private void OnValidate()
    {
        if (speakAmount < MIN_TALK_AMOUNT)
        {
            speakAmount = MIN_TALK_AMOUNT;
        }
    }

    // Subscribe to notify about this event.
    public override void AcceptQuest()
    {
        completed = false;
        speakCount = 0;
        DialogueManager.onSpokenWithNPC += UpdateQuestProgress;
    }

    private void UpdateQuestProgress(string a_npc_id)
    {
        if (a_npc_id == NPC_ID)
        {
            speakCount++;

            if (speakCount == speakAmount)
            {
                Complete();
            }
        }
    }

    public override void Complete()
    {
        base.Complete();
        DialogueManager.onSpokenWithNPC -= UpdateQuestProgress;
        
        DialogueManager.instance.OpenDialogue(NPC_ID);
        DialogueManager.instance.EnqueueDialogue(completeDialogue);
        DialogueManager.instance.EnqueueDialogue(completeDialogue);
        DialogueManager.instance.EnqueueDialogue(completeDialogue);
    }
}
