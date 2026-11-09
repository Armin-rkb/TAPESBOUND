using System.Collections;
using UnityEngine;

/// <summary>
/// Runs the cutscenes of the Underwhere train chapter, from the black intro screen to the chapter card.
/// Between the opening and the machinist the player walks through the train by themselves.
/// All pictures, sounds, dialogue and timing are set in the inspector.
/// </summary>
public class TrainChapterDirector : MonoBehaviour
{
    public enum StartPoint
    {
        Intro,
        PlayerControl
    }

    [System.Serializable]
    public class PanShot
    {
        public SpriteRenderer room = null;
        public float seconds = 5f;
    }

    [Header("Testing")]
    [Tooltip("Player Control skips the opening cutscenes.")]
    [SerializeField] private StartPoint startAt = StartPoint.Intro;

    [Header("References")]
    [SerializeField] private PlayerController player = null;
    [SerializeField] private ScreenShake screenShake = null;

    [Header("Sound")]
    [SerializeField] private AudioSource trainTracks = null;
    [SerializeField] private AudioSource earbuds = null;
    [SerializeField] private AudioSource ambience = null;
    [SerializeField] private AudioClip doorSlam = null;
    [SerializeField] private AudioClip trainWhistle = null;
    [SerializeField] private float trainTracksVolume = 0.6f;
    [SerializeField] private float earbudsVolume = 0.25f;
    [SerializeField] private float ambienceVolume = 0.7f;

    [Header("Timing")]
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float cardHold = 2.5f;

    [Header("1. Intro")]
    [Tooltip("Shown in web builds, browsers only play sound after the first tap or click.")]
    [SerializeField] private GameObject tapToStart = null;
    [SerializeField] private float blackScreenHold = 5f;
    [SerializeField] private Sprite rememberCard = null;
    [SerializeField] private Sprite logoCard = null;

    [Header("2. On the way to Valencia")]
    [SerializeField] private PanShot trainOutside = null;
    [SerializeField] private PanShot[] trainInside = null;

    [Header("3. MC's compartment")]
    [SerializeField] private SpriteRenderer compartment = null;
    [SerializeField] private float sleepHold = 4f;
    [SerializeField] private SpriteRenderer sittingMC = null;
    [SerializeField] private Sprite mcAwake = null;
    [SerializeField] private Transform walkOffSofaPoint = null;
    [SerializeField] private SpriteRenderer mbk = null;
    [SerializeField] private Sprite mbkFront = null;
    [SerializeField] private Sprite mbkSide = null;
    [SerializeField] private Transform mbkDoorPoint = null;
    [SerializeField] private Transform mbkTalkPoint = null;
    [SerializeField] private DialogueBase mbkApology = null;

    [Header("4. The machinist")]
    [SerializeField] private SpriteRenderer machinist = null;
    [SerializeField] private Transform machinistDoorPoint = null;
    [SerializeField] private Transform mcStartledPoint = null;
    [SerializeField] private Sprite mcStartled = null;
    [SerializeField] private DialogueBase machinistYells = null;

    [Header("5. Machinist room")]
    [SerializeField] private SpriteRenderer machinistRoom = null;
    [SerializeField] private Transform machinistStartPoint = null;
    [SerializeField] private Transform machinistWindowPoint = null;
    [SerializeField] private Transform mcStartPoint = null;
    [SerializeField] private Transform mcWindowPoint = null;
    [SerializeField] private DialogueBase rightOnTime = null;
    [SerializeField] private float walkSpeed = 1.5f;

    [Header("6. Ending")]
    [SerializeField] private PanShot mountains = null;
    [SerializeField] private Sprite endingLogo = null;
    [SerializeField] private float endingLogoScale = 0.72f;
    [SerializeField] private Sprite chapterCard = null;

    private ScreenFader Fader => ScreenFader.instance;
    private RoomCamera Cam => RoomCamera.instance;

    private void Start()
    {
        machinist.gameObject.SetActive(false);

        if (startAt == StartPoint.Intro)
        {
            StartCoroutine(PlayOpening());
        }
        else
        {
            Fader.SetBlack(false);
            mbk.gameObject.SetActive(false);
            sittingMC.gameObject.SetActive(false);
            trainTracks.volume = trainTracksVolume;
            trainTracks.Play();
            Cam.SetRoom(compartment);
            Cam.Follow(player.transform);
        }
    }

