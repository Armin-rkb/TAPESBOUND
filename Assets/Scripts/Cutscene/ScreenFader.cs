using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fades the whole screen to black and back, and shows full screen pictures like title cards.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;

    [SerializeField] private Image blackScreen = null;
    [Tooltip("Shown on top of the black screen.")]
    [SerializeField] private Image picture = null;

    private void Awake()
    {
        instance = this;
        SetAlpha(picture, 0f);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void SetBlack(bool a_isBlack)
    {
        SetAlpha(blackScreen, a_isBlack ? 1f : 0f);
    }

    public IEnumerator FadeToBlack(float a_time)
    {
        yield return Fade(blackScreen, 1f, a_time);
    }

    public IEnumerator FadeFromBlack(float a_time)
    {
        yield return Fade(blackScreen, 0f, a_time);
    }

    public IEnumerator FadeInPicture(Sprite a_sprite, float a_time, float a_scale = 1f)
    {
        SetAlpha(picture, 0f);
        picture.sprite = a_sprite;
        picture.rectTransform.localScale = Vector3.one * a_scale;
        yield return Fade(picture, 1f, a_time);
    }

    public IEnumerator FadeOutPicture(float a_time)
    {
        yield return Fade(picture, 0f, a_time);
    }

    /// <summary>
    /// Fade a picture in, keep it on screen for a_hold seconds and fade it out again.
    /// </summary>
    public IEnumerator ShowPicture(Sprite a_sprite, float a_fadeTime, float a_hold)
    {
        yield return FadeInPicture(a_sprite, a_fadeTime);
        yield return new WaitForSeconds(a_hold);
        yield return FadeOutPicture(a_fadeTime);
    }

    private static IEnumerator Fade(Graphic a_graphic, float a_alpha, float a_time)
    {
        float start = a_graphic.color.a;
        float elapsed = 0f;
        while (elapsed < a_time)
        {
            elapsed += Time.deltaTime;
            SetAlpha(a_graphic, Mathf.Lerp(start, a_alpha, elapsed / a_time));
            yield return null;
        }
        SetAlpha(a_graphic, a_alpha);
    }

    private static void SetAlpha(Graphic a_graphic, float a_alpha)
    {
        Color color = a_graphic.color;
        color.a = a_alpha;
        a_graphic.color = color;
    }
}
