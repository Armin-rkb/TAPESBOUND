using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Moves the camera so every player fits on the screen. Playing alone it simply follows the player, like before.
/// The camera is put on a rig, so screen shake and other effects on the camera itself keep working.
/// Added by the PartyManager when the camera is a child of the player.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PartyCamera : MonoBehaviour
{
    [Tooltip("How long the camera takes to move over when someone joins or leaves.")]
    [SerializeField] private float blendTime = 0.4f;

    private Transform rig = null;
    private int playerCount = 1;
    private float blendTimer = 0f;
    private Vector2 blendVelocity;

    public static PartyCamera Attach(Camera a_camera, Transform a_leader)
    {
        PartyCamera partyCamera = a_camera.GetComponent<PartyCamera>();
        if (partyCamera == null)
        {
            partyCamera = a_camera.gameObject.AddComponent<PartyCamera>();
        }
        partyCamera.CreateRig(a_leader);
        return partyCamera;
    }

    private void CreateRig(Transform a_leader)
    {
        if (rig != null)
        {
            return;
        }

        rig = new GameObject("Party Camera").transform;
        if (rig.gameObject.scene != gameObject.scene)
        {
            SceneManager.MoveGameObjectToScene(rig.gameObject, gameObject.scene);
        }
        rig.position = new Vector3(a_leader.position.x, a_leader.position.y, a_leader.position.z);
        transform.SetParent(rig, true);
    }

    private void LateUpdate()
    {
        if (rig == null || PartyManager.instance == null)
        {
            return;
        }

        int count = PartyManager.instance.GetScreenPlayers().Count;
        if (count != playerCount)
        {
            playerCount = count;
            blendTimer = blendTime;
        }

        Vector2 focus = PartyManager.instance.GetFocusPoint();
        if (blendTimer > 0f)
        {
            blendTimer -= Time.unscaledDeltaTime;
            focus = Vector2.SmoothDamp(rig.position, focus, ref blendVelocity, blendTime * 0.25f,Mathf.Infinity, Time.unscaledDeltaTime);
        }
        else
        {
            blendVelocity = Vector2.zero;
        }

        rig.position = new Vector3(focus.x, focus.y, rig.position.z);
    }
}
