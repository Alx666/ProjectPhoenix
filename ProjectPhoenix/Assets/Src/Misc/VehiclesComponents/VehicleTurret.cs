using UnityEngine;
using System.Collections;

internal class VehicleTurret : MonoBehaviour
{
    [SerializeField]
    internal Transform AxeY;
    [SerializeField]
    internal Transform AxeX;
    [SerializeField]
    internal float RotationSpeed;

    public Vector3 debugRotation;
    public Vector3 debugScreenDistance;
    public Vector3 debugNewRot;

    internal void UpdateRotation(Vector3 MousePosition)
    {
        Vector3 screenDistance = (MousePosition - (Camera.main.WorldToScreenPoint(this.transform.position))).normalized;
        //Vector3 newRotation = (Quaternion.RotateTowards(AxeY.transform.rotation, Quaternion.LookRotation(direction), RotationSpeed)).eulerAngles;
        //AxeY.transform.rotation = Quaternion.Euler(AxeY.transform.rotation.x, newRotation.y, AxeY.transform.rotation.z);

        Vector3 newRot = (Quaternion.RotateTowards(AxeY.transform.rotation, Quaternion.LookRotation(screenDistance), RotationSpeed)).eulerAngles;
        AxeY.transform.rotation = Quaternion.Euler(AxeY.transform.rotation.x, newRot.y, AxeY.transform.rotation.z);

        debugRotation = AxeY.transform.rotation.eulerAngles;
        debugScreenDistance = screenDistance;
        debugNewRot = newRot;
    }
}
