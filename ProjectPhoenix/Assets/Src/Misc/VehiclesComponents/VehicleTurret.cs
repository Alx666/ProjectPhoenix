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

    public GameObject BulletLocator;

    private Plane playerPlane;

    void Awake()
    {
        this.playerPlane = new Plane(Vector3.up, this.transform.position);
    }

    internal void UpdateRotation(Vector3 MousePosition)
    {
        if (AxeY != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(MousePosition);
            RaycastHit vHit;
            if (Physics.Raycast(ray.origin, ray.direction, out vHit, Mathf.Infinity))
            {
                Vector3 vDirection = vHit.point;// - AxeY.transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(vDirection);
                AxeY.transform.localRotation = Quaternion.RotateTowards(AxeY.transform.localRotation, targetRotation, RotationSpeed);
                AxeY.transform.localRotation = Quaternion.Euler(0f, AxeY.transform.localRotation.eulerAngles.y, 0f);

                Debug.Log(vDirection); //altezza massima obj
                // BulletLocator.transform.localRotation = Quaternion.RotateTowards(AxeX.transform.localRotation, targetRotation, RotationSpeed);
                // BulletLocator.transform.localRotation = Quaternion.Euler(BulletLocator.transform.localRotation.x, 0f,0f);
                Quaternion provvisorio = BulletLocator.transform.localRotation;
                provvisorio = Quaternion.Slerp(BulletLocator.transform.localRotation, targetRotation, RotationSpeed);
                provvisorio.y = 0;
                provvisorio.z = 0;
                BulletLocator.transform.localRotation = provvisorio;
            }
        }

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
    }
}
