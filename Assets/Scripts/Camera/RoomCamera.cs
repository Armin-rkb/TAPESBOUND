using System.Collections;
using UnityEngine;

/// <summary>
/// Earthbound style camera: follows the player but never shows anything outside the current room.
/// A room smaller than the screen stays centered. Put this on a parent of the camera, so ScreenShake
/// can keep moving the camera itself.
/// </summary>
public class RoomCamera : MonoBehaviour
{
    public static RoomCamera instance;

    [SerializeField] private Camera view = null;
    [SerializeField] private Transform target = null;
    [SerializeField] private SpriteRenderer room = null;

    public Vector2 HalfView => new Vector2(view.orthographicSize * view.aspect, view.orthographicSize);

    /// <summary>
    /// Where the camera has to be to look at a_focus without showing anything outside a_room.
    /// </summary>
    public static Vector2 ClampToRoom(Vector2 a_focus, Bounds a_room, Vector2 a_halfView)
    {
        return new Vector2(ClampAxis(a_focus.x, a_room.min.x, a_room.max.x, a_halfView.x),
                           ClampAxis(a_focus.y, a_room.min.y, a_room.max.y, a_halfView.y));
    }

    private static float ClampAxis(float a_focus, float a_min, float a_max, float a_halfView)
    {
        if (a_max - a_min <= a_halfView * 2f)
        {
            return (a_min + a_max) * 0.5f;
        }
        return Mathf.Clamp(a_focus, a_min + a_halfView, a_max - a_halfView);
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private void LateUpdate()
    {
        // Without a target a cutscene is moving the camera.
        if (target != null && room != null)
        {
            LookAt(target.position);
        }
    }

    public void SetRoom(SpriteRenderer a_room)
    {
        room = a_room;
        if (target != null)
        {
            LookAt(target.position);
        }
    }

    /// <summary>
    /// Follow a_target, or stop following with null.
    /// </summary>
    public void Follow(Transform a_target)
    {
        target = a_target;
    }

    public void LookAt(Vector2 a_focus)
    {
        Vector2 position = ClampToRoom(a_focus, room.bounds, HalfView);
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    /// <summary>
    /// Look at the left edge of the room and pan to the right edge.
    /// </summary>
    public IEnumerator PanAcross(float a_seconds)
    {
        Bounds bounds = room.bounds;
        float elapsed = 0f;
        while (elapsed < a_seconds)
        {
            elapsed += Time.deltaTime;
            LookAt(new Vector2(Mathf.Lerp(bounds.min.x, bounds.max.x, elapsed / a_seconds), bounds.center.y));
            yield return null;
        }
    }
}
