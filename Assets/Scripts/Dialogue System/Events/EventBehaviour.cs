using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Event")]
public class EventBehaviour : ScriptableObject
{
    public void TestEvent()
    {
        Debug.Log("Test event successful");
    }

    public void LookAtPlayer(string a_targetNPC_id)
    {
        GameObject target = ObjectId.Find(a_targetNPC_id);
        NPC npc = target != null ? target.GetComponent<NPC>() : null;
        if (npc != null)
        {
            npc.LookAtPlayer(PlayerController.GetPosition);
        }
    }

    public void GiveItemToPlayer(Item a_item)
    {
        PlayerInventory playerInventory = PlayerInventory.Current;
        if (playerInventory != null)
        {
            playerInventory.AddToInventory(a_item);
        }
    }

    public void AddDialogue(DialogueBase a_dialogue)
    {
        DialogueManager.instance.EnqueueDialogue(a_dialogue);
    }

    // Quest & flag helpers, so UnityEvents (timeline signals, buttons) can use them too.
    public void StartQuest(Quest a_quest)
    {
        QuestManager.StartQuest(a_quest);
    }

    public void CompleteQuest(Quest a_quest)
    {
        QuestManager.CompleteQuest(a_quest);
    }

    public void SetFlag(string a_flag)
    {
        GameFlags.Set(a_flag);
    }

    public void ClearFlag(string a_flag)
    {
        GameFlags.Clear(a_flag);
    }

    public void GiveMoney(int a_amount)
    {
        DialogueCommand.GiveMoney(a_amount, false);
    }
}


