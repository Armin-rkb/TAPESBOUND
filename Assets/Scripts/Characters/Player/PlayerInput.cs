using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input of player 1, who plays the leader: the keyboard plus the first controller they use.
/// Other controllers join as party members, see LocalCoop.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    [Header("Movement keys")]
    [SerializeField] private KeyCode[] upKeys = { KeyCode.UpArrow, KeyCode.W };
    [SerializeField] private KeyCode[] downKeys = { KeyCode.DownArrow, KeyCode.S };
    [SerializeField] private KeyCode[] leftKeys = { KeyCode.LeftArrow, KeyCode.A };
    [SerializeField] private KeyCode[] rightKeys = { KeyCode.RightArrow, KeyCode.D };
    [HideInInspector] public Vector2 movementDirection;

    // True on the frame up/down is pushed, for moving through menus.
    [HideInInspector] public bool menuUpPressed;
    [HideInInspector] public bool menuDownPressed;
    private float previousVertical = 0f;

    [Header("Using menu key")]
    [SerializeField] private KeyCode menuKey = KeyCode.LeftShift;
    [HideInInspector] public bool menuKeyPressed;

    [Header("Interacting key")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;
    [HideInInspector] public bool interactKeyPressed;

    [Header("Quest log key")]
    [SerializeField] private KeyCode questLogKey = KeyCode.Q;
    [HideInInspector] public bool questLogKeyPressed;

    private readonly List<InputDevice> controllers = new List<InputDevice>();

    void Update()
    {
        ControlState controls = ReadKeyboard();
        controls.Add(ReadController());

        movementDirection = controls.move;

        float verticalInput = movementDirection.y;
        menuUpPressed = verticalInput > 0.5f && previousVertical <= 0.5f;
        menuDownPressed = verticalInput < -0.5f && previousVertical >= -0.5f;
        previousVertical = verticalInput;

        menuKeyPressed = controls.menu;

        interactKeyPressed = controls.interact;

        questLogKeyPressed = controls.questLog;
    }

    private ControlState ReadKeyboard()
    {
        ControlState controls = new ControlState();
        controls.move = new Vector2(ReadAxis(leftKeys, rightKeys), ReadAxis(downKeys, upKeys)).normalized;
        controls.menu = Input.GetKeyDown(menuKey);
        controls.interact = Input.GetKeyDown(interactKey);
        controls.questLog = Input.GetKeyDown(questLogKey);
        return controls;
    }

    private ControlState ReadController()
    {
        // Player 1 takes the first free controller that is used to play (Start is for joining).
        if (LocalCoop.LeaderDevice == null)
        {
            foreach (InputDevice controller in ControllerInput.GetControllers(controllers))
            {
                if (!LocalCoop.IsInUse(controller) && ControllerInput.Read(controller).HasGameplayInput)
                {
                    LocalCoop.SetLeaderDevice(controller);
                    break;
                }
            }
        }

        return LocalCoop.LeaderDevice != null ? ControllerInput.Read(LocalCoop.LeaderDevice) : new ControlState();
    }

    private static float ReadAxis(KeyCode[] a_negativeKeys, KeyCode[] a_positiveKeys)
    {
        return (IsAnyKeyHeld(a_positiveKeys) ? 1f : 0f) - (IsAnyKeyHeld(a_negativeKeys) ? 1f : 0f);
    }

    private static bool IsAnyKeyHeld(KeyCode[] a_keys)
    {
        if (a_keys == null)
        {
            return false;
        }

        foreach (KeyCode key in a_keys)
        {
            if (Input.GetKey(key))
            {
                return true;
            }
        }
        return false;
    }
}
