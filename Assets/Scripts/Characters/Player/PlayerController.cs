using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public enum PlayerState
{
    normal,
    menu,
    dialogue,
    cutscene,
    questLog
}

public enum PlayerOutfit
{
    Default,
    Pajama,
    Scuba,
    Doctor,
    Eskimo
}

public class PlayerController : MonoBehaviour
{
    public static Vector3 GetPosition => GetInstance().transform.position;
    private static PlayerController GetInstance() 
    {
        if (instance == null)
        {
            instance = FindObjectOfType<PlayerController>();
        }

        return instance;
    }
    private static PlayerController instance;

    // Name used for {player} in dialogue.
    public static string PlayerName
    {
        get
        {
            PlayerController player = GetInstance();
            if (player != null && player.playerCharacterProfile != null && !string.IsNullOrEmpty(player.playerCharacterProfile.characterName))
            {
                return player.playerCharacterProfile.characterName;
            }
            return "You";
        }
    }

    [Header("Character Stats")]
    public CharacterProfile playerCharacterProfile;

    [Header("References:")]
    public SpriteRenderer spriteRenderer = null;
    public Animator animator = null;
    public Rigidbody2D rb = null;
    public PlayerInput playerInput = null;
    [SerializeField] private PlayerMovement playerMovement = null;
    [SerializeField] private GameObject playerOptionUI = null;

    public IInteractable currentInteractable = null;

    [Header("PlayerState")]
    public PlayerState currentPlayerState = PlayerState.normal;
    private PlayerState previousPlayerState = PlayerState.normal;
    public PlayerOutfit currentOutfit = PlayerOutfit.Default;
    private PlayerOutfit previousOutfit = PlayerOutfit.Default;

    private readonly string outfitAnimationParam = "Outfit";
    private readonly string outfitChangedAnimationParam = "OutfitChanged";

    private void Awake()
    {
        ChangeOutfit(currentOutfit);
    }

    private void Update()
    {
        ChangeOutfit(currentOutfit);

        switch (currentPlayerState)
        {
            case PlayerState.normal:
            {
                CheckInteraction();

                if (playerInput.menuKeyPressed)
                {
                    OpenMenu();
                }
                else if (playerInput.questLogKeyPressed)
                {
                    OpenQuestLog();
                }
                break;
            }
            case PlayerState.questLog:
            {
                if (playerInput.questLogKeyPressed || playerInput.menuKeyPressed)
                {
                    CloseQuestLog();
                }
                else if (playerInput.interactKeyPressed)
                {
                    QuestUI.instance.TrackSelected();
                }
                else if (playerInput.menuUpPressed)
                {
                    QuestUI.instance.MoveSelection(-1);
                }
                else if (playerInput.menuDownPressed)
                {
                    QuestUI.instance.MoveSelection(1);
                }
                break;
            }
            case PlayerState.menu:
            {
                if (playerInput.menuKeyPressed)
                {
                    CloseMenu();
                }
                break;
            }
            case PlayerState.dialogue:
            {
                if (playerInput.interactKeyPressed)
                {
                    DialogueManager.instance.DequeueDialogue();
                }
                break;
            }
            case PlayerState.cutscene:
            {
                break;
            }
        }
    }

    private void ChangeOutfit(PlayerOutfit newOutfit)
    {
        if (previousOutfit == newOutfit)
            return;
        
        previousOutfit = currentOutfit;
        currentOutfit = newOutfit;
        animator.SetInteger(outfitAnimationParam, (int)currentOutfit);
        animator.SetTrigger(outfitChangedAnimationParam);
        animator.ResetTrigger(outfitChangedAnimationParam);
    }

    private void OpenMenu()
    {
        playerOptionUI.SetActive(true);
        EventSystem.current.SetSelectedGameObject(playerOptionUI.transform.GetChild(0).GetChild(0).gameObject);
        playerOptionUI.transform.GetChild(0).GetChild(0).GetComponent<SelectableUI>()
            .OnSelect(new BaseEventData(EventSystem.current));
        
        SwitchPlayerState(PlayerState.menu);
        LockMovement();
    }

    private void CloseMenu()
    {
        playerOptionUI.SetActive(false);

        SwitchPlayerState(PlayerState.normal);
        UnlockMovement();
    }

    // Public so a menu button can open it too.
    public void OpenQuestLog()
    {
        if (QuestUI.instance == null)
        {
            return;
        }

        if (currentPlayerState == PlayerState.menu)
        {
            playerOptionUI.SetActive(false);
        }

        QuestUI.instance.OpenLog();
        SwitchPlayerState(PlayerState.questLog);
        LockMovement();
    }

    public void CloseQuestLog()
    {
        if (QuestUI.instance != null)
        {
            QuestUI.instance.CloseLog();
        }

        SwitchPlayerState(PlayerState.normal);
        UnlockMovement();
    }
    
    // Change the current player state.
    public void SwitchPlayerState(PlayerState a_newState)
    {
        previousPlayerState = currentPlayerState;
        currentPlayerState = a_newState;
    }

    // Interact with an object in the world.
    private void CheckInteraction()
    {
        if (currentInteractable != null)
        {
            if (playerInput.interactKeyPressed)
            {
                currentInteractable.Interact();
            }
        }
    }

    private void DialogueEnter()
    {
        SwitchPlayerState(PlayerState.dialogue);
        LockMovement();
    }
    
    private void DialogueExit()
    {
        if (previousPlayerState == PlayerState.cutscene)
        {
            SwitchPlayerState(PlayerState.cutscene);
        }
        else
        {
            SwitchPlayerState(PlayerState.normal);
            UnlockMovement();
        }
    }

    public void EnterCutscene(PlayableDirector aDirector)
    {
        SwitchPlayerState(PlayerState.cutscene);
        LockMovement();
    }

    public void ExitCutscene(PlayableDirector aDirector)
    {
        SwitchPlayerState(PlayerState.normal);
        UnlockMovement();
    }

    public void LockMovement()
    {
        playerMovement.StopMove();
        playerMovement.enabled = false;
        animator.enabled = false;
    }

    public void UnlockMovement()
    {
        animator.enabled = true;
        playerMovement.enabled = true;
    }

    private void OnEnable()
    {
        // TODO: Replace this!
        PlayableDirector playableDirector = GameObject.FindObjectOfType<PlayableDirector>();
        if (playableDirector)
        {
            playableDirector.played += EnterCutscene;
            playableDirector.stopped += ExitCutscene;
        }

        DialogueManager.onDialogueEnter += DialogueEnter;
        DialogueManager.onDialogueExit += DialogueExit;
    }

    private void OnDisable()
    {
        // TODO: Replace this!
        PlayableDirector playableDirector = GameObject.FindObjectOfType<PlayableDirector>();
        if (playableDirector)
        {
            playableDirector.played -= EnterCutscene;
            playableDirector.stopped -= ExitCutscene;
        }

        DialogueManager.onDialogueEnter -= DialogueEnter;
        DialogueManager.onDialogueExit -= DialogueExit;
    }
}
