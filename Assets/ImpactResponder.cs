using UnityEngine;

/* Ends the session when the basket hits solid geometry.

Attach script to each collision object in the scene. On impact it stops boom
movement, shows the collision message, logs the event, and opens the
debrief screen. */
public class ImpactResponder : MonoBehaviour
{
    [Tooltip("The tag used to identify the basket's collider. Must match the tag used on basket collision object.")]
    public string basketTag = "Basket";

    [Header("References")]
    [Tooltip("Disabled on impact, which also disables input via its own OnDisable().")]
    public BoomMovement boomMovement;

    [Tooltip("The shared alert panel the end-of-session message is shown on.")]
    public AlertDisplay alertDisplay;

    [Tooltip("The message shown when the session ends due to a collision.")]
    [TextArea]
    public string impactMessage = "Simulation Ended:\nCollision Detected";

    [Tooltip("Optional. If assigned, the impact is logged here.")]
    public EventRecorder eventRecorder;

    [Tooltip("Shows the debrief screen after a short puase, or immediately if the trigger is pressed to skip ahead.")]
    public DebriefScreen debriefScreen;

    [Tooltip("How long the impact message and alarm stay up before the debrief takes over, unless skipped early.")]
    public float debriefDelaySeconds = 4f;

    private bool hasImpacted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(basketTag)) return;

        /* Only trigger once for this collision zone. */
        if (hasImpacted) return;

        hasImpacted = true;

        if (eventRecorder != null)
        {
            eventRecorder.LogEvent(EventRecorder.EventType.Impact, gameObject.name);
            eventRecorder.PauseTimer();
        }

        if (boomMovement != null)
        {
            /* Disabling BoomMovement also disables its input actions. */
            boomMovement.enabled = false;
        }

        alertDisplay.ShowImpactMessage(impactMessage);
        debriefScreen.ShowDebriefAfterDelay("Simulation Ended: Collision", debriefDelaySeconds);

        Debug.Log("Impact detected with: " + gameObject.name);
    }
}
