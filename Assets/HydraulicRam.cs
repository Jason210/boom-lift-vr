using UnityEngine;

/* Attach this script to boom_cylinder! */

/* Animates the hydraulic ram so it follows the boom as it moves.
Sources for this: 
https://discussions.unity.com/t/draw-cylinder-between-2-points/392309
and 
https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Quaternion.LookRotation.html

Unity setup:
1. Parent boom_piston under boom_cylinder.
2. Create an empty GameObject at the piston's original attachment point,
   parent it to main_boom, and assign it as pistonAnchor.

Each frame, the cylinder points at pistonAnchor and the piston extends
along the cylinder to meet it. */

public class HydraulicRam : MonoBehaviour
{
    [Header("Rig references")]
    public Transform cylinder;      // boom_cylinder
    public Transform piston;        // boom_piston, child of cylinder
    public Transform pistonAnchor;  // attachment point on main_boom

    [Header("Outward axis")]

    /* Local axis pointing from the cylinder toward the piston. */
    public Vector3 outwardAxis = new Vector3(0f, 1f, 0f);

    /* Reference direction used to control roll. */
    public Vector3 upHint = Vector3.zero;

    /* piston extension adjustment */
    private float extensionOffset = -0.8f;

    /* local Z adjustment. */
    private float localZOffset = -0.008f;

    private void LateUpdate()
    {
        /* Runs after BoomMovement has finished updating the boom position. */
        if (cylinder == null || piston == null || pistonAnchor == null)
        {
            return;
        }

        /* Find the direction from the cylinder to the attachment point
           on the moving boom. */
        Vector3 toTarget = pistonAnchor.position - cylinder.position;

        /* The length of that vector gives the distance the piston needs
           to cover. */
        float distance = toTarget.magnitude;

        /* so we don't calculate a rotation if the two points are
           effectively in the same position. */
        if (distance < 0.0001f)
        {
            return;
        }

        /* Imported cylinder model might not extend along
           Unity's normal +Z forward axis.
           'outwardAxis' specifies which local axis of the model really
           points outward toward the piston. */
        Vector3 cylinderOutwardDirection = outwardAxis.normalized;

        /* Quaternion.LookRotation expects +Z to be the object's forward
        direction. Create a correction rotation which converts the
        cylinder's real outward axis into the +Z direction expected by 'LookRotation'. */
        Quaternion axisCorrection = Quaternion.FromToRotation(cylinderOutwardDirection, Vector3.forward);

        /* Convert the direction toward the piston anchor into a unit
        direction. We only need its direction here, not the distance. */
        Vector3 directionToAnchor = toTarget.normalized;

        /* Calculate world-space rotation that points Unity's normal
           forward (+Z) direction toward the piston anchor.
           'upHint' is supplied to control cylinder roll. */
        Quaternion rotationTowardAnchor = Quaternion.LookRotation(directionToAnchor, upHint);

        /* Combine target rotation with the correction needed so cylinder's
        actual outward axis points toward the piston anchor */
        Quaternion finalCylinderRotation = rotationTowardAnchor * axisCorrection;

        /* Apply the completed rotation to the cylinder. */
        cylinder.rotation = finalCylinderRotation;

        /* So piston reaches the anchor
         'extensionOffset' was determined by testing exposed value in the Inspector */
        float pistonExtension = distance + extensionOffset;

        /* Move along the cylinder's own outward axis. */
        Vector3 extensionAlongCylinder = cylinderOutwardDirection * pistonExtension;

        /* Imported model needs an z offset to
           keep the piston visually aligned with the cylinder. */
        Vector3 alignmentOffset = new Vector3(0f, 0f, localZOffset);

        /* Combine the extension and alignment adjustment and apply them
           as the piston's local position. */
        piston.localPosition = extensionAlongCylinder + alignmentOffset;
    }
}