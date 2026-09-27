using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

/* Displays the session debrief, summary and exit options.
Reads results directly from EventRecorder.
*/


public class DebriefScreen : MonoBehaviour
{
    [Header("Debrief")]
    public TextMeshProUGUI debriefText;
    public GameObject panel;
    public EventRecorder eventRecorder;

    [Tooltip("Cleared before the debrief shows, so nothing from the old message system stays underneath it.")]
    public AlertDisplay alertDisplay;

    [Header("Reflection")]
    public GameObject reflectionPanel;
    public TextMeshProUGUI reflectionQuestion;
    public TextMeshProUGUI reflectionMessage;
    public TextMeshProUGUI ansButton1Text;
    public TextMeshProUGUI ansButton2Text;
    public TextMeshProUGUI ansButton3Text;

    [Header("Skip trigger (Impact/Completion delay)")]
    [Tooltip("Left controller trigger - skips the delay in ShowDebriefAfterDelay, used only while that delay is actually running.")]
    public InputActionProperty skipTriggerLeft;

    [Tooltip("Right controller trigger - same purpose as Skip Trigger Left, either hand works.")]
    public InputActionProperty skipTriggerRight;

    /* Kept as constants to prevent Inspector accident from causing blank
    runtime messages.of needing to edit the script(not the Inspector) to
    change wording. */
    private const string ReflectionQuestion =
        "Do you feel ready to continue toward real MEWP training or use?";

    private const string AnswerReadyLabel = "Yes, I feel ready";
    private const string AnswerSomewhatLabel = "Somewhat, but I'd want more practice";
    private const string AnswerNoLabel = "No, I still feel unsure";

    private const string AcknowledgmentReadyClean =
        "You said you feel ready to continue toward real MEWP training or use. That's a reasonable response, given that this session went so smoothly.";
    private const string AcknowledgmentReadyCollision =
        "You said you feel ready to continue toward real MEWP training or use. This session included a collision, which is worth keeping in mind as you think about next steps.";
    private const string AcknowledgmentSomewhatClean =
        "You said you'd want more practice before continuing. That's a reasonable response. You performed well in this session.";
    private const string AcknowledgmentSomewhatCollision =
        "You said you'd want more practice before continuing. That's a completely reasonable response, especially given what happened this session.";
    private const string AcknowledgmentNoClean =
        "You said you do not feel ready. That's fine. You performed well in this session.";
    private const string AcknowledgmentNoCollision =
        "You said you still feel unsure. That's a completely reasonable response, especially given what happened this session.";

    /* Tracks whether the session ended early so reflection uses dedicated messages
    instead of the normal completion or collision variants. */

    private string lastStatus = "";

    private const string EndedEarlyTargetNotReached =
        "You aborted before reaching the target. Was there a reason for this?";
    private const string EndedEarlyTargetReached =
        "You reached the target, without any collisions, then ended the simulation before returning the basket. In a real MEWP, you need to bring the basket back. Good so far.";

    private void OnEnable()
    {
        skipTriggerLeft.action?.Enable();
        skipTriggerRight.action?.Enable();
    }

    private void OnDisable()
    {
        skipTriggerLeft.action?.Disable();
        skipTriggerRight.action?.Disable();
    }

    private void Start()
    {
        Debug.Log("[DebriefScreen] Start() running - component (re)initialized");

        if (debriefText != null)
        {
            /* Same styling as InstructionsPanel matching fonts and
            colours so all these panels have same visual look. */
            ColorUtility.TryParseHtmlString("#1B418C", out Color debriefBodyColor);
            debriefText.color = debriefBodyColor;

            debriefText.fontSize = 6f;
            debriefText.richText = true;
        }

        if (reflectionQuestion != null)
        {
            reflectionQuestion.fontSize = 6f;
            ColorUtility.TryParseHtmlString("#1B418C", out Color questionColor);
            reflectionQuestion.color = questionColor;
            reflectionQuestion.richText = true;
            reflectionQuestion.text = ReflectionQuestion;
        }

        if (reflectionMessage != null)
        {
            reflectionMessage.fontSize = 6f;
            ColorUtility.TryParseHtmlString("#1B418C", out Color messageColor);
            reflectionMessage.color = messageColor;
            reflectionMessage.richText = true;
            reflectionMessage.text = "";
        }

        if (ansButton1Text != null) ansButton1Text.text = AnswerReadyLabel;
        if (ansButton2Text != null) ansButton2Text.text = AnswerSomewhatLabel;
        if (ansButton3Text != null) ansButton3Text.text = AnswerNoLabel;

        /* Hidden by default. The debrief should not appear before
        before anything has actually happened. ShowDebrief() is called
        by whatever ends the session:- Impact, natural completion, or
        the End button. */
        panel.SetActive(false);
        reflectionPanel.SetActive(false);
    }

