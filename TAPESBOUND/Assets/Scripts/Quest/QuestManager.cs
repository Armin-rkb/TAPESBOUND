using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Create instance!");
        }
        else
        {
            instance = this;
        }
    }

    // Static Events
    public delegate void OnQuestAccepted();
    public static event OnQuestAccepted onQuestAccepted;
    public delegate void OnQuestCompleted();
    public static event OnQuestCompleted onQuestCompleted;

    [SerializeField] private List<Quest> activeQuests = new List<Quest>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (var quest in activeQuests)
        {
            quest.AcceptQuest();
        }
    }

    private void AddNewQuest(Quest a_quest)
    {
        activeQuests.Add(a_quest);
        onQuestAccepted?.Invoke();
    }
}