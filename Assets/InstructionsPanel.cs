using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/* Displays the task instructions and controls at the start of the session.
The text is stored here instead of in the Inspector for robustness. */
public class InstructionsPanel : MonoBehaviour
{
    public TextMeshProUGUI instructionsText;
    public TextMeshProUGUI controlsLeftText;
    public TextMeshProUGUI controlsRightText;
    public TextMeshProUGUI noteText;
    public GameObject panel;

    [Tooltip("Timer starts ion EventRecorder when the panel is first dismissed (wire here).")]
    public EventRecorder eventRecorder;

    [Tooltip("BoomMovement disabled while the panel is showing, enabled when the user presses Continue.")]
    public BoomMovement boomMovement;

    [Tooltip("Left controller X button - re-shows the panel mid-session.")]
    public InputActionProperty pauseButton;

    [Tooltip("Shown when the user voluntarily ends the session.")]
    public DebriefScreen debriefScreen;

    private const string Instructions =
           "<color=#0D1B2A><b>Your task</b></color>\n" +
           "Move the basket so you can reach the red box high up on the wall behind you. Watch out for hazards. When finished return to the starting position.";

    private const string LeftControls =
           "<color=#0D1B2A><b>Left Controller</b></color>\n" +
           "<size=80%><b>Stick</b> - rotate left/right\n" +
           "<b>X button</b> - pause/cont</size>";

    private const string RightControls =
           "<color=#0D1B2A><b>Right Controller</b></color>\n" +
           "<size=80%><b>Stick</b> - boom in/out\n" +
           "<b>B button</b> - raise basket\n" +
           "<b>A button</b> - lower basket</size>";

    private const string Note =
           "<size=85%><color=#3A5A8C>\n" +
           "(Note: Controls move relative to the boom as on a real MEWP, not the direction you're facing. This means they may sometimes feel reversed, depending on your orientation.)";

    /* InputActionProperty fields assigned in the Inspector seem to need
       enabling manually. */
    private void OnEnable()
    {
        pauseButton.action?.Enable();
    }

    private void OnDisable()
    {
        pauseButton.action?.Disable();
    }

    private void Start()
    {
            ColorUtility.TryParseHtmlString("#1B418C", out Color bodyColor);

            instructionsText.color = bodyColor;
            instructionsText.fontSize = 6f;
            instructionsText.richText = true;
            instructionsText.text = Instructions;

            controlsLeftText.color = bodyColor;
            controlsLeftText.fontSize = 6f;
            controlsLeftText.richText = true;
            controlsLeftText.text = LeftControls;

            controlsRightText.color = bodyColor;
            controlsRightText.fontSize = 6f;
            controlsRightText.richText = true;
            controlsRightText.text = RightControls;

            noteText.color = bodyColor;
            noteText.fontSize = 6f;
            noteText.richText = true;
            noteText.text = Note;
      
        /* Start with the instructions visible and  movement disabled,
           matching the normal paused state. */

        panel.SetActive(true);
        
        if (boomMovement != null)
        {
            boomMovement.enabled = false;
        }
    }

    private void Update()
    {
        /* Only pause if the panel is currently hidden */
        if (panel != null && !panel.activeSelf
            && pauseButton.action != null && pauseButton.action.WasPressedThisFrame())
        {
            Pause();
        }
    }

    /* Hides the panel, enables movement and starts or resumes the timer. */
    public void Dismiss()
    {
        panel.SetActive(false);
    
        if (boomMovement != null)
        {
            boomMovement.enabled = true;
        }

        if (eventRecorder != null)
        {
            eventRecorder.StartTimer();
        }
    }

    /* Shows the panel, disables movement,  and pauses the timer. */
    public void Pause()
    {
        panel.SetActive(true);

        if (boomMovement != null)
        {
            boomMovement.enabled = false;
        }

        if (eventRecorder != null)
        {
            eventRecorder.PauseTimer();
        }
    }

    /* Hides the instructions and opens the debrief screen. */
    public void End()
    {
        panel.SetActive(false);
        debriefScreen.ShowDebrief("Simulation Not Complete");
    }
}
