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
        npc.LookAtPlayer(PlayerState.GetPosition);
    }
}


