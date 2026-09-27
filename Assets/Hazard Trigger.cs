using UnityEngine;

/* Detects when the basket enters/exits a hazard trigger zone and tells
the shared AlertDisplay to show/hide it's hazard message. Needs attaching
to the hazard trigger GameObject, ie. non-rendered collision geometry serving as
hazard zone. ("Is Trigger" should be checked)

Unlike ImpactRelay/ImpactResponder, this is a warning,
not an actual collision. So it doesn't stop movement or end the session. 
The basket can pass through the zone freely, entering and exiting it as
many times as it likes. */
public class HazardTrigger : MonoBehaviour
{
    [Tooltip("The tag used to identify the basket's collider. Basket's collision object should have this tag set.")]
    public string basketTag = "Basket";

    [Tooltip("The shared alert panel this hazard's warning message is shown on.")]
    public AlertDisplay alertDisplay;

    [Tooltip("Message shown while the basket is inside this hazard zone.")]
    public string warningMessage = "Collision Warning";

    [Tooltip("Hazard entry/exit is logged here.")]
    public EventRecorder eventRecorder;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(basketTag))
        {            
            alertDisplay.ShowHazardWarning(warningMessage);
            eventRecorder.LogEvent(EventRecorder.EventType.HazardEnter, gameObject.name);
            Debug.Log("Hazard zone entered: " + gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(basketTag))
        {
            alertDisplay.HideHazardWarning();
            eventRecorder.LogEvent(EventRecorder.EventType.HazardExit, gameObject.name);
            Debug.Log("Hazard zone exited: " + gameObject.name);
        }
    }
}