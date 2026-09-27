using UnityEngine;
using UnityEngine.InputSystem;

/*Drives boom-lift rig: turret yaw, boom elevation, and 
synchronized two-stage telescope extension. All kinematic
including Basket's Rigidbody.*/

public class BoomMovement : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty leftJoystick;   //  x-axis = turret yaw
    public InputActionProperty rightJoystick;  // y-axis = telescope extension
    public InputActionProperty raiseButton;    // boom elevation up
    public InputActionProperty lowerButton;    // boom elevation down

    [Header("Rig references")]
    public Transform turret;       // local Y (yaw)
    public Transform boomPivot;    // main arm - local X (elevation)
    public Transform telescope1;   // first telescopic part
    public Transform telescope2;   // second telescopic, (is parent to basket)

    [Header("Speeds")]
    public float yawSpeed = 1f;         // degrees per second
    public float elevationSpeed = 1f;  
    public float extensionSpeed = 0.1f;   //mps

    [Header("Yaw easing")]
    [Tooltip("Time to reach full yaw speed")]
    public float yawAccelerationTime = 0.3f;
    [Tooltip("Time to stop from full yaw speed.")]
    public float yawDecelerationTime = 0.4f;

    [Header("Extension easing")]
    [Tooltip("Time to reach full extension speed.")]
    public float extensionAccelerationTime = 0.3f;
    [Tooltip("Time to stop from full extension speed.")]
    public float extensionDecelerationTime = 0.4f;

    [Header("Stowed detection")]
    [Tooltip("Elevation (degrees) below which the boom counts as retracted.")]
    public float stowedElevationThreshold = 5f;
    [Tooltip("Extension value at or above which the basket counts as retracted.")]
    public float stowedExtensionThreshold = -0.5f;
    [Tooltip("Yaw (degrees) within which the turret counts as centred.")]
    public float stowedYawThreshold = 5f;

    [Header("Elevation easing")]
    [Tooltip("Time to reach full elevation")]
    public float elevationAccelerationTime = 0.3f;
    [Tooltip("Time to stop from full elevation speed.")]
    public float elevationDecelerationTime = 0.4f;

    [Header("Rotation axes")]

    /* Hard-coded, confirmed by in-game testing
    Turret's base rotation includes a baked (-90, 0, 90)
    correction from the Max/Unity FBX axis conversion (Z-up to Y-up).
    Doing Y-axis offset onto that base did NOT produce true
    vertical yaw. Tested: Y (wrong axis) -> X (wrong axis) -> Z
    (correct axis, but wrong direction!) -> Z negated (confirmed
    correct). This is specific to this model's baked correction, which
    is why it needed hands-on testing rather than being derivable
    from the Euler values alone.*/

    [Header("Input shaping")]
    public float deadzone = 0.15f;

    [Header("Clamped ranges")]
    public float minYaw = -180f;
    public float maxYaw = 180f;

    public float minElevation = 0.1f;
    public float maxElevation = 75f;

    /* Segment extension range. Both segments move together, so total
    reach gained is double this. Values were found by
    trial and error watching the model from a detached camera */

    public float extensionLimitExtended = -3f;
    public float extensionLimitRetracted = -0.03f;

    /* Tracks state separately so input accumulates smoothly without reading
    rotation back from the transform, especially past 180 degrees. */
    private float currentYaw;
    private float currentElevation;
    private float currentExtension;

    /* Eased version of the raw yaw input (-1..1), ramped toward
    rawYaw each frame rather than snapping to it */
    private float currentYawInput;

    /* Eased version of the raw extension input (-1 , 1).*/
    private float currentExtensionInput;

    /* Eased version of elevationInput (-1, 0, or 1 depending on
    which button is held).The target here is fixed rather
    than changing since it's a button not a joystick it doesn't
    matter to the easing logic, it still smooths the transition
    between states.*/
    private float currentElevationInput;

    /* Exposes the tracked yaw for other scripts without reading it back from
    the transform- avoids issues caused by the turret's axis correction. */
    public float CurrentYaw => currentYaw;

    /* True while the boom is in its stowed position: turret centred, elevation
    low, and telescope retracted. Updates continuously for use by other parts of 
    app */
    public bool IsStowed =>
        Mathf.Abs(currentYaw) <= stowedYawThreshold &&
        currentElevation <= stowedElevationThreshold &&
        currentExtension >= stowedExtensionThreshold;

    /* Exposes each axis smoothed input (-1..1) for other systems, such as
    matching mechanical sound volume to movement. */
    public float CurrentYawInput => currentYawInput;
    public float CurrentElevationInput => currentElevationInput;
    public float CurrentExtensionInput => currentExtensionInput;

    /* Exposes boomPivots resting rotation so other scripts can rebuild the
    boom rotation without elevation - see BasketLeveling.cs.*/
    public Quaternion BoomPivotBaseRotation => boomPivotBaseRotation;

    /*Local starting positions of each telescope segment, so extension
    is applied as an offset from where they started, not from zero.*/
    private Vector3 telescope1StartPos;
    private Vector3 telescope2StartPos;

    /*Each part's starting local rotation, captured once at Start(). Yaw
    and elevation are then applied as rotations relative to this base,
    rather than as absolute angles*/
    private Quaternion turretBaseRotation;
    private Quaternion boomPivotBaseRotation;

    private void OnEnable()
    {
        /* fields wired up in the Inspector aren't
        enabled automatically by Unity, so I had to enable them
        here.*/
        leftJoystick.action?.Enable();
        rightJoystick.action?.Enable();
        raiseButton.action?.Enable();
        lowerButton.action?.Enable();
    }

    private void OnDisable()
    {
        /* Not sure if necessary but this is what
        Unity's Input System documentation recommends when you're manually
        enabling actions rather than using a PlayerInput component! */
        leftJoystick.action?.Disable();
        rightJoystick.action?.Disable();
        raiseButton.action?.Disable();
        lowerButton.action?.Disable();
    }

    private void Awake()
    {
        /* Capture the boom's starting pos in Awake so it is ALWAYS initialized,
        even if this component is disabled before Start runs. Yaw and elevation
        are stored as offsets from this position, so both begin at zero. */

        currentYaw = 0f;
        currentElevation = 0.1f; // best position based on trial and error
        currentExtension = 0f;
        currentYawInput = 0f;
        currentExtensionInput = 0f;
        currentElevationInput = 0f;

        telescope1StartPos = telescope1.localPosition;
        telescope2StartPos = telescope2.localPosition;

        turretBaseRotation = turret.localRotation;
        boomPivotBaseRotation = boomPivot.localRotation;
    }

    private void Update()
    {
        // Read raw input
        Vector2 leftInput;
        Vector2 rightInput;

        if (leftJoystick.action != null)
        {
            leftInput = leftJoystick.action.ReadValue<Vector2>();
        }
        else
        {
            leftInput = Vector2.zero;
        }
        
        if (rightJoystick.action != null)
        {
            rightInput = rightJoystick.action.ReadValue<Vector2>();
        }
        else
        {
            rightInput = Vector2.zero;
        }

                float rawYaw = leftInput.x;
        float rawExtension = rightInput.y;

        // joystick deadzone
        if (Mathf.Abs(rawYaw) < deadzone) rawYaw = 0f;
        if (Mathf.Abs(rawExtension) < deadzone) rawExtension = 0f;

        bool raising = raiseButton.action != null && raiseButton.action.IsPressed();
        bool lowering = lowerButton.action != null && lowerButton.action.IsPressed();

        float elevationInput;
        if (raising && !lowering) elevationInput = 1f;
        else if (lowering && !raising) elevationInput = -1f;
        else elevationInput = 0f;

        /* eases yaw input toward the joystick value using separate
        acceleration and deceleration rates. */
        bool yawAccelerating = Mathf.Abs(rawYaw) > Mathf.Abs(currentYawInput);
        float yawTimeToFull;

        // speeding up or slowing down?
        if (yawAccelerating)
        {
            yawTimeToFull = yawAccelerationTime;
        }
        else
        {
            yawTimeToFull = yawDecelerationTime;
        }

        float yawRate;

        if (yawTimeToFull > 0f)
        {
            yawRate = 1f / yawTimeToFull;
        }
        else
        {
            yawRate = Mathf.Infinity;
        }

        /* MoveTowards() moves the current input toward the target by a maximum amount each frame.
        1 / accelerationTime converts "seconds to reach full input" into "input units per second" */
        currentYawInput = Mathf.MoveTowards(
            currentYawInput,
            rawYaw,
            yawRate * Time.deltaTime
        );

        bool extensionAccelerating =
            Mathf.Abs(rawExtension) > Mathf.Abs(currentExtensionInput);

        float extensionTimeToFull;

        if (extensionAccelerating)
        {
            extensionTimeToFull = extensionAccelerationTime;
        }
        else
        {
            extensionTimeToFull = extensionDecelerationTime;
        }

        float extensionRate;

        if (extensionTimeToFull > 0f)
        {
            extensionRate = 1f / extensionTimeToFull;
        }
        else
        {
            extensionRate = Mathf.Infinity;
        }

        currentExtensionInput = Mathf.MoveTowards(
            currentExtensionInput,
            rawExtension,
            extensionRate * Time.deltaTime
        );

        bool elevationAccelerating =
            Mathf.Abs(elevationInput) > Mathf.Abs(currentElevationInput);

        float elevationTimeToFull;

        if (elevationAccelerating)
        {
            elevationTimeToFull = elevationAccelerationTime;
        }
        else
        {
            elevationTimeToFull = elevationDecelerationTime;
        }

        float elevationRate;

        if (elevationTimeToFull > 0f)
        {
            elevationRate = 1f / elevationTimeToFull;
        }
        else
        {
            elevationRate = Mathf.Infinity;
        }

        currentElevationInput = Mathf.MoveTowards(
            currentElevationInput,
            elevationInput,
            elevationRate * Time.deltaTime
        );

        // Update current state clamped to safe ranges
        currentYaw += currentYawInput * yawSpeed * Time.deltaTime;
        currentYaw = Mathf.Clamp(currentYaw, minYaw, maxYaw);

        currentElevation += currentElevationInput * elevationSpeed * Time.deltaTime;
        currentElevation = Mathf.Clamp(currentElevation, minElevation, maxElevation);

        currentExtension += currentExtensionInput * extensionSpeed * Time.deltaTime;
        currentExtension = Mathf.Clamp(currentExtension, extensionLimitExtended, extensionLimitRetracted);

        /* take the object's original/baked local rotation, then apply an additional rotation as an offset.
        see: https://discussions.unity.com/t/rotate-but-keep-initial-offset/628393 */
        turret.localRotation = turretBaseRotation * Quaternion.Euler(0f, 0f, -currentYaw);
        boomPivot.localRotation = boomPivotBaseRotation * Quaternion.Euler(currentElevation, 0f, 0f);

        /* Both telescope segments extend by the same amount, which
        matches how a real booms extend: both stages driven together, not one
        after the other. Local Y. Tried z at first but testing indicated Y.
        Note the negatives were necessary to get the telescopic part to move in the
        right direction (trial and error again.)*/

        telescope1.localPosition = telescope1StartPos + Vector3.up * -currentExtension;
        telescope2.localPosition = telescope2StartPos + Vector3.up * -currentExtension;
    }
}