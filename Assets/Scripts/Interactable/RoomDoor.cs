using System.Collections;
using UnityEngine;

/// <summary>
/// Walk into it to go to another room in the same scene: the screen fades out and the player comes out
/// at the arrival point. For a door to another scene use a LevelLoader.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomDoor : MonoBehaviour
{
    [SerializeField] private Transform arrival = null;
    [Tooltip("The background of the room behind this door, the camera stays inside it.")]
    [SerializeField] private SpriteRenderer arrivalRoom = null;
    [SerializeField] private AudioClip sound = null;
    [SerializeField] private float fadeTime = 0.3f;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D a_collision)
    {
        if (a_collision != null && a_collision.CompareTag(Tags.Player) && a_collision.TryGetComponent(out PlayerController player) && player.CanMove)
        {
            StartCoroutine(Travel(player));
        }
    }

    private IEnumerator Travel(PlayerController a_player)
    {
        a_player.EnterCutscene(null);
        GameAudio.Play(sound, GameSound.Door);
        yield return ScreenFader.instance.FadeToBlack(fadeTime);

        MovePlayer(a_player, arrival.position);
        RoomCamera.instance.SetRoom(arrivalRoom);

        yield return ScreenFader.instance.FadeFromBlack(fadeTime);
        a_player.ExitCutscene(null);
    }

    /// <summary>
    /// Put the player somewhere right away.
    /// </summary>
    public static void MovePlayer(PlayerController a_player, Vector2 a_position)
    {
        a_player.transform.position = new Vector3(a_position.x, a_position.y, a_player.transform.position.z);
        a_player.rb.position = a_position;
        a_player.rb.linearVelocity = Vector2.zero;
    }
}
