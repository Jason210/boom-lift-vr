using UnityEngine;

/* Keeps the basket level like a real boom lift. */

public class BasketLevelling : MonoBehaviour
{
    [Header("Reference")]
    /* Assign the GameObject with BoomMovement.cs. This provides the turret,
    telescope transforms, and boomPivot resting rotation automatically. */
    public BoomMovement boomMovement;

    [Header("Level correction")]
    /* Corrects for a non-horizontal resting boom angle. Adjust until the basket
    stays level through the full boom movement, then leave the value fixed. */
    public float levelCorrectionDegrees = 27f;

    private void LateUpdate()
    {
        /* Runs after BoomMovement updates the boom, then redoes the rotation chain
        using boomPivot's resting rotation to remove angle while preserving the
        telescope rotations. */

        Quaternion levelBoomPivotRotation =
            boomMovement.BoomPivotBaseRotation * Quaternion.Euler(levelCorrectionDegrees, 0f, 0f);

        /*  rebuild basket’s world rotation so it follows the boom’s structure but stays level. 
        See:https://discussions.unity.com/t/quaternion-multiplication-order/119632 */
        Quaternion desired =
            boomMovement.turret.rotation *
            levelBoomPivotRotation *
            boomMovement.telescope1.localRotation *
            boomMovement.telescope2.localRotation;

        transform.rotation = desired;
    }
}