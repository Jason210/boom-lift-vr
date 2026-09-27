using UnityEngine;

/* Controls two lamp objects to show if basket is stowed (green)
or not stowed (yellow). */

public class StowedIndicator : MonoBehaviour
{
    [Header("References")]
    public BoomMovement boomMovement;

    [Tooltip("Lit while the boom is stowed.")]
    public Renderer greenLampRenderer;

    [Tooltip("Lit while the boom is not stowed.")]
    public Renderer orangeLampRenderer;

    [Header("Colours")]
    public Color greenEmission = Color.green;
    public Color orangeEmission = new Color(1f, 0.5f, 0f);

    private void Update()
    {
        if (boomMovement == null) return;

        SetLampLit(greenLampRenderer, greenEmission, boomMovement.IsStowed);
        SetLampLit(orangeLampRenderer, orangeEmission, !boomMovement.IsStowed);
    }

    private void SetLampLit(Renderer lampRenderer, Color emissionColor, bool lit)
    {
        if (lampRenderer == null) return;

        if (lit)
        {
            lampRenderer.material.EnableKeyword("_EMISSION");
            lampRenderer.material.SetColor("_EmissionColor", emissionColor);
        }
        else
        {
            lampRenderer.material.DisableKeyword("_EMISSION");
            lampRenderer.material.SetColor("_EmissionColor", Color.black);
        }
    }
}