using System.Collections.Generic;
using UnityEngine;

public class Quest : ScriptableObject, IQuest
{
    public new string name;
    public string description;
    public bool completed;
    public List<string> questRewards;

    public virtual void AcceptQuest() {}

    public virtual void Complete()
    {
        Debug.Log("Quest:" + name + " completed");
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
