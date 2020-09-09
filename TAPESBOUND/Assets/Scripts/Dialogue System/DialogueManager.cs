using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
    private float dialogueDelay = 0.01f;

    [Header("UI References")]
    public GameObject dialogueBox = null;
    public Text dialogueText = null;
    public GameObject dialogueOptionUI = null;
    public GameObject[] optionButtons = null;

    // Dialogue variables.
    private bool isDialogueOption = false;
    private int optionsAmount = 0;

    private bool isCurrentlyTyping = false;
    private string completeText = "";

    private bool isBuffering = false;
    private float dialogueBufferTime = 0.1f;

    // All dialogues lines.
    public Queue<DialogueBase.Info> dialogueInfo = new Queue<DialogueBase.Info>(); // FIFO Collection

    // Static Events
    public delegate void OnDialogueEnter();
    public static event OnDialogueEnter onDialogueEnter;
    public delegate void OnDialogueExit();
    public static event OnDialogueExit onDialogueExit;

    /// <summary>
    /// Retrieve the upcoming dialogue information.
    /// </summary>
    public void EnqueueDialogue(DialogueBase a_dialogueBase)
    {
        dialogueInfo.Clear();
        StartCoroutine(BufferTimer());
        
        // Check if the conversation has respond options.
        OptionsParser(a_dialogueBase);

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
        if (isCurrentlyTyping)
        {
            if (isBuffering)
            {
                return;
            }
            CompleteText();
            StopAllCoroutines();
            isCurrentlyTyping = false;
            return;
        }

        // Close dialogue box when reaching the end.
        if (dialogueInfo.Count == 0)
        {
            if (isDialogueOption)
            {
                OptionsLogic();
            }
            else
            {
                CloseDialogue();
            }
            return;
        }

        DialogueBase.Info info = dialogueInfo.Dequeue();
        completeText = info.text; 

        dialogueText.text = "";
        StartCoroutine(TypeText(info));
    }

    private void OptionsParser(DialogueBase a_dialogueBase)
    {
        if (a_dialogueBase is DialogueOptions)
        {
            isDialogueOption = true;
            DialogueOptions dialogueOptions = a_dialogueBase as DialogueOptions;
            optionsAmount = dialogueOptions.optionsInfo.Length;

            // Deactivate all options buttons to prevent choosing previous buttons.
            foreach (GameObject optionButton in optionButtons)
            {
                optionButton.SetActive(false);
            }

            // Enable all possible buttons and set their event when pressed.
            for (int i = 0; i < optionsAmount; i++)
            {
                optionButtons[i].SetActive(true);
                optionButtons[i].transform.GetChild(0).GetComponent<Text>().text = dialogueOptions.optionsInfo[i].buttonName;
                
                int index = i;
                optionButtons[i].GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnButtonPressed(dialogueOptions.optionsInfo[index].myEvent,
                        dialogueOptions.optionsInfo[index].nextDialogue);
                });
            }

            // Select the first button by default.
            EventSystem.current.SetSelectedGameObject(optionButtons[0]);
            optionButtons[0].GetComponent<SelectableUI>().OnSelect(new BaseEventData(EventSystem.current));
        }
        else
        {
            isDialogueOption = false;
        }
    }

    void OnButtonPressed(UnityEvent a_dialogueEvent, DialogueBase a_nextDialogue)
    {
        a_dialogueEvent?.Invoke();

        if (a_nextDialogue != null)
        {
            EnqueueDialogue(a_nextDialogue);
        }

        foreach (GameObject optionButton in optionButtons)
        {
            optionButton.GetComponent<Button>().onClick.RemoveAllListeners();
        }
        CloseOptions();
    }

    private IEnumerator BufferTimer()
    {
        isBuffering = true;
        yield return new WaitForSeconds(dialogueBufferTime);
        isBuffering = false;
    }

    private IEnumerator TypeText(DialogueBase.Info a_info)
    {
        isCurrentlyTyping = true;
        foreach (char a_char in a_info.text.ToCharArray())
        {
            yield return new WaitForSeconds(dialogueDelay);
            dialogueText.text += a_char;
        }
        isCurrentlyTyping = false;
    }

    private void CompleteText()
    {
        dialogueText.text = completeText;
    }

    public void OpenDialogue()
    {
        dialogueBox.SetActive(true);
        CloseOptions();

        onDialogueEnter?.Invoke();
    }
    
    private void CloseDialogue()
    {
        dialogueBox.SetActive(false);

        onDialogueExit?.Invoke();
    }

    private void OptionsLogic()
    {
        if (isDialogueOption)
        {
            dialogueOptionUI.SetActive(true);
        }
    }
    
    public void CloseOptions()
    {
        dialogueOptionUI.SetActive(false);
    }
}
