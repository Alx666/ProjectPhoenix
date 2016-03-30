using UnityEngine;
using System.Collections;

public class CustomCamera 
{
    Camera Camera;
    Vector3 Offset;
    Rigidbody rB;
    Vector3 savedCameraPosition;
    bool cameraPositionIsSaved;

    public CustomCamera(Camera myCamera, Rigidbody target, Vector3 offset)
    {
        Camera = myCamera;
        rB = target;
        Offset = offset;
    }

    public void ZoomOnTarget(float MinOffset, float MaxOffset)
    {
        Camera.transform.position = rB.transform.position + Offset;

        if (rB.velocity.magnitude * Time.deltaTime >= 0f)
        {
            Offset = new Vector3(Offset.x, Mathf.Lerp(Offset.y, rB.velocity.magnitude, Time.deltaTime), Offset.z);
        }
        if (rB.velocity.magnitude * Time.deltaTime < 0f)
        {
            Offset = new Vector3(Offset.x, Mathf.Lerp(MinOffset, rB.velocity.magnitude, Time.deltaTime), Offset.z);
        }
        if (Offset.y < MinOffset)
        {
            Offset = new Vector3(Offset.x, MinOffset, Offset.z);
        }
    }

    public void AimHelper(Vector3 targetAimed, KeyCode key)
    {
        Vector3 distanceFromTarget;
        RaycastHit ray;

        if (!cameraPositionIsSaved && Input.GetKey(key))
        {
            savedCameraPosition = Camera.transform.position;
            cameraPositionIsSaved = true;
        }

        if (Input.GetKey(key))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out ray))
            {
                distanceFromTarget = ray.point - rB.transform.position;
                Camera.transform.position = Offset + distanceFromTarget;
            }
        }

        if (Input.GetKeyUp(key))
        {
            Camera.transform.position = savedCameraPosition;
            cameraPositionIsSaved = false;
        }
    }
}
