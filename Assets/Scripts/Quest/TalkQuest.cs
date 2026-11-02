using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shortcut for a quest that only needs you to talk to one NPC.
/// A regular Quest with a TalkTo objective does the same thing.
/// </summary>
[CreateAssetMenu(fileName = "Talk Quest", menuName = "Quest/TalkQuest", order = 1)]
public class TalkQuest : Quest
{
    [Header("ID of the NPC to talk to.")]
    public string NPC_ID;

    [Header("How often required to talk.")]
    public int speakAmount = 1;

    private const int MIN_TALK_AMOUNT = 1;

    private List<QuestObjective> talkObjectives = null;

    private void OnValidate()
    {
        if (speakAmount < MIN_TALK_AMOUNT)
        {
            speakAmount = MIN_TALK_AMOUNT;
        }
        talkObjectives = null;
    }

    // Uses the objectives list when it's filled in, otherwise a TalkTo objective made from NPC_ID.
    public override IReadOnlyList<QuestObjective> Objectives
    {
        get
        {
            if (objectives.Count > 0)
            {
                return objectives;
            }

            if (talkObjectives == null)
            {
                talkObjectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        description = description,
                        type = QuestObjectiveType.TalkTo,
                        target = NPC_ID,
                        requiredAmount = speakAmount
                    }
                };
            }
            return talkObjectives;
        }
    }
}
