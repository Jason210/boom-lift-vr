using UnityEngine;

/* Plays a looping mechanical sound for one boom axis, with volume fading
in and out based on input. This avoids sudden cuts on
short joystick taps, while still sounding smooth during continuous movement. */

public class AxisSoundVolume : MonoBehaviour
{
    public enum Axis { Yaw, HydraulicPump }

    [Header("Which boom part/axis this instance tracks")]
    public Axis axis;

    [Header("References")]
    public BoomMovement boomMovement;
    public AudioSource audioSource;
    public AudioClip loopClip;

    [Header("Fade")]
    [Tooltip("How quickly volume ramps up & down, in units per second. Higher is snappier.")]
    public float fadeSpeed = 8f;
    [Tooltip("Input volume. Below this, the part is inactive (silent).")]
    public float activeThreshold = 0.05f;
    [Tooltip("Volume while the part is moving")]
    public float activeVolume = 1f;

    [Header("Pitch under combined load (Hydraulic Pump only)")]
    [Tooltip("Normal pitch when idle or only one ram is active.")]
    public float basePitch = 1f;
    [Tooltip("Pitch drop when elevation and extension are both active at once (increased load on the shared pump.)")]
    public float dualAxisPitchDrop = 0.08f;

    private void Start()
    {
        if (audioSource != null && loopClip != null)
        {
            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.volume = 0f;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (boomMovement == null || audioSource == null) return;

        float inputMagnitude;
        float targetPitch = basePitch;

        if (axis == Axis.Yaw)
        {
            inputMagnitude = boomMovement.CurrentYawInput;
        }
        else
        {
            /* Pretend one shared hydraulic pump drives both boom and extension. It's active when either
            has input, and lowers pitch slightly when both run together to simulate the
            motor working under heavier load. */

            float elevationMagnitude = Mathf.Abs(boomMovement.CurrentElevationInput);
            float extensionMagnitude = Mathf.Abs(boomMovement.CurrentExtensionInput);
            inputMagnitude = Mathf.Max(elevationMagnitude, extensionMagnitude);

            bool bothAxesActive = elevationMagnitude > activeThreshold && extensionMagnitude > activeThreshold;

            if (bothAxesActive)
            {
                targetPitch = basePitch - dualAxisPitchDrop;
            }
            else
            {
                targetPitch = basePitch;
            }
        }

        float targetVolume;

        if (Mathf.Abs(inputMagnitude) > activeThreshold)
        {
            targetVolume = activeVolume;
        }
        else
        {
            targetVolume = 0f;
        }

        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);
        audioSource.pitch = Mathf.MoveTowards(audioSource.pitch, targetPitch, fadeSpeed * Time.deltaTime);
    }
}