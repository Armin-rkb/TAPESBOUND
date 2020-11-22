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
        NPC npc = ObjectId.Find(a_targetNPC_id).GetComponent<NPC>();
        npc.LookAtPlayer(PlayerController.GetPosition);
    }

    public void GiveItemToPlayer(Item a_item)
    {
        PlayerInventory playerInventory = FindObjectOfType<PlayerInventory>();
        Debug.Log(playerInventory);
        playerInventory.AddToInventory(a_item);
    }
}


