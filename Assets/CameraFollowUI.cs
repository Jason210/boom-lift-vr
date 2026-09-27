using UnityEngine;

/* Keeps the alert canvas in front of the user so important messages stay visible.
Movement is smoothed to avoid the UI snapping with every small head movement,
which can feel uncomfortable in VR.

Follow camera goes in late update:
https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html

And see: https://docs.unity3d.com/ScriptReference/Vector3.Lerp.html
 */

public class CameraFollowUI : MonoBehaviour
{
    [Header("Reference")]
    [Tooltip("The VR camera the panel should stay in front of.")]
    public Transform cameraTransform;

    [Header("Positioning")]
    [Tooltip("Distance in front of the camera")]
    public float followDistance = 1.75f;

    [Tooltip("Vertical offset from the camera's forward direction.")]
    public float verticalOffset = -0.35f;

    [Tooltip("The panel stops rising / falling once the user looks up past this angle.")]
    [Range(0f, 80f)]
    public float maxPitchDegrees = 25f;

    /* Flip if the panel faces away from the user */
    private bool invertFacing = false;

    [Header("Smoothing")]
    [Tooltip("How quickly the panel catches up to the camera. Higher is snappier, lower is smoother.")]
    public float followSpeed = 4f;

    private void LateUpdate()
    {
        /* Positions the panel a fixed horizontal distance in front of the camera,
        with a small downward offset.
        
        The forward vector is adjusted so the horizontal
        distance stays same when user looks up or down. A small value 
        is used to stop by zero when looking straight up or down.        
        Uses world up for the vertical offset so head tilt does not 
        move the panel sideways. */

        Vector3 f = cameraTransform.forward;
        float flat = Mathf.Max(new Vector2(f.x, f.z).magnitude, 0.0001f);
        Vector3 offset = f * (followDistance / flat);

        /* Clamps the panel's vertical movement so it does not move too far overhead when the user looks straight up.
        It follows the gaze up to maxPitchDegrees, then keeps the same height while the gaze continues upward.
        The maximum rise is calculated each frame from the selected angle. */
        float maxRise = followDistance * Mathf.Tan(maxPitchDegrees * Mathf.Deg2Rad);
        offset.y = Mathf.Clamp(offset.y, -maxRise, maxRise);

        Vector3 targetPosition = cameraTransform.position
            + offset
            + Vector3.up * verticalOffset;

        Vector3 facing;
                if (invertFacing)
                {
                    facing = cameraTransform.position - targetPosition;
                }
                else
                {
                    facing = targetPosition - cameraTransform.position;
                }

        /* Removes the vertical component so the panel stays upright instead of tilting toward the user.
        Only left/right head movement affects its rotation; looking up/down or tilting the head is ignored. */
        facing.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(facing, Vector3.up);

        /* Damping for comfort. Eased towards target. */
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, followSpeed * Time.deltaTime);
    }
}