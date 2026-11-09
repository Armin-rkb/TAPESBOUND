using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data

public class SelectableUI : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler // Required interface when using the OnSelect and OnDeselect method.
{
    [SerializeField] private Image uiCursor = null;

    public void OnSelect(BaseEventData a_eventData)
    {
        uiCursor.gameObject.SetActive(true);
        GameAudio.Play(GameSound.Cursor);
    }

    public void OnSubmit(BaseEventData a_eventData)
    {
        GameAudio.Play(GameSound.Confirm);
    }

    public void OnPointerClick(PointerEventData a_eventData)
    {
        GameAudio.Play(GameSound.Confirm);
    }

    public void OnDeselect(BaseEventData a_eventData)
    {
        uiCursor.gameObject.SetActive(false);
    }
}
