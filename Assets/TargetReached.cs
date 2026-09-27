using UnityEngine;

/*Detects when the basket reaches the target and shows the target message.
The target uses its own alert slot so it can be shown at the same time
as a hazard warning without blocking antoher message. */
public class TargetReached : MonoBehaviour
{
    [Tooltip("Tag used to identify the basket's collider. Make sure the basket's collision object has this tag set.")]
    public string basketTag = "Basket";

    [Tooltip("The shared alert Canvas panel this message is shown on.")]
    public AlertDisplay alertDisplay;

    [Tooltip("The message shown while the basket is at the target.")]
    public string message = "Target Reached";

    [Tooltip("Optional. If assigned, reaching the target is logged here.")]
    public EventRecorder eventRecorder;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(basketTag))
        {
            alertDisplay.ShowTargetMessage(message);

            if (eventRecorder != null)
            {
                eventRecorder.LogEvent(EventRecorder.EventType.TargetReached, gameObject.name);
            }

            Debug.Log("Target reached: " + gameObject.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(basketTag))
        {
            alertDisplay.HideTargetMessage();

            Debug.Log("Left target zone: " + gameObject.name);
        }
    }
}
