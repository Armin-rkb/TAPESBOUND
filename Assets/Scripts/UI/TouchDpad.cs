using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

/// <summary>
/// On-screen d-pad for touch screens. It works as the left stick of a virtual gamepad, so the game reads it
/// like any other controller. Slide a finger around it to change direction.
/// </summary>
public class TouchDpad : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [InputControl(layout = "Vector2")]
    [SerializeField] private string stickPath = "<Gamepad>/leftStick";
    [Tooltip("Touching the middle of the d-pad does nothing, as a part of its width.")]
    [SerializeField] private float centerSize = 0.12f;

    protected override string controlPathInternal
    {
        get => stickPath;
        set => stickPath = value;
    }

    public void OnPointerDown(PointerEventData a_eventData)
    {
        SendDirection(a_eventData);
    }

    public void OnDrag(PointerEventData a_eventData)
    {
        SendDirection(a_eventData);
    }

    public void OnPointerUp(PointerEventData a_eventData)
    {
        SendValueToControl(Vector2.zero);
    }

    private void SendDirection(PointerEventData a_eventData)
    {
        RectTransform rect = (RectTransform)transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, a_eventData.position, a_eventData.pressEventCamera, out Vector2 point);
        SendValueToControl(GetDirection(point - rect.rect.center, rect.rect.width, centerSize));
    }

    /// <summary>
    /// The direction of a touch, a_offset away from the middle of a d-pad that is a_width wide.
    /// </summary>
    public static Vector2 GetDirection(Vector2 a_offset, float a_width, float a_centerSize)
    {
        return a_offset.magnitude < a_width * a_centerSize ? Vector2.zero : a_offset.normalized;
    }
}
