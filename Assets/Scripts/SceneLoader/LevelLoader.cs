using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator uiAnimator;

    public int level = 0;

    private float transitionTime = 3f;
    private bool isLoading = false;

    void OnTriggerEnter2D(Collider2D a_collider)
    {
        LoadToScene();
    }

    public void LoadToScene()
    {
        // UGLY!
        // Todo: Need to pause playermovement and set all npc's to idle state.
        GameObject.Find("Player").GetComponent<PlayerMovement>().enabled = false;
        GameObject.Find("Player").GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        GameObject.Find("Player").GetComponent<Animator>().enabled = false;
        
        if (!isLoading)
        {
            isLoading = true;
            GameAudio.Play(GameSound.Transition);
        }

        StartCoroutine(LoadLevel(level));
    }

    IEnumerator LoadLevel(int a_levelIndex)
    {
        uiAnimator.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(a_levelIndex);
    }
}
