using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required when using Event data

public class SelectableUI : MonoBehaviour, ISelectHandler, IDeselectHandler // Required interface when using the OnSelect and OnDeselect method.
{
    [SerializeField] private Image uiCursor = null;

    public void OnSelect(BaseEventData a_eventData)
    {
        uiCursor.gameObject.SetActive(true);
    }

    public void OnDeselect(BaseEventData a_eventData)
    {
        uiCursor.gameObject.SetActive(false);
    }
}
