using UnityEngine;
using System.Collections;

internal class VehicleTurret : MonoBehaviour
{
    [SerializeField]
    internal Transform AxeY;
    [SerializeField]
    internal Transform AxeX;
    [SerializeField]
    internal float RotationSpeed = 15;
    [SerializeField]
    internal float MinAngle = -45;
    [SerializeField]
    internal float MaxAngle = 15;

    public GameObject BulletLocator;

    private Plane playerPlane;

    void Awake()
    {
        this.playerPlane = new Plane(Vector3.up, this.transform.position);
    }

    internal void UpdateRotation(Vector3 MousePosition)
    {
        //if (AxeY != null)
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(MousePosition);
        //    RaycastHit vHit;
        //    if (Physics.Raycast(ray.origin, ray.direction, out vHit, Mathf.Infinity))
        //    {
        //        Vector3 vDirection = vHit.point;// - AxeY.transform.position;
        //        Quaternion targetRotation = Quaternion.LookRotation(vDirection);
        //        AxeY.transform.localRotation = Quaternion.RotateTowards(AxeY.transform.localRotation, targetRotation, RotationSpeed);
        //        AxeY.transform.localRotation = Quaternion.Euler(0f, AxeY.transform.localRotation.eulerAngles.y, 0f);

        //        Debug.Log(vDirection); //altezza massima obj
        //        // BulletLocator.transform.localRotation = Quaternion.RotateTowards(AxeX.transform.localRotation, targetRotation, RotationSpeed);
        //        // BulletLocator.transform.localRotation = Quaternion.Euler(BulletLocator.transform.localRotation.x, 0f,0f);
        //        Quaternion provvisorio = BulletLocator.transform.localRotation;
        //        provvisorio = Quaternion.Slerp(BulletLocator.transform.localRotation, targetRotation, RotationSpeed);
        //        provvisorio.y = 0;
        //        provvisorio.z = 0;
        //        BulletLocator.transform.localRotation = provvisorio;
        //    }
        //}

        //temporaneo
        //if (AxeY != null)
        //{
        //    //playerPlane.SetNormalAndPosition(Vector3.up, this.transform.position);
        //    //Ray ray = Camera.main.ScreenPointToRay(MousePosition);
        //    //float hitdist = 0.0f;
        //    //if (playerPlane.Raycast(ray, out hitdist))
        //    //{
        //    //    Vector3 targetPoint = ray.GetPoint(hitdist);
        //    //    Quaternion targetRotation = Quaternion.LookRotation(targetPoint - AxeY.transform.position);
        //    //    AxeY.transform.rotation = Quaternion.Slerp(AxeY.transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        //    //}
        //}

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
