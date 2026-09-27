using System;
using System.Collections.Generic;
using UnityEngine;

/* Tracks everything that happens as the data
source for debrief screen and anything else that might need it. */

public class EventRecorder : MonoBehaviour
{
    public enum EventType
    {
        HazardEnter,
        HazardExit,
        TargetReached,
        Impact,
        ScenarioComplete
    }

    [Serializable]
    public struct LoggedEvent
    {
        public EventType type;
        public float timeSeconds;
        public string detail;
    }

    private readonly List<LoggedEvent> events = new List<LoggedEvent>();

    public IReadOnlyList<LoggedEvent> Events => events;

    /* True once the target has been reached at least once this
    session. This is needed because if user ends early it will
    not count as completing the scenario. */
    public bool TargetReachedFlag { get; private set; } = false;

    /* Session timer state. Elapsed time is calculated only when requested,
    since nothing needs it updated every frame.*/

    private bool timerRunning = false;
    private float timerStartTime = 0f;
    private float accumulatedSeconds = 0f;

    public void StartTimer()
    {
        if (timerRunning) return;
        timerRunning = true;
        timerStartTime = Time.time;
    }
    /* Adds the elapsed time to the total and stops the timer without
    resetting it, so timing continues correctly after a pause or resume.
    pause panel) holds the running total rather than losing it. */
    public void PauseTimer()
    {
        if (!timerRunning) return;
        accumulatedSeconds += Time.time - timerStartTime;
        timerRunning = false;
    }

    public float ElapsedSeconds =>
        timerRunning ? accumulatedSeconds + (Time.time - timerStartTime) : accumulatedSeconds;

    public void LogEvent(EventType type, string detail = "")
    {
        if (type == EventType.TargetReached)
        {
            TargetReachedFlag = true;
        }

        LoggedEvent entry = new LoggedEvent
        {
            type = type,
            timeSeconds = Time.timeSinceLevelLoad,
            detail = detail
        };
        events.Add(entry);
        Debug.Log($"[EventRecorder] {type} at {entry.timeSeconds:F1}s {detail}");
    }
}