    /// <summary>
    /// Everything before the player can walk around.
    /// </summary>
    private IEnumerator PlayOpening()
    {
        player.EnterCutscene(null);
        player.spriteRenderer.enabled = false;
        mbk.gameObject.SetActive(false);
        Fader.SetBlack(true);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            tapToStart.SetActive(true);
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.touchCount > 0 || Input.anyKeyDown);
            tapToStart.SetActive(false);
            GameAudio.Play(GameSound.Confirm);
        }

        trainTracks.volume = 0f;
        trainTracks.Play();
        StartCoroutine(FadeVolume(trainTracks, trainTracksVolume, blackScreenHold));
        yield return new WaitForSeconds(blackScreenHold);

        yield return Fader.ShowPicture(rememberCard, fadeTime, cardHold);
        yield return Fader.ShowPicture(logoCard, fadeTime, cardHold);

        yield return PlayPan(trainOutside);

        // MC's music, leaking out of the earbuds.
        earbuds.volume = 0f;
        earbuds.Play();
        StartCoroutine(FadeVolume(earbuds, earbudsVolume, fadeTime * 3f));
        foreach (PanShot shot in trainInside)
        {
            yield return PlayPan(shot);
        }

        yield return PlayCompartment();
    }

    private IEnumerator PlayPan(PanShot a_shot)
    {
        Cam.Follow(null);
        Cam.SetRoom(a_shot.room);
        Cam.LookAt(a_shot.room.bounds.min);
        StartCoroutine(Cam.PanAcross(a_shot.seconds));

        yield return Fader.FadeFromBlack(fadeTime);
        yield return new WaitForSeconds(Mathf.Max(0f, a_shot.seconds - fadeTime * 2f));
        yield return Fader.FadeToBlack(fadeTime);
    }

    private IEnumerator PlayCompartment()
    {
        Cam.SetRoom(compartment);
        Cam.LookAt(compartment.bounds.center);

        yield return Fader.FadeFromBlack(fadeTime);
        yield return new WaitForSeconds(sleepHold);

        // The door flies open and wakes MC up, which is also the end of the music.
        SlamDoor();
        earbuds.Stop();
        sittingMC.sprite = mcAwake;
        mbk.gameObject.SetActive(true);
        mbk.sprite = mbkFront;
        mbk.transform.position = mbkDoorPoint.position;
        yield return new WaitForSeconds(1f);

        mbk.sprite = mbkSide;
        yield return WalkActor(mbk, mbkTalkPoint.position);
        yield return Say(mbkApology);
        yield return WalkActor(mbk, mbkDoorPoint.position);
        mbk.gameObject.SetActive(false);
        SlamDoor();
        yield return new WaitForSeconds(1f);

        // Off the sofa, from here on the player is in control.
        sittingMC.gameObject.SetActive(false);
        RoomDoor.MovePlayer(player, sittingMC.transform.position);
        player.spriteRenderer.enabled = true;
        yield return WalkPlayer(walkOffSofaPoint.position);

        Cam.Follow(player.transform);
        player.ExitCutscene(null);
    }

    /// <summary>
    /// Runs when the player has knocked enough on the machinist's door.
    /// </summary>
    public void PlayMachinist()
    {
        StartCoroutine(PlayMachinistRoutine());
    }

    private IEnumerator PlayMachinistRoutine()
    {
        player.EnterCutscene(null);
        machinist.gameObject.SetActive(true);
        machinist.flipX = false;
        machinist.transform.position = machinistDoorPoint.position;
        RoomDoor.MovePlayer(player, mcStartledPoint.position);
        player.spriteRenderer.sprite = mcStartled;
        player.spriteRenderer.flipX = false;
        SlamDoor();
        yield return new WaitForSeconds(0.6f);

        yield return Say(machinistYells);
        yield return Fader.FadeToBlack(fadeTime);

        Cam.Follow(null);
        Cam.SetRoom(machinistRoom);
        Cam.LookAt(machinistRoom.bounds.center);
        machinist.transform.position = machinistStartPoint.position;
        RoomDoor.MovePlayer(player, mcStartPoint.position);
        LookRight();
        yield return null;
        yield return Fader.FadeFromBlack(fadeTime);

        yield return WalkActor(machinist, machinistWindowPoint.position);
        yield return WalkPlayer(mcWindowPoint.position);
        yield return Say(rightOnTime);
        yield return Fader.FadeToBlack(fadeTime);

        player.spriteRenderer.enabled = false;
        machinist.gameObject.SetActive(false);
        yield return PlayEnding();
    }

    private IEnumerator PlayEnding()
    {
        Cam.SetRoom(mountains.room);
        Cam.LookAt(mountains.room.bounds.min);
        StartCoroutine(Cam.PanAcross(mountains.seconds));
        yield return Fader.FadeFromBlack(fadeTime * 2f);

        yield return new WaitForSeconds(mountains.seconds * 0.35f);
        yield return Fader.FadeInPicture(endingLogo, fadeTime * 2f, endingLogoScale);
        yield return new WaitForSeconds(mountains.seconds * 0.35f);

        StartCoroutine(Fader.FadeOutPicture(fadeTime * 2f));
        StartCoroutine(FadeVolume(trainTracks, 0f, fadeTime * 2f));
        yield return Fader.FadeToBlack(fadeTime * 2f);

        DialogueManager.PlaySound(trainWhistle);
        yield return new WaitForSeconds(fadeTime * 2f);

        ambience.volume = 0f;
        ambience.Play();
        StartCoroutine(FadeVolume(ambience, ambienceVolume, fadeTime * 3f));
        yield return Fader.ShowPicture(chapterCard, fadeTime * 2f, cardHold * 2f);
    }

    private void SlamDoor()
    {
        DialogueManager.PlaySound(doorSlam);
        if (screenShake != null)
        {
            screenShake.Shake(0.25f);
        }
    }

    private static IEnumerator Say(DialogueBase a_dialogue)
    {
        DialogueManager.instance.EnqueueDialogue(a_dialogue);
        yield return new WaitWhile(() => DialogueManager.instance.IsOpen);
    }

    // The side sprites look left.
    private IEnumerator WalkActor(SpriteRenderer a_actor, Vector2 a_destination)
    {
        Transform actor = a_actor.transform;
        a_actor.flipX = a_destination.x > actor.position.x;
        float nextStep = 0f;

        while ((Vector2)actor.position != a_destination)
        {
            PlayStepWhenDue(ref nextStep);
            Vector2 next = Vector2.MoveTowards(actor.position, a_destination, walkSpeed * Time.deltaTime);
            actor.position = new Vector3(next.x, next.y, actor.position.z);
            yield return null;
        }
    }

    // Back to the normal standing sprite.
    private void LookRight()
    {
        player.animator.enabled = true;
        player.animator.SetFloat("LastHorizontal", 1f);
        player.animator.SetFloat("LastVertical", 0f);
        player.animator.SetBool("IsWalking", false);
        player.spriteRenderer.flipX = true;
    }

    private IEnumerator WalkPlayer(Vector2 a_destination)
    {
        Transform body = player.transform;
        Animator animator = player.animator;
        Vector2 direction = (a_destination - (Vector2)body.position).normalized;

        player.rb.simulated = false;
        animator.enabled = true;
        animator.SetFloat("Horizontal", direction.x);
        animator.SetFloat("Vertical", direction.y);
        animator.SetFloat("LastHorizontal", direction.x);
        animator.SetFloat("LastVertical", direction.y);
        animator.SetBool("IsWalking", true);
        player.spriteRenderer.flipX = direction.x > 0f;
        float nextStep = 0f;

        while ((Vector2)body.position != a_destination)
        {
            PlayStepWhenDue(ref nextStep);
            Vector2 next = Vector2.MoveTowards(body.position, a_destination, walkSpeed * Time.deltaTime);
            body.position = new Vector3(next.x, next.y, body.position.z);
            yield return null;
        }

        animator.SetBool("IsWalking", false);
        yield return null;
        RoomDoor.MovePlayer(player, a_destination);
        player.rb.simulated = true;
    }

    private static void PlayStepWhenDue(ref float a_nextStep)
    {
        if (Time.time >= a_nextStep)
        {
            GameAudio.PlayStep();
            a_nextStep = Time.time + GameAudio.StepInterval;
        }
    }

    private static IEnumerator FadeVolume(AudioSource a_source, float a_volume, float a_time)
    {
        float start = a_source.volume;
        float elapsed = 0f;
        while (elapsed < a_time)
        {
            elapsed += Time.deltaTime;
            a_source.volume = Mathf.Lerp(start, a_volume, elapsed / a_time);
            yield return null;
        }
        a_source.volume = a_volume;
    }
}
