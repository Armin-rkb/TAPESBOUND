using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{

    [Range(0.1f, 10)] public float intensity;
    [SerializeField] private Transform target = null;
    private float pendingDuration = 0;
    private bool effectActive = false;
    private Vector3 initialPos;

    void Start()
    {
        initialPos = target.localPosition;
    }

    void Update()
    {
        if (pendingDuration > 0 && !effectActive)
        {
            // Enable effects
            StartCoroutine(PlayEffect());
        }
    }
    public void Shake(float duration)
    {
        if (duration > 0)
        {
            pendingDuration += duration;
        }
    }

    IEnumerator PlayEffect()
    {
        effectActive = true;
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < startTime + pendingDuration)
        {
            Vector3 randomPoint = new Vector3(Random.Range(-1f, 1f) * intensity, Random.Range(-1f, 1f) * intensity, target.localPosition.z);
            target.localPosition = randomPoint;
            yield return null;
        }

        pendingDuration = 0f;
        target.localPosition = initialPos;
        effectActive = false;
    }
}
