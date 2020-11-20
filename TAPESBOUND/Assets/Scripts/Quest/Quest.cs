using System.Collections.Generic;
using UnityEngine;

public class Quest : ScriptableObject, IQuest
{
    [SerializeField] protected new string name;
    [SerializeField] protected string description;
    [SerializeField] protected bool completed;
    [SerializeField] protected List<string> questRewards;

    public virtual void AcceptQuest() {}

    public virtual void Complete()
    {
        Debug.Log("Quest:" + name + " completed");
        completed = true;
        GrantReward();
    }
    
    public void GrantReward()
    {
        Debug.Log("Giving rewards:");
        foreach (var reward in questRewards)
        {
            Debug.Log(reward);   
        }
    }
}

public interface IQuest
{
    void AcceptQuest();
    void Complete();
}
