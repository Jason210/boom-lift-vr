using UnityEngine;
using UnityEngine.EventSystems;

/* for trying to get the raycaster to work. */

public class UIHoverDebug : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[UIHoverDebug] Pointer ENTERED " + gameObject.name);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[UIHoverDebug] Pointer EXITED " + gameObject.name);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[UIHoverDebug] Pointer CLICKED " + gameObject.name);
    }
}