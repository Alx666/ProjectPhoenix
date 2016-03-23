using UnityEngine;
using System.Collections;

internal class VehicleTurret : MonoBehaviour
{
    public Transform AxeY;
    public Transform AxeX;
    public float RotationSpeed = 15;
    public float MinAngle = -45;
    public float MaxAngle = 15;

    internal void UpdateRotation(Vector3 MousePosition)
    {
        if (AxeY != null && AxeX != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(MousePosition);
            RaycastHit vHit;
            if (Physics.Raycast(ray.origin, ray.direction, out vHit, Mathf.Infinity))
            {
                Quaternion targetDirectionY = Quaternion.LookRotation(vHit.point - AxeY.transform.position);
                Vector3 targetRotationY = (Quaternion.RotateTowards(AxeY.transform.rotation, targetDirectionY, RotationSpeed)).eulerAngles;
                AxeY.transform.rotation = Quaternion.Euler(AxeY.transform.rotation.x, targetRotationY.y, AxeY.transform.rotation.z);

                Quaternion targetDirectionX = Quaternion.LookRotation(vHit.point - AxeX.transform.position);
                Vector3 targetRotationX = (Quaternion.RotateTowards(AxeX.transform.rotation, targetDirectionX, RotationSpeed)).eulerAngles;
                AxeX.transform.rotation = Quaternion.Euler(ClampAngle(targetRotationX.x, MaxAngle, MinAngle), targetRotationY.y, AxeX.transform.rotation.z);
            }
        }
    }

    internal float ClampAngle(float angle, float max, float min)
    {
        if (angle < 90 || angle > 270)
        {       // if angle in the critic region...
            if (angle > 180) angle -= 360;  // convert all angles to -180..+180
            if (max > 180) max -= 360;
            if (min > 180) min -= 360;
        }
        angle = Mathf.Clamp(angle, min, max);
        if (angle < 0) angle += 360;  // if angle negative, convert to 0..360
        return angle;
    }
}
