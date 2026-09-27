using UnityEngine;

/* Temporary diagnostic script: periodically logs EventRecorder's
current ElapsedSeconds to the Console, so the session timer's behaviour
(starting, counting up, holding its value through a pause) can be
directly observed without the debrief screen being built yet.

Deliberately kept as its own small script, separate from click/hover
testing (UIHoverDebug) - that one has to live on a UI button to receive
Unity's EventSystem callbacks, which gets deactivated when the
instructions panel closes. This script needs the opposite: something
that stays active for the whole session, so it belongs on a persistent
object like SessionManager instead.

Remove once the actual problem is found and fixed, or once the debrief
screen makes this redundant. */

public class TimerDebug : MonoBehaviour
{
    public EventRecorder eventRecorder;

    private float logInterval = 1f;
    private float timeSinceLastLog = 0f;

    private void Update()
    {
        if (eventRecorder == null) return;

        timeSinceLastLog += Time.deltaTime;
        if (timeSinceLastLog >= logInterval)
        {
            timeSinceLastLog = 0f;
            Debug.Log($"[TimerDebug] ElapsedSeconds = {eventRecorder.ElapsedSeconds:F2}");
        }
    }
}
