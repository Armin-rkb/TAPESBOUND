using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// What one player pressed this frame.
/// </summary>
public struct ControlState
{
    public Vector2 move;
    public bool interact;
    public bool menu;
    public bool questLog;
    public bool join;
    public bool leave;

    // Join and leave don't count, they are not used to play.
    public bool HasGameplayInput => move != Vector2.zero || interact || menu || questLog;

    /// <summary>
    /// Combine the input of two devices (keyboard + controller).
    /// </summary>
    public void Add(ControlState a_other)
    {
        move = (move + a_other.move).normalized;
        interact |= a_other.interact;
        menu |= a_other.menu;
        questLog |= a_other.questLog;
        join |= a_other.join;
        leave |= a_other.leave;
    }
}

/// <summary>
/// Reads gamepads and joysticks with the Input System. Every controller is read on its own, so each player
/// only controls their own character.
///
/// Gamepad (Xbox, PlayStation, Switch...): stick or d-pad to walk, A talk/check, B menu, Y quest log, Start join, Select leave.
/// Other joysticks (like USB SNES pads): stick or hat to walk, button 1/2 talk/check, 3 menu, 4 quest log, 10 join, 9 leave.
/// </summary>
public static class ControllerInput
{
    // Stick input below this is ignored.
    public const float DeadZone = 0.4f;

    // sin(22.5°): splits the stick into 8 directions.
    private const float DiagonalThreshold = 0.383f;

    public static bool IsController(InputDevice a_device)
    {
        return a_device is Gamepad || a_device is Joystick;
    }

    /// <summary>
    /// Every connected gamepad and joystick.
    /// </summary>
    public static List<InputDevice> GetControllers(List<InputDevice> a_result = null)
    {
        List<InputDevice> result = a_result ?? new List<InputDevice>();
        result.Clear();
        foreach (InputDevice device in InputSystem.devices)
        {
            if (device.added && IsController(device))
            {
                result.Add(device);
            }
        }
        return result;
    }

    public static ControlState Read(InputDevice a_device)
    {
        ControlState state = new ControlState();

        if (a_device is Gamepad gamepad)
        {
            state.move = ToEightDirections(gamepad.leftStick.ReadValue() + gamepad.dpad.ReadValue());
            state.interact = gamepad.buttonSouth.wasPressedThisFrame;
            state.menu = gamepad.buttonEast.wasPressedThisFrame;
            state.questLog = gamepad.buttonNorth.wasPressedThisFrame;
            state.join = gamepad.startButton.wasPressedThisFrame;
            state.leave = gamepad.selectButton.wasPressedThisFrame;
        }
        else if (a_device is Joystick joystick)
        {
            Vector2 move = joystick.stick.ReadValue();
            if (joystick.hatswitch != null)
            {
                move += joystick.hatswitch.ReadValue();
            }
            state.move = ToEightDirections(move);
            state.interact = joystick.trigger.wasPressedThisFrame || WasPressed(joystick, "button2");
            state.menu = WasPressed(joystick, "button3");
            state.questLog = WasPressed(joystick, "button4");
            state.join = WasPressed(joystick, "button10");
            state.leave = WasPressed(joystick, "button9");
        }

        return state;
    }

    /// <summary>
    /// Earthbound walks in 8 directions, so a stick works the same as the d-pad and keyboard.
    /// </summary>
    public static Vector2 ToEightDirections(Vector2 a_input)
    {
        if (a_input.magnitude < DeadZone)
        {
            return Vector2.zero;
        }

        Vector2 direction = a_input.normalized;
        float x = Mathf.Abs(direction.x) > DiagonalThreshold ? Mathf.Sign(direction.x) : 0f;
        float y = Mathf.Abs(direction.y) > DiagonalThreshold ? Mathf.Sign(direction.y) : 0f;
        return new Vector2(x, y).normalized;
    }

    private static bool WasPressed(InputDevice a_device, string a_button)
    {
        ButtonControl button = a_device.TryGetChildControl<ButtonControl>(a_button);
        return button != null && button.wasPressedThisFrame;
    }
}
