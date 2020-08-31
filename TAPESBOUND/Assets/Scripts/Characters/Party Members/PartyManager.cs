using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    private GameObject partyLeader = null;
    private List<Follower> partyMembers = null;

    public void AddPartyMember(Follower a_newMember)
    {
        if (a_newMember != null)
        {
            a_newMember.leader = partyLeader;
            partyMembers.Add(a_newMember);
        }
    }
}
