using System.Collections;
using UnityEngine;

// Here we manage the audio playback for background music in the game.
// This script allows us to play and stop background tracks which will be used in the Timeline system.
public class AudioManager : MonoBehaviour
{
  [SerializeField] private AudioSource audioSource;
  
  public void PlayBackgroundTrack(AudioClip clip)
  {
    if (clip == null || audioSource == null)
    {
      Debug.LogWarning("Audio clip or AudioSource is not set.");
      return;
    }
    
    audioSource.clip = clip;
    audioSource.loop = true;
    audioSource.Play();
  }

  public void StopBackgroundTrack() {
    audioSource.Stop();
  }
  
  public void TriggerAudioFadeOut(float fadeDuration) {
    StartCoroutine(FadeOutAndStop(audioSource, fadeDuration));
  }

  private IEnumerator FadeOutAndStop(AudioSource source, float duration) {
    if (audioSource == null || !audioSource.isPlaying) yield break;

    float startVolume = audioSource.volume;
    float time = 0f;

    while (time < duration) {
      time += Time.deltaTime;
      audioSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
      yield return null;
    }

    audioSource.Stop();
    audioSource.volume = startVolume;
  }

}