    public void ShowDebrief(string status)
    {
        Debug.Log($"[DebriefScreen] ShowDebrief called with status = '{status}'");
        lastStatus = status;

        /* Clears any active messages before showing the debrief
        preventing old UI or audio from carrying over. */

        alertDisplay.HideHazardWarning();
        alertDisplay.HideTargetMessage();
        alertDisplay.HideImpactMessage();
        alertDisplay.HideCompletionMessage();

        panel.SetActive(true);

        var (targetReached, hazardCount, collisionOccurred, elapsedSeconds) = GetSessionFacts();

        debriefText.text = BuildSummary(status, targetReached, hazardCount, collisionOccurred, elapsedSeconds);

    }

    /* Keeps the impact or completion message visible for a few seconds before the debrief screen,
    unless a controller trigger is pressed to skip ahead. */

    public void ShowDebriefAfterDelay(string status, float delaySeconds)
    {
        StartCoroutine(ShowDebriefAfterDelayRoutine(status, delaySeconds));
    }

    private IEnumerator ShowDebriefAfterDelayRoutine(string status, float delaySeconds)
    {
        float elapsed = 0f;
        while (elapsed < delaySeconds)
        {
            bool leftPressed = skipTriggerLeft.action != null && skipTriggerLeft.action.WasPressedThisFrame();
            bool rightPressed = skipTriggerRight.action != null && skipTriggerRight.action.WasPressedThisFrame();

            if (leftPressed || rightPressed)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        ShowDebrief(status);
    }

    /* Reads the session results from EventRecorder and stores it for reuse by the
    debrief summary and reflection messages. */

    private (bool targetReached, int hazardCount, bool collisionOccurred, float elapsedSeconds) GetSessionFacts()
    {
        if (eventRecorder == null)
        {
            return (false, 0, false, 0f);
        }

        bool targetReached = eventRecorder.TargetReachedFlag;

        int hazardCount = 0;
        bool collisionOccurred = false;

        foreach (var recordedEvent in eventRecorder.Events)
        {
            if (recordedEvent.type == EventRecorder.EventType.HazardEnter)
            {
                hazardCount++;
            }

            if (recordedEvent.type == EventRecorder.EventType.Impact)
            {
                collisionOccurred = true;
            }
        }

        float elapsedSeconds = eventRecorder.ElapsedSeconds;

        return (targetReached, hazardCount, collisionOccurred, elapsedSeconds);
    }

    private string BuildSummary(string status, bool targetReached, int hazardCount, bool collisionOccurred, float elapsedSeconds)
    {
        return
            $"<color=#0D1B2A><b>{status}</b></color>\n\n" +
            $"Target reached: {(targetReached ? "Yes" : "No")}\n" +
            $"Hazard warnings: {hazardCount} times\n" +
            $"Collision: {(collisionOccurred ? "Yes" : "No")}\n" +
            $"Time taken: {elapsedSeconds:F0} seconds";
    }

    /* Called by the 'Reflect on Session' button's OnClick() event. The
    debrief panel and its summary stay untouched underneath.*/
    public void ShowReflectionPanel()
    {
        panel.SetActive(false);
        reflectionPanel.SetActive(true);
    }

    /* Called by the Back button's OnClick() event.*/
    public void BackToDebrief()
    {
        reflectionPanel.SetActive(false);
        panel.SetActive(true);
    }

    /* Called by each of the three answer buttons' OnClick() events.*/
    public void AnswerReady()
    {
        if (TryShowEndedEarlyMessage()) return;

        var facts = GetSessionFacts();
        string acknowledgment = facts.collisionOccurred ? AcknowledgmentReadyCollision : AcknowledgmentReadyClean;
        ShowReflectionMessage(acknowledgment);
    }

    public void AnswerSomewhat()
    {
        if (TryShowEndedEarlyMessage()) return;

        var facts = GetSessionFacts();
        string acknowledgment = facts.collisionOccurred ? AcknowledgmentSomewhatCollision : AcknowledgmentSomewhatClean;
        ShowReflectionMessage(acknowledgment);
    }

    public void AnswerNo()
    {
        if (TryShowEndedEarlyMessage()) return;

        var facts = GetSessionFacts();
        string acknowledgment = facts.collisionOccurred ? AcknowledgmentNoCollision : AcknowledgmentNoClean;
        ShowReflectionMessage(acknowledgment);
    }

    /* Opens the reflection question without hiding the debrief or its summary.
    alos handles sessions ended early via the "End" button, using a reflection message. */
    private bool TryShowEndedEarlyMessage()
    {
        Debug.Log($"[DebriefScreen] TryShowEndedEarlyMessage checking lastStatus = '{lastStatus}'");

        if (lastStatus != "Simulation Not Complete") return false;

        bool targetReached = GetSessionFacts().targetReached;
        string message = targetReached ? EndedEarlyTargetReached : EndedEarlyTargetNotReached;
        ShowReflectionMessage(message);
        return true;
    }

    /* Shows the selected reflection message directly, without repeating facts already
    covered by the message itself and the debrief summary. */

    private void ShowReflectionMessage(string acknowledgment)
    {
        if (reflectionMessage == null) return;
        reflectionMessage.text = acknowledgment;
    }

    /* Called by the Close App button's OnClick() event.*/
    public void CloseApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /* Called by the Run Simulation Again button's OnClick() event.
    Reloads the current scene entirely so everything resets */
    public void RunAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}