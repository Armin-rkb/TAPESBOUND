using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
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

    [Header("Dialogue Options")]
    [SerializeField]
    private float dialogueDelay;


    [Header("UI References")]
    public GameObject dialogueBox;
    public Text dialogueText;

    // All dialogues lines.
    public Queue<DialogueBase.Info> dialogueInfo = new Queue<DialogueBase.Info>(); // FIFO Collection

    /// <summary>
    /// Retrieve the upcomming dialogue information.
    /// </summary>
    public void EnqueueDialogue(DialogueBase a_dialogueBase)
    {
        OpenDialogue();
        dialogueInfo.Clear();

        foreach (DialogueBase.Info a_info in a_dialogueBase.dialogueInfo)
        {
            dialogueInfo.Enqueue(a_info);
        }

        DequeueDialogue();
    }

    /// <summary>
    /// Update the next dialogue line.
    /// </summary>
    public void DequeueDialogue()
    {
        // Close dialogue box when reaching the end.
        if (dialogueInfo.Count == 0)
        {
            CloseDialogue();
            return;
        }

        DialogueBase.Info info = dialogueInfo.Dequeue();

        dialogueText.text = info.text; // Remove?
        StartCoroutine(TypeText(info));
    }

    private IEnumerator TypeText(DialogueBase.Info a_info)
    {
        dialogueText.text = "";
        foreach (char a_char in a_info.text.ToCharArray())
        {
            yield return new WaitForSeconds(dialogueDelay);
            dialogueText.text += a_char;
            yield return null;
        }
    }

    private void OpenDialogue()
    {
        dialogueBox.SetActive(true);
    }
    
    private void CloseDialogue()
    {
        dialogueBox.SetActive(false);
    }
}
