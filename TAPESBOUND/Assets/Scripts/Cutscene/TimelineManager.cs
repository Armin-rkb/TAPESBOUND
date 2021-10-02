using UnityEngine;
using UnityEngine.Playables;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager instance;
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

    [SerializeField]
    private PlayableDirector playableDirector = null;

    public void PauseTimeline()
    {
        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
    }

    public void ResumeTimeline()
    {
        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
    }    
}
