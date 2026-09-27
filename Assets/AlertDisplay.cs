using System.Collections;
using UnityEngine;
using TMPro;

/* For shared alert canvas, for other scripts to call into */

/* References for beep generator:
https://docs.unity.com/en-us/engine/6000.7/script-reference/unityengine/audioclip/create
and here:
https://gist.github.com/MirzaBeig/639fe29d075703287fb82b37e2ad6c2f
*/

public class AlertDisplay : MonoBehaviour
{
    [Header("Hazard warning")]
    public GameObject hazardPanel;
    public TextMeshProUGUI hazardText;

    [Header("Hazard beep")]
    [Tooltip("Plays a beep, like a car proximity sensor, while inside a hazard zone.")]
    public AudioSource audioSource;
    [Tooltip("Beep time interval.")]
    public float beepInterval = 0.6f;
    [Tooltip("Beep pitch in Hz.")]
    public float generatedBeepFrequency = 800f;
    [Tooltip("Length of beep in seconds")]
    public float generatedBeepDuration = 0.15f;

    private AudioClip beepClip;
    private Coroutine beepCoroutine;

    [Header("Target reached")]
    public GameObject targetPanel;
    public TextMeshProUGUI targetText;

    [Header("Scenario complete")]
    [Tooltip("Scripted but you can override it here")]
    public GameObject completionPanel;
    public TextMeshProUGUI completionText;

    [Header("Impact / session end")]
    public GameObject impactPanel;
    public TextMeshProUGUI impactText;

    [Header("Impact alarm")]
    [Tooltip("Plays when impact occurs.")]
    public AudioSource impactAudioSource;
    [Tooltip("Pitch (default 500)")]
    public float generatedAlarmFrequency = 500f;

    private AudioClip impactAlarmClip;

    /* Track overlapping hazard/target zones so the panel stays visible
    until the last active zone is exited. */
private int activeHazardCount = 0;
    private int activeTargetCount = 0;

    private void OnEnable()
    {
        beepCoroutine = null;
    }

    private void Start()
    {
        hazardPanel.SetActive(false);
        targetPanel.SetActive(false);
        completionPanel.SetActive(false);
        impactPanel.SetActive(false);

        beepClip = GenerateBeepClip(generatedBeepFrequency, generatedBeepDuration);
        impactAlarmClip = GenerateLoopableTone(generatedAlarmFrequency);
    }

    /* Generate a short beep and fade the ends to avoid clicks.*/
    private AudioClip GenerateBeepClip(float frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        int fadeSamples = Mathf.Min(sampleCount / 4, sampleRate / 100); // up to ~10ms fade

        for (int i = 0; i < sampleCount; i++)
        {
            float envelope = 1f;

            if (i < fadeSamples)
            {
                envelope = (float)i / fadeSamples;
            }
            else if (i > sampleCount - fadeSamples)
            {
                envelope = (float)(sampleCount - i) / fadeSamples;
            }

            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * envelope;
        }

        AudioClip clip = AudioClip.Create("GeneratedBeep", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    /* Generate one sine-wave cycle so the alarm can loop smoothly.*/
    private AudioClip GenerateLoopableTone(float frequency)
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.RoundToInt(sampleRate / frequency);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate);
        }

        AudioClip clip = AudioClip.Create("GeneratedAlarm", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);

        return clip;
    }

    public void ShowHazardWarning(string message)
    {
        activeHazardCount++;

        hazardPanel.SetActive(true);
        hazardText.text = message;

        /* Start the beep loop only when the first hazard becomes active.
        to avoid stacking / overlapping */
        if (activeHazardCount == 1 && beepCoroutine == null)
        {
            beepCoroutine = StartCoroutine(BeepRepeatedly());
        }
    }

    public void HideHazardWarning()
    {
        activeHazardCount--;

        if (activeHazardCount < 0)
        {
            activeHazardCount = 0;
        }

        if (activeHazardCount == 0)
        {
            hazardPanel.SetActive(false);

            if (beepCoroutine != null)
            {
                StopCoroutine(beepCoroutine);
                beepCoroutine = null;
            }
        }
    }

    private IEnumerator BeepRepeatedly()
    {
        while (true)
        {
            audioSource.PlayOneShot(beepClip);
            yield return new WaitForSeconds(beepInterval);
        }
    }

    public void ShowTargetMessage(string message)
    {
        activeTargetCount++;

        targetPanel.SetActive(true);
        targetText.text = message;
    }

    public void HideTargetMessage()
    {
        activeTargetCount--;

        if (activeTargetCount < 0)
        {
            activeTargetCount = 0;
        }

        if (activeTargetCount == 0)
        {
            targetPanel.SetActive(false);
        }
    }

    /* A single, one-time message. This one doesn't need
    counting, since it only happens once. */
    public void ShowCompletionMessage(string message)
    {
        completionPanel.SetActive(true);
        completionText.text = message;
    }

    /* Impact ends the session, so clear the other messages first. */
    public void ShowImpactMessage(string message)
    {
        activeHazardCount = 0;
        activeTargetCount = 0;

        HideHazardWarning();
        HideTargetMessage();
        completionPanel.SetActive(false);

        impactPanel.SetActive(true);
        impactText.text = message;

        impactAudioSource.clip = impactAlarmClip;
        impactAudioSource.loop = true;
        impactAudioSource.Play();
    }

    /* Clears the impact panel and stops the continuous alarm. */
    public void HideImpactMessage()
    {
        impactPanel.SetActive(false);
        impactAudioSource.Stop();
    }

    public void HideCompletionMessage()
    {
        completionPanel.SetActive(false);
    }
}