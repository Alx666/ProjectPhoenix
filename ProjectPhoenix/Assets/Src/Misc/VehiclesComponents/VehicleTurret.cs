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

    private Plane playerPlane;

    void Awake()
    {
        this.playerPlane = new Plane(Vector3.up, this.transform.position);
    }

    internal void UpdateRotation(Vector3 MousePosition)
    {
        //Ray ray = Camera.main.ScreenPointToRay(MousePosition);
        //RaycastHit vHit;
        //if (Physics.Raycast(ray.origin, ray.direction, out vHit, Mathf.Infinity))
        //{
        //    Quaternion targetRotation = Quaternion.LookRotation(vHit.point - AxeY.transform.position);
        //    AxeY.transform.rotation = Quaternion.Slerp(AxeY.transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        //}
        //temporaneo
        if (AxeY != null)
        {
            playerPlane.SetNormalAndPosition(Vector3.up, this.transform.position);
            Ray ray = Camera.main.ScreenPointToRay(MousePosition);
            float hitdist = 0.0f;
            if (playerPlane.Raycast(ray, out hitdist))
            {
                Vector3 targetPoint = ray.GetPoint(hitdist);
                Quaternion targetRotation = Quaternion.LookRotation(targetPoint - AxeY.transform.position);
                AxeY.transform.rotation = Quaternion.Slerp(AxeY.transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
            }
        }
    }
}
