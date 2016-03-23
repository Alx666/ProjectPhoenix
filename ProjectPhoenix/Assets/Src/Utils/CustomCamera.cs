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
        Vector3 newTarget;

        if (!cameraPositionIsSaved)
        {
            savedCameraPosition = Camera.transform.position;
            cameraPositionIsSaved = true;
        }

        if (Input.GetKey(key))
        {
            newTarget = targetAimed;

            distanceFromTarget = rB.transform.position - newTarget;

            Camera.transform.position = new Vector3((Offset.x + distanceFromTarget.normalized.x * (distanceFromTarget.magnitude / 10)), Offset.y, (Offset.z + distanceFromTarget.normalized.z * (distanceFromTarget.magnitude / 10)));
        }

        if (Input.GetKeyUp(key))
        {
            Camera.transform.position = savedCameraPosition;
            cameraPositionIsSaved = false;
        }
    }
}
