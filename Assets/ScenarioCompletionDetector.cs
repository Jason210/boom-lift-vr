using UnityEngine;

/* Detects when the scenario is complete: ie the target has been reached
and the boom has returned to its stowed position.This only triggers once,
then shows the completion message and debrief. */

public class ScenarioCompletionDetector : MonoBehaviour
{
    [Header("References")]
    public BoomMovement boomMovement;
    public EventRecorder eventRecorder;
    public AlertDisplay alertDisplay;

    [Tooltip("Message shown when the scenario is completed.")]
    public string completionMessage = "Scenario Complete";

    [Tooltip("Shows debrief screen after brief completion message, or immediately if the trigger is pressed to skip ahead.")]
    public DebriefScreen debriefScreen;

    [Tooltip("How long the completion message stays up before the debrief takes over, unless skipped early.")]
    public float debriefDelaySeconds = 4f;

    private bool hasCompleted = false;

    private void Update()
    {
        if (hasCompleted) return;
        if (boomMovement == null || eventRecorder == null) return;

        /* Only complete after the target has been reached. */
        if (!eventRecorder.TargetReachedFlag) return;

        if (!boomMovement.IsStowed) return;

        hasCompleted = true;

        eventRecorder.LogEvent(EventRecorder.EventType.ScenarioComplete, "Boom stowed after target reached");
        eventRecorder.PauseTimer();

        alertDisplay.ShowCompletionMessage(completionMessage);
        debriefScreen.ShowDebriefAfterDelay("Simulation Complete", debriefDelaySeconds);
        
    }
}
