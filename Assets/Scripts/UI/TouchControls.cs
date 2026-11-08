using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the on-screen d-pad and button on phones, and on anything else as soon as it gets touched.
/// They are sized to fit in the black bars next to the picture, so they never cover the game.
/// </summary>
public class TouchControls : MonoBehaviour
{
    [SerializeField] private GameObject buttons = null;
    [SerializeField] private Camera view = null;
    [SerializeField] private CanvasScaler scaler = null;
    [Tooltip("Show them on every device, to try them out in the Editor.")]
    [SerializeField] private bool alwaysShow = false;
    [Tooltip("The size of the controls on a phone: the short side of the screen in canvas units.")]
    [SerializeField] private float phoneScreenSize = 390f;
    [Tooltip("How much of a black bar the controls need, in canvas units.")]
    [SerializeField] private float controlsSize = 152f;

    private void Awake()
    {
        buttons.SetActive(alwaysShow || Application.isMobilePlatform);
    }

    private void Update()
    {
        if (!buttons.activeSelf && Input.touchCount > 0)
        {
            buttons.SetActive(true);
        }
    }

    private void LateUpdate()
    {
        if (buttons.activeSelf)
        {
            scaler.scaleFactor = GetScale(Screen.width, Screen.height, view.pixelRect, phoneScreenSize, controlsSize);
        }
    }

    /// <summary>
    /// Phone sized controls, or smaller when the black bars around the picture aren't big enough.
    /// </summary>
    public static float GetScale(float a_screenWidth, float a_screenHeight, Rect a_picture, float a_phoneScreenSize, float a_controlsSize)
    {
        float phoneScale = Mathf.Min(a_screenWidth, a_screenHeight) / a_phoneScreenSize;
        float barScale = Mathf.Max(a_picture.x, a_picture.y) / a_controlsSize;
        return Mathf.Max(1f, Mathf.Min(phoneScale, barScale));
    }
}
