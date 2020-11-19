using UnityEngine;

[CreateAssetMenu(fileName = "Talk Quest", menuName = "Quest/TalkQuest", order = 0)]
public class TalkQuest : Quest
{
    [Header("ID of the NPC to talk to.")]
    public string NPC_ID;

    [Header("How often required to talk.")]
    public int speakAmount = 1;

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
        GameObject npc = ObjectId.Find(NPC_ID);
        Debug.Log("Found npc: " + npc.name);
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
        Debug.Log("We completed the quest! but was base called twice?");
        DialogueManager.onSpokenWithNPC -= UpdateQuestProgress;

    }
}